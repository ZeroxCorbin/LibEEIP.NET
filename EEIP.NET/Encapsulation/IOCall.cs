namespace Sres.Net.EEIP.Encapsulation
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Sres.Net.EEIP.CIP.IO;
    using Sres.Net.EEIP.Data;

    public class IOCall :
        CIP.IO.IOCall
    {
        public IOCall(IPAddress address, ForwardOpenRequest forwardOpenRequest, UnconnectedMessageManagerReply forwardOpenReply) :
            base(
                forwardOpenRequest,
                CreateForwardOpenResponse(forwardOpenReply))
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            OutputEndPoint = GetOutputEndPoint(forwardOpenReply);
        }

        private static ForwardOpenResponse CreateForwardOpenResponse(UnconnectedMessageManagerReply forwardOpenReply)
        {
            if (forwardOpenReply is null)
                throw new ArgumentNullException(nameof(forwardOpenReply));
            return new ForwardOpenResponse(forwardOpenReply.Value.Data);
        }

        private IPEndPoint GetOutputEndPoint(UnconnectedMessageManagerReply forwardOpenReply)
        {
            // Socket Address Item T->O
            var socketAddress = forwardOpenReply.CommonPacket.GetSocketAddress(SocketAddressItemType.TargetToOriginator);
            return new(
                socketAddress?.Value.Address ?? IPAddress.Any,
                socketAddress?.Value.Port ?? ForwardOpenRequest.Target.ConnectionToOriginator.Port);
        }

        /// <summary>
        /// Starts IO data transfer
        /// </summary>
        public void Start()
        {
            // Open receiving UDP Port
            var originatorToTargetConnection = ForwardOpenRequest.Originator.ConnectionToTarget;
            var receivingEndPoint = new IPEndPoint(IPAddress.Any, OutputEndPoint.Port);
            receivingClient = new UdpClient(receivingEndPoint) { ExclusiveAddressUse = false };
            if (originatorToTargetConnection.Type == ConnectionType.Multicast &&
                OutputEndPoint.Address != IPAddress.Any)
            {
                receivingClient.JoinMulticastGroup(OutputEndPoint.Address);
            }

            // start sending input
            sendingThread = new Thread(SendInput);
            sendingThread.Start();

            // receive first output
            var state = new UdpState
            {
                Client = receivingClient,
                EndPoint = receivingEndPoint
            };
            var asyncResult = receivingClient.BeginReceive(new AsyncCallback(ReceiveOutputClass1), state);
        }

        #region Input

        public IPAddress Address { get; }

        private void SendInput()
        {
            using var client = new UdpClient() { ExclusiveAddressUse = false };
            var endPointSend = new IPEndPoint(Address, ForwardOpenRequest.Target.ConnectionToOriginator.Port);
            var realTimeFormat = ForwardOpenRequest.Originator.ConnectionToTarget.RealTimeFormat;
            var data = InputData.Data;
            int packageRate = (int)ForwardOpenResponse.TargetToOriginatorActualPacketRate / 1000;
            var connectionId = ForwardOpenResponse.OriginatorToTargetConnectionId.AsByteable();
            uint sequenceCount = 0;
            var o_t_IOData = new byte[564];

            while (send)
            {
                OnInputDataSending(InputData);

                int index = 0;
                //---------------Item count
                o_t_IOData[index++] = 2;
                o_t_IOData[index++] = 0;
                //---------------Item count

                //---------------Type ID
                o_t_IOData[index++] = 0x02;
                o_t_IOData[index++] = 0x80;
                //---------------Type ID

                //---------------Length
                o_t_IOData[index++] = 0x08;
                o_t_IOData[index++] = 0x00;
                //---------------Length

                //---------------connection ID
                connectionId.ToBytes(o_t_IOData, ref index);
                //---------------connection ID

                //---------------sequence count
                sequenceCount++;
                sequenceCount.AsByteable().ToBytes(o_t_IOData, ref index);
                //---------------sequence count            

                //---------------Type ID
                o_t_IOData[index++] = 0xB1;
                o_t_IOData[index++] = 0x00;
                //---------------Type ID

                ushort headerOffset = 0;
                if (realTimeFormat == ConnectionRealTimeFormat.Header32Bit)
                    headerOffset = 4;
                if (realTimeFormat == ConnectionRealTimeFormat.Heartbeat)
                    headerOffset = 0;
                ushort o_t_Length = (ushort)(data.Length + headerOffset + 2);   //Modeless and zero Length

                index = 16;

                //---------------Length
                o_t_IOData[index++] = (byte)o_t_Length;
                o_t_IOData[index++] = (byte)(o_t_Length >> 8);
                //---------------Length

                //---------------Sequence count
                sequence++;
                if (realTimeFormat != ConnectionRealTimeFormat.Heartbeat)
                {
                    o_t_IOData[index++] = (byte)sequence;
                    o_t_IOData[index++] = (byte)(sequence >> 8);
                }
                //---------------Sequence count

                if (realTimeFormat == ConnectionRealTimeFormat.Header32Bit)
                {
                    o_t_IOData[index++] = (byte)1;
                    o_t_IOData[index++] = (byte)0;
                    o_t_IOData[index++] = (byte)0;
                    o_t_IOData[index++] = (byte)0;

                }

                //---------------Write data
                for (int i = 0; i < data.Length; i++)
                    o_t_IOData[20 + headerOffset + i] = (byte)data[i];
                //---------------Write data

                client.Send(o_t_IOData, data.Length + 20 + headerOffset, endPointSend);

                InputData.LastDataTransferTime = DateTime.Now;

                OnInputDataSent(InputData);

                Thread.Sleep(packageRate);
            }
        }

        internal void StopSendingInput() => send = false;

        private Thread sendingThread;
        private bool send = true;
        private int sequence = 0;

        #endregion

        #region Output

        public IPEndPoint OutputEndPoint { get; }

        private void ReceiveOutputClass1(IAsyncResult ar)
        {
            if (!receive)
                return;
            var realTimeFormat = ForwardOpenRequest.Target.ConnectionToOriginator.RealTimeFormat;
            var data = OutputData.Data;
            var state = (UdpState)ar.AsyncState;
            state.Client.BeginReceive(new AsyncCallback(ReceiveOutputClass1), state);
            var endPoint = state.EndPoint;
            byte[] receiveBytes = state.Client.EndReceive(ar, ref endPoint);
            // EndReceive worked and we have received data and remote endpoint
            if (receiveBytes.Length > 20)
            {
                //Get the connection ID
                uint connectionID = (uint)(receiveBytes[6] | receiveBytes[7] << 8 | receiveBytes[8] << 16 | receiveBytes[9] << 24);

                if (connectionID == ForwardOpenResponse.TargetToOriginatorConnectionId)
                {
                    ushort headerOffset = 0;
                    if (realTimeFormat == ConnectionRealTimeFormat.Header32Bit)
                        headerOffset = 4;
                    if (realTimeFormat == ConnectionRealTimeFormat.Heartbeat)
                        headerOffset = 0;
                    for (int i = 0; i < receiveBytes.Length - 20 - headerOffset; i++)
                    {
                        data[i] = receiveBytes[20 + i + headerOffset];
                    }
                }
            }
            OutputData.LastDataTransferTime = DateTime.Now;
            OnOutputDataReceived(OutputData);
        }

        private UdpClient receivingClient;
        private bool receive = true;

        #endregion

        public ForwardCloseRequest CreateForwardCloseRequest() => new(
            ForwardOpenRequest.GetConnectionPath(),
            ForwardOpenResponse);

        public override void Dispose()
        {
            //First stop the thread which sends data
            StopSendingInput();
            // Close receiving Socket
            receive = false;
            receivingClient.Dispose();

            base.Dispose();
        }
    }
}
