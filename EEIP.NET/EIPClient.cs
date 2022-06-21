using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Sres.Net.EEIP.CIP;
using Sres.Net.EEIP.CIP.IO;
using Sres.Net.EEIP.CIP.ObjectLibrary;
using Sres.Net.EEIP.Data;
using Sres.Net.EEIP.Encapsulation;

namespace Sres.Net.EEIP
{
    /// <summary>
    /// EtherNet/IP client
    /// </summary>
    public class EEIPClient :
        IDisposable
    {
        public EEIPClient()
        {
            identity = new Lazy<Identity>(() => new Identity(this));
            assembly = new Lazy<Assembly>(() => new Assembly(this));
            messageRouter = new Lazy<MessageRouter>(() => new MessageRouter(this));
            tcpIpInterface = new Lazy<TcpIpInterface>(() => new TcpIpInterface(this));
        }

        #region ListIdentity

        /// <summary>
        /// Autodiscovers EtherNet/IP device on network and uses it to set <see cref="Address"/> and <see cref="Port"/>
        /// </summary>
        /// <param name="filter">Optional identity filter. null means any device.</param>
        /// <returns>First device identity satisfying <paramref name="filter"/></returns>
        /// <exception cref="Exception">No device found</exception>
        public IdentityItem AutoDiscover(Func<IdentityItem, bool> filter = null)
        {
            var identities = ListIdentity();
            var identity = filter is null ?
                identities.FirstOrDefault() :
                identities.FirstOrDefault(filter);
            if (identity is null)
                throw new Exception("No suitable EtherNet/IP device found on network");
            Address = identity.SocketAddress.Address;
            Port = identity.SocketAddress.Port;
            return identity;
        }

        /// <summary>
        /// List and identify potential targets. This command shall be sent as braodcast massage using UDP.
        /// </summary>
        /// <returns>The received informations from all devices</returns>	
        public IReadOnlyList<IdentityItem> ListIdentity()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!(
                    ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    continue;
                }
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    IPAddress mask = ip.IPv4Mask;
                    IPAddress address = ip.Address;

                    string multicastAddress = (address.GetAddressBytes()[0] | (~(mask.GetAddressBytes()[0])) & 0xFF).ToString() + "." + (address.GetAddressBytes()[1] | (~(mask.GetAddressBytes()[1])) & 0xFF).ToString() + "." + (address.GetAddressBytes()[2] | (~(mask.GetAddressBytes()[2])) & 0xFF).ToString() + "." + (address.GetAddressBytes()[3] | (~(mask.GetAddressBytes()[3])) & 0xFF).ToString();

                    var sendData = new byte[24];
                    sendData[0] = 0x63;               //Command for "ListIdentity"
                    var udpClient = new UdpClient();
                    var endPoint = new IPEndPoint(IPAddress.Parse(multicastAddress), 44818);
                    udpClient.Send(sendData, sendData.Length, endPoint);

                    var state = new UdpState
                    {
                        EndPoint = endPoint,
                        Client = udpClient
                    };

                    var asyncResult = udpClient.BeginReceive(new AsyncCallback(ReceiveIdentity), state);

                    System.Threading.Thread.Sleep(1000);
                }
            }
            return identityList;
        }

        private void ReceiveIdentity(IAsyncResult ar)
        {
            lock (this)
            {
                var state = (UdpState)ar.AsyncState;
                var endPoint = state.EndPoint;
                byte[] receiveBytes = state.Client.EndReceive(ar, ref endPoint);
                // EndReceive worked and we have received data and remote endpoint
                if (receiveBytes.Length > 0)
                {
                    var command = (Command)Convert.ToUInt16(
                        receiveBytes[0]
                        | (receiveBytes[1] << 8));
                    if (command == Command.ListIdentity)
                    {
                        identityList.Add(IdentityItem.From(24, receiveBytes));
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
        /// Sends <see cref="Command.RegisterSession"/> to a target to initiate TCP session
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
            var endPoint = new IPEndPoint(ipAddress, tcpPort.Value);
            tcpClient = new TcpClient(endPoint);
            tcpStream = tcpClient.GetStream();

            var reply = Call(RegisterSessionRequest.Instance);
            return SessionHandle = reply.SessionHandle;
        }

        /// <summary>
        /// Sends <see cref="Command.UnRegisterSession"/> to a target to terminate TCP session
        /// </summary> 
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

            byte[] replyBuffer = new byte[564];
            int replyLength = tcpStream.Read(replyBuffer, 0, replyBuffer.Length);
            var reply = new Encapsulation.Encapsulation(replyBuffer);
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

        public Encapsulation.IOCall ForwardOpen(ForwardOpenRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            lock (ioCalls)
            {
                AutodetectDataSize(request);

                // Add Socket Address Item O->T
                var originatorSocketAddress = request.GetOriginatorSocketAddress(Address);

                var reply = Call(request, originatorSocketAddress);

                var result = new Encapsulation.IOCall(Address, request, reply);
                ioCalls.Add(result);
                return result;
            }
        }

        private void AutodetectDataSize(ForwardOpenRequest request)
        {
            request.Originator.ConnectionToTarget.DataSize ??= GetDataSize(request.Originator.ConnectionToTarget.InputPath);
            request.Target.ConnectionToOriginator.DataSize ??= GetDataSize(request.Target.ConnectionToOriginator.OutputPath);
        }

        /// <summary>
        /// Detects response data length with <see cref="GetAttributeSingle"/>
        /// </summary>
        /// <param name="path">Data path</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        public ushort GetDataSize(EPath path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            var result = (ushort)this.GetAttributeSingle(path).Count;
            return result;
        }

        public void ForwardClose()
        {
            lock (ioCalls)
            {
                foreach (var ioCall in ioCalls)
                    ForwardClose(ioCall);
                ioCalls.Clear();
            }
        }

        public void ForwardClose(Encapsulation.IOCall call)
        {
            if (call is null)
                throw new ArgumentNullException(nameof(call));
            lock (ioCalls)
            {
                call.StopSendingInput();
                try
                {
                    var request = call.CreateForwardCloseRequest();
                    var reply = Call(request);
                }
                catch
                {
                    // Handle Exception to allow Forward close if the connection was closed by the Remote Device before
                }
                call.Dispose();
                ioCalls.Remove(call);
            }
        }

        private readonly List<Encapsulation.IOCall> ioCalls = new();

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

        public void Dispose()
        {
            ForwardClose();
            UnRegisterSession();
        }
    }
}
