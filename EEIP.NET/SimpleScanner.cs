using Sres.Net.EEIP.CIP.ObjectLibrary;
using Sres.Net.EEIP.Data;
using Sres.Net.EEIP.Encapsulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Sres.Net.EEIP
{
    public class SimpleScanner
    {
        /// <summary>
        /// IP address of device for explicit messaging
        /// </summary>
        /// <value>Default: null</value>
        public IPAddress Address { get; set; }
        /// <summary>
        /// TCP port of device for explicit messaging
        /// </summary>
        /// <value>Default: <see cref="DefaultPort"/></value>
        public ushort BindPort { get; set; } = DefaultPort;
        /// <summary>
        /// Default <see cref="Port"/>: 0xAF12 = 44818
        /// </summary>
        public const ushort DefaultPort = 44818;

        public static readonly TimeSpan DefaultListIdentityWaitTime = TimeSpan.FromSeconds(3);

        public IEnumerable<IdentityItem> ListIdentities(string bindIpAddress, Func<IdentityItem, bool> where, int bindPort = DefaultPort, TimeSpan time = default ) =>
            where is null ? ListIdentities(bindIpAddress, bindPort, time) : ListIdentities(bindIpAddress, bindPort, time).Where(where);

        /// <summary>
        /// List and identify potential targets.
        /// This command is sent as broadcast message using UDP.
        /// </summary>
        /// <param name="time">Time to wait for receiving responses to broadcasted request. Value &lt;= 0 means <see cref="DefaultListIdentityWaitTime"/>.</param>
        /// <returns>The received informations from all devices</returns>	
        public IEnumerable<IdentityItem> ListIdentities(string bindIpAddress, int bindPort = DefaultPort, TimeSpan time = default)
        {
            BindPort = (ushort)bindPort;
            Address = IPAddress.Parse(bindIpAddress);
            identityList.Clear();

            if (time <= TimeSpan.Zero)
                time = DefaultListIdentityWaitTime;
            var listIdentity = new Encapsulation.Encapsulation(Command.ListIdentity).ToBytes();
            var receiveEndPoint = new IPEndPoint(Address, BindPort);
            using var udpClient = new UdpClient(receiveEndPoint);
            var state = new UdpState
            {
                Client = udpClient,
                EndPoint = receiveEndPoint
            };
            udpClient.BeginReceive(new AsyncCallback(ReceiveIdentity), state);
            var sendEndPoint = new IPEndPoint(IPAddress.Broadcast, BindPort);
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
                        int index = 8;
                        var soc = new Encapsulation.SocketAddress(response.Data.ToBytes(), ref index);
                        index = 24;
                        var identity = new IdentityItem(new IdentityInstance(response.Data.ToBytes(), ref index), soc);

                        if (identity != null)
                            identityList.Add(identity);
                    }
                }
                var asyncResult = state.Client.BeginReceive(new AsyncCallback(ReceiveIdentity), state);
            }

        }

        private List<IdentityItem> identityList = new();
    }
}
