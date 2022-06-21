namespace Sres.Net.EEIP.Encapsulation
{
    using Sres.Net.EEIP.CIP.ObjectLibrary;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Unconnected Message Manager (UCMM) (EtherNet/IP specification 6-2) reply (EtherNet/IP specification 3-2.1)
    /// </summary>
    public record UnconnectedMessageManagerReply :
        UnconnectedMessageManagerMessage
    {
        public UnconnectedMessageManagerReply(Encapsulation reply) :
            base(reply)
            => Value = new MessageRouterResponse(CommonPacket.Data.Data.ToBytesReadOnly());

        public MessageRouterResponse Value { get; }
    }
}