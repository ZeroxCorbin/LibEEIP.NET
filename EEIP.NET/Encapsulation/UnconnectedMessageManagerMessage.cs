namespace Sres.Net.EEIP.Encapsulation
{
    using System;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Unconnected Message Manager (UCMM) (EtherNet/IP specification 6-2) request/reply (EtherNet/IP specification 3-2.1)
    /// </summary>
    public abstract record UnconnectedMessageManagerMessage :
        Encapsulation
    {
        protected UnconnectedMessageManagerMessage(CommonPacket commonPacket) :
            base(
                Command.SendRRData,
                Prefix.Concat(commonPacket))
            => CommonPacket = commonPacket;

        protected UnconnectedMessageManagerMessage(Encapsulation reply) :
            base(reply)
        {
            if (reply is null)
                throw new ArgumentNullException(nameof(reply));
            CommonPacket = GetCommonPacket(Prefix.ByteCount);
        }

        public CommonPacket CommonPacket { get; }

        private static readonly Bytes Prefix = new Bytes(
            // CIP interface handle
            0, 0, 0, 0,
            // timeout realized by TCP
            0, 0);
    }
}