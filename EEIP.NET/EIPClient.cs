using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Sres.Net.EEIP.CIP;
using Sres.Net.EEIP.CIP.IO;
using Sres.Net.EEIP.CIP.ObjectLibrary;
using Sres.Net.EEIP.CIP.Path;
using Sres.Net.EEIP.Data;
using Sres.Net.EEIP.Encapsulation;
using Sres.Net.EEIP.ObjectLibrary;

namespace Sres.Net.EEIP
{
    /// <summary>
    /// EtherNet/IP client (implementation version >= 2.x) according to specification:
    /// The CIP Networks Library (https://www.odva.org/subscriptions-services/specifications/)
    /// <list type="bullet">
    /// <item>Volume 1 - Common Industrial Protocol - 2007</item>
    /// <item>Volume 2 - EtherNetIP Adaptation of CIP - 2007</item>
    /// </list>
    /// </summary>
    public class EIPClient :
        IDisposable
    {
        public EIPClient()
        {
            identity = new Lazy<Identity>(() => new Identity(this));
            assembly = new Lazy<Assembly>(() => new Assembly(this));
            messageRouter = new Lazy<MessageRouter>(() => new MessageRouter(this));
            tcpIpInterface = new Lazy<TcpIpInterface>(() => new TcpIpInterface(this));
        }

        #region ListIdentity

        /// <summary>
        /// Autodiscovers EtherNet/IP device on network (using <see cref="ListIdentity"/>) and uses it to set <see cref="Address"/> and <see cref="Port"/>
        /// </summary>
        /// <param name="filter">Optional identity filter. null means any device.</param>
        /// <param name="time">Time to wait for receiving responses to broadcasted request. Value &lt;= 0 means <see cref="DefaultListIdentityWaitTime"/>.</param>
        /// <returns>First device identity satisfying <paramref name="filter"/></returns>
        /// <exception cref="Exception">No device found</exception>
        public IdentityItem AutoDiscover(Func<IdentityItem, bool> filter = null, TimeSpan time = default)
        {
            var identities = ListIdentity(time);
            var identity = filter is null ?
                identities.FirstOrDefault() :
                identities.FirstOrDefault(filter);
            if (identity is null)
                throw new Exception("No suitable EtherNet/IP device found on network");
            Address = identity.SocketAddress.Address;
            Port = identity.SocketAddress.Port;
            return identity;
        }

        public static readonly TimeSpan DefaultListIdentityWaitTime = TimeSpan.FromSeconds(1);

        /// <summary>
        /// List and identify potential targets.
        /// This command is sent as broadcast message using UDP.
        /// </summary>
        /// <param name="time">Time to wait for receiving responses to broadcasted request. Value &lt;= 0 means <see cref="DefaultListIdentityWaitTime"/>.</param>
        /// <returns>The received informations from all devices</returns>	
        public IReadOnlyList<IdentityItem> ListIdentity(TimeSpan time = default)
        {
            if (time <= TimeSpan.Zero)
                time = DefaultListIdentityWaitTime;
            var listIdentity = new Encapsulation.Encapsulation(Command.ListIdentity).ToBytes();
            const ushort port = DefaultPort;
            var receiveEndPoint = new IPEndPoint(IPAddress.Any, port);
            using var udpClient = new UdpClient(receiveEndPoint);
            var state = new UdpState
            {
                Client = udpClient,
                EndPoint = receiveEndPoint
            };
            udpClient.BeginReceive(new AsyncCallback(ReceiveIdentity), state);
            var sendEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            udpClient.Send(listIdentity, listIdentity.Length, sendEndPoint);
            System.Threading.Thread.Sleep(time);
            lock (identityList)
            {
                state.Client = null;
                return identityList;
            }
        }

        private void ReceiveIdentity(IAsyncResult ar)
        {
            lock (identityList)
            {
                var state = (UdpState)ar.AsyncState;
                if (state.Client is null)
                    return;
                var endPoint = state.EndPoint;
                byte[] bytes = state.Client.EndReceive(ar, ref endPoint);
                // EndReceive worked and we have received data and remote endpoint
                if (bytes.Length > Encapsulation.Encapsulation.MinByteCount)
                {
                    var response = new Encapsulation.Encapsulation(bytes);
                    if (response.Command == Command.ListIdentity)
                    {
                        var identity = response.GetCommonPacket().IdentityItem;
                        if (identity != null)
                            identityList.Add(identity);
                    }
                }
                var asyncResult = state.Client.BeginReceive(new AsyncCallback(ReceiveIdentity), state);
            }

        }

        private List<IdentityItem> identityList = new();

        #endregion

        #region TCP Session

        /// <summary>
        /// IP address of device for explicit messaging
        /// </summary>
        /// <value>Default: null</value>
        public IPAddress Address { get; set; }
        /// <summary>
        /// TCP port of device for explicit messaging
        /// </summary>
        /// <value>Default: <see cref="DefaultPort"/></value>
        public ushort Port { get; set; } = DefaultPort;
        /// <summary>
        /// Default <see cref="Port"/>: 0xAF12 = 44818
        /// </summary>
        public const ushort DefaultPort = 0xAF12;
        /// <summary>
        /// Session handle from <see cref="RegisterSession"/>
        /// </summary>
        /// <value><see cref="RegisterSession"/> result, otherwise 0</value>
        public uint SessionHandle { get; private set; }

        /// <summary>
        /// Sends <see cref="Command.RegisterSession"/> to a target to initiate TCP/IP session
        /// </summary>
        /// <param name="ipAddress">IP address of the target device. null means <see cref="Address"/></param> 
        /// <param name="tcpPort">TCP port of the target device. null means <see cref="Port"/>.</param> 
        /// <returns>Session Handle</returns>	
        public uint RegisterSession(IPAddress ipAddress = null, ushort? tcpPort = null)
        {
            if (SessionHandle != 0)
                return SessionHandle;
            if (ipAddress is null)
                ipAddress = Address;
            if (ipAddress is null)
                throw new ArgumentNullException(nameof(ipAddress));
            this.Address = ipAddress;
            if (tcpPort is null)
                tcpPort = Port;
            else
                Port = tcpPort.Value;
            tcpClient = new TcpClient();
            var endPoint = new IPEndPoint(ipAddress, tcpPort.Value);
            tcpClient.Connect(endPoint);
            tcpStream = tcpClient.GetStream();

            var reply = Call(RegisterSessionRequest.Instance);
            return SessionHandle = reply.SessionHandle;
        }

        /// <summary>
        /// Sends <see cref="Command.UnRegisterSession"/> to a target to terminate TCP/IP session
        /// </summary> 
        /// <remarks>Called by <see cref="Close"/></remarks>
        public void UnRegisterSession()
        {
            if (SessionHandle == 0)
                return;
            var request = new UnregisterSessionRequest(SessionHandle);
            try
            {
                Call(request);
            }
            catch (Exception)
            {
                //Handle Exception to allow to Close the Stream if the connection was closed by Remote Device
            }
            tcpClient.Dispose();
            tcpStream.Dispose();
            SessionHandle = 0;
        }

        public Encapsulation.Encapsulation Call(Encapsulation.Encapsulation request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            request.SessionHandle = SessionHandle;

            byte[] requestBytes = request.ToBytes();
            tcpStream.Write(requestBytes, 0, requestBytes.Length);

            var replyBuffer = new byte[564];
            int replyLength = tcpStream.Read(replyBuffer, 0, replyBuffer.Length);
            var replyBytes = replyBuffer.Segment(count: replyLength);
            var reply = new Encapsulation.Encapsulation(replyBytes);
            if (reply.Status != EncapsulationStatus.Success)
                throw new StatusException(reply.Status);
            return reply;
        }

        public IReadOnlyList<byte> Call(byte service, EPath path, params byte[] data)
            => Call(new MessageRouterRequest(service, path, new Bytes(data))).Value.Data;

        public UnconnectedMessageManagerReply Call(MessageRouterRequest request, params Item[] items)
        {
            // If a session is not registered, try to register a session with the predefined IP-Address and Port
            if (SessionHandle == 0)
                this.RegisterSession();
            var reply = Call(new UnconnectedMessageManagerRequest(request, items));
            var replyPacket = new UnconnectedMessageManagerReply(reply);
            var error = GeneralException.From(replyPacket.Value);
            if (error != null)
                throw error;
            return replyPacket;
        }

        public IReadOnlyList<byte> GetAttributeSingle(EPath path)
            => Call((byte)CommonServices.Get_Attribute_Single, path);

        public IReadOnlyList<byte> SetAttributeSingle(EPath path, params byte[] value)
            => Call((byte)CommonServices.Set_Attribute_Single, path, value);

        /// <summary>
        /// Implementation of Common Service "Get_Attribute_All" - Service Code: 0x01
        /// </summary>
        /// <param name="path">Path with <see cref="EPath.ClassId"/> or <see cref="EPath.InstanceId"/></param>
        public IReadOnlyList<byte> GetAttributeAll(EPath path)
            => Call((byte)CommonServices.Get_Attributes_All, path);

        private TcpClient tcpClient;
        private NetworkStream tcpStream;

        #endregion

        #region Forward Open/Close

        /// <summary>
        /// Establishes implicit IO connections with UDP/IP messaging
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>IO context which is disposed by <see cref="ForwardClose(Encapsulation.IOContext)"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is null</exception>
        public Encapsulation.IOContext ForwardOpen(ForwardOpenRequest request) => ForwardOpen(request, false);

        private protected Encapsulation.IOContext ForwardOpen(ForwardOpenRequest request, bool backwardsCompatible)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            lock (ioContexts)
            {
                EnsureDataSize(request);
                var originatorSocketAddress = request.GetOriginatorToTargetSocketAddress(Address);
                var reply = Call(request, originatorSocketAddress);

                var result = new Encapsulation.IOContext(backwardsCompatible, Address, request, reply);
                ioContexts.Add(result);
                return result;
            }
        }

        private void EnsureDataSize(ForwardOpenRequest request)
        {
            request.OriginatorToTargetConnection.DataSize ??= GetDataSize(request.OriginatorToTargetConnection.DataPath);
            request.TargetToOriginatorConnection.DataSize ??= GetDataSize(request.TargetToOriginatorConnection.DataPath);
            request.ValidateDataSize();
        }

        /// <summary>
        /// Detects response data length with <see cref="GetAttributeSingle"/> if <paramref name="path"/> is non-null, otherwise returns 0
        /// </summary>
        /// <param name="path">Data path</param>
        public ushort GetDataSize(EPath path)
        {
            var result = path is null ?
                ushort.MinValue :
                (ushort)this.GetAttributeSingle(path).Count;
            return result;
        }

        /// <summary>
        /// Closes all previous <see cref="Encapsulation.IOContext"/>s (with <see cref="ForwardClose(Encapsulation.IOContext)"/>) created with <see cref="ForwardOpen(ForwardOpenRequest)"/>
        /// </summary>
        /// <remarks>Called by <see cref="Close"/></remarks>
        public void ForwardClose()
        {
            lock (ioContexts)
            {
                foreach (var ioCall in ioContexts)
                    ForwardClose(ioCall);
                ioContexts.Clear();
            }
        }
        /// <summary>
        /// Closes <paramref name="context"/> created with <see cref="ForwardOpen(ForwardOpenRequest)"/> and disposes it
        /// </summary>
        /// <param name="context">Context to close</param>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is null</exception>
        public void ForwardClose(Encapsulation.IOContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            lock (ioContexts)
            {
                context.StopSendDataToTarget();
                try
                {
                    var request = context.CreateForwardCloseRequest();
                    var reply = Call(request);
                }
                catch
                {
                    // Handle Exception to allow Forward close if the connection was closed by the Remote Device before
                }
                context.Dispose();
                ioContexts.Remove(context);
            }
        }

        private readonly List<Encapsulation.IOContext> ioContexts = new();

        #endregion

        #region Object Library

        private readonly Lazy<Identity> identity;

        /// <summary>
        /// Implementation of the identity Object (Class Code: 0x01) - Required Object according to CIP-Specification
        /// </summary>
        public Identity Identity => identity.Value;

        private readonly Lazy<MessageRouter> messageRouter;

        /// <summary>
        /// Implementation of the Message Router Object (Class Code: 0x02) - Required Object according to CIP-Specification
        /// </summary>
        public MessageRouter MessageRouter => messageRouter.Value;

        private readonly Lazy<Assembly> assembly;

        /// <summary>
        /// Implementation of the Assembly Object (Class Code: 0x04)
        /// </summary>
        public Assembly Assembly => assembly.Value;

        private readonly Lazy<TcpIpInterface> tcpIpInterface;

        /// <summary>
        /// Implementation of the TCP/IP Object (Class Code: 0xF5) - Required Object according to CIP-Specification
        /// </summary>
        public TcpIpInterface TcpIpInterface => tcpIpInterface.Value;

        #endregion

        /// <summary>
        /// Calls <see cref="Close"/>
        /// </summary>
        public void Dispose() => Close();

        /// <summary>
        /// Closes IO contexts (<see cref="ForwardClose()"/>) and session (<see cref="UnRegisterSession"/>)
        /// </summary>
        /// <remarks>Called by <see cref="Dispose"/></remarks>
        public void Close()
        {
            ForwardClose();
            UnRegisterSession();
        }
    }
}
