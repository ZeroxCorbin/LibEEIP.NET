namespace Sres.Net.EEIP.Encapsulation
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Sres.Net.EEIP.CIP.IO;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// IO context for Class 1 implicit messaging
    /// </summary>
    public class IOContext :
        CIP.IO.IOContext
    {
        #region Init

        internal IOContext(bool backwardsCompatible, IPAddress targetAddress, ForwardOpenRequest forwardOpenRequest, UnconnectedMessageManagerReply forwardOpenReply) :
            base(
                backwardsCompatible,
                forwardOpenRequest,
                CreateForwardOpenResponse(forwardOpenReply))
        {
            if (targetAddress is null)
                throw new ArgumentNullException(nameof(targetAddress));
            TargetToOriginatorConnection.SocketAddress = forwardOpenReply.CommonPacket.GetSocketAddress(SocketAddressItemType.TargetToOriginator)?.Value.EndPoint;
            if (TargetToOriginatorConnection.SocketAddress != null)
                Target.Port = (ushort)TargetToOriginatorConnection.SocketAddress.Port;
            TargetEndPoint = new(targetAddress, Target.Port);
            if (!TargetToOriginatorConnection.IsNull)
                InitReceiveDataFromTarget();
            if (!OriginatorToTargetConnection.IsNull)
                InitSendDataToTarget(backwardsCompatible);
            if (!TargetToOriginatorConnection.IsNull)
                BeginReceiveDataFromTarget();
        }

        public IOContext(IPAddress targetAddress, ForwardOpenRequest forwardOpenRequest, UnconnectedMessageManagerReply forwardOpenReply) :
            this(false, targetAddress, forwardOpenRequest, forwardOpenReply)
        { }

        private static ForwardOpenResponse CreateForwardOpenResponse(UnconnectedMessageManagerReply forwardOpenReply)
        {
            if (forwardOpenReply is null)
                throw new ArgumentNullException(nameof(forwardOpenReply));
            return new ForwardOpenResponse(forwardOpenReply.Value.Data);
        }

        #endregion

        #region Sending

        /// <summary>
        /// Target IP end point
        /// </summary>
        public IPEndPoint TargetEndPoint { get; }

        protected override void InitSendDataToTarget(bool backwardsCompatible)
        {
            sendingClient = new UdpClient()
            {
                ExclusiveAddressUse = false
            };
            sendingData = new byte[564];
            base.InitSendDataToTarget(backwardsCompatible);
        }

        protected override void DoSendDataToTarget()
        {
            int count = PrepareDataToTarget();
            sendingClient.Send(sendingData, count, TargetEndPoint);
        }

        private int PrepareDataToTarget()
        {
            var realTimeFormat = OriginatorToTargetConnection.RealTimeFormat;
            var data = OriginatorToTargetConnection.Data;
            var connectionId = ForwardOpenResponse.OriginatorToTargetConnectionId.AsByteable();
            int index = 0;

            //---------------Item count
            sendingData[index++] = 2;
            sendingData[index++] = 0;
            //---------------Item count

            //---------------Type ID
            sendingData[index++] = 0x02;
            sendingData[index++] = 0x80;
            //---------------Type ID

            //---------------Length
            sendingData[index++] = 0x08;
            sendingData[index++] = 0x00;
            //---------------Length

            //---------------connection ID
            connectionId.ToBytes(sendingData, ref index);
            //---------------connection ID

            //---------------sequence count
            sendingSequenceCount++;
            sendingSequenceCount.ToBytes(sendingData, ref index);
            //---------------sequence count            

            //---------------Type ID
            sendingData[index++] = 0xB1;
            sendingData[index++] = 0x00;
            //---------------Type ID

            byte headerOffset = 0;
            if (realTimeFormat == ConnectionRealTimeFormat.Header32Bit)
                headerOffset = 4;
            if (realTimeFormat == ConnectionRealTimeFormat.Heartbeat)
                headerOffset = 0;
            ushort o_t_Length = (ushort)(data.Length + headerOffset + 2);   //Modeless and zero Length

            index = 16;

            //---------------Length
            sendingData[index++] = (byte)o_t_Length;
            sendingData[index++] = (byte)(o_t_Length >> 8);
            //---------------Length

            //---------------Sequence count
            if (realTimeFormat != ConnectionRealTimeFormat.Heartbeat)
            {
                sendingSequenceCountRealTime++;
                sendingSequenceCountRealTime.ToBytes(sendingData, ref index);
            }
            //---------------Sequence count

            if (realTimeFormat == ConnectionRealTimeFormat.Header32Bit)
            {
                sendingData[index++] = 1;
                sendingData[index++] = 0;
                sendingData[index++] = 0;
                sendingData[index++] = 0;

            }

            //---------------Write data
            for (int i = 0; i < data.Length; i++)
                sendingData[20 + headerOffset + i] = data[i];
            //---------------Write data

            return data.Length + 20 + headerOffset;
        }

        private UdpClient sendingClient;
        private byte[] sendingData;
        private ushort sendingSequenceCountRealTime;
        private uint sendingSequenceCount;

        #endregion

        #region Receiving

        // Open receiving UDP Port
        private void InitReceiveDataFromTarget()
        {
            receivingEndPoint = new IPEndPoint(IPAddress.Any, Originator.Port);
            receivingClient = new UdpClient(receivingEndPoint)
            {
                ExclusiveAddressUse = false
            };
            if (TargetToOriginatorConnection.Type == ConnectionType.Multicast &&
                TargetToOriginatorConnection.SocketAddress != null)
            {
                receivingClient.JoinMulticastGroup(TargetToOriginatorConnection.SocketAddress.Address);
            }
        }

        protected override void DoBeginReceiveDataFromTarget()
            => receivingClient.BeginReceive(EndReceiveDataFromTarget, null);

        protected override bool DoEndReceiveDataFromTarget(IAsyncResult result)
        {
            var endPoint = receivingEndPoint;
            var bytes = receivingClient.EndReceive(result, ref endPoint);
            // EndReceive worked and we have received data and remote endpoint
            return PrepareDataFromTarget(bytes);
        }

        private bool PrepareDataFromTarget(byte[] bytes)
        {
            if (bytes.Length <= 20)
                return false;
            // check connection ID
            int index = 6;
            var connectionId = bytes.ToUint(ref index);
            if (connectionId != ForwardOpenResponse.TargetToOriginatorConnectionId)
                return false;
            var realTimeFormat = TargetToOriginatorConnection.RealTimeFormat;
            var data = TargetToOriginatorConnection.Data;
            ushort headerOffset = 0;
            if (realTimeFormat == ConnectionRealTimeFormat.Header32Bit)
                headerOffset = 4;
            if (realTimeFormat == ConnectionRealTimeFormat.Heartbeat)
                headerOffset = 0;
            for (int i = 0; i < bytes.Length - 20 - headerOffset; i++)
            {
                data[i] = bytes[20 + i + headerOffset];
            }
            return true;
        }

        private IPEndPoint receivingEndPoint;
        private UdpClient receivingClient;

        #endregion

        public override void Dispose()
        {
            sendingClient?.Dispose();
            receivingClient?.Dispose();
            base.Dispose();
        }

        public override void FinishSendDataToTarget()
        {
            throw new NotImplementedException();
        }
    }
}
