namespace Sres.Net.EEIP.Encapsulation
{
    using Sres.Net.EEIP.CIP.ObjectLibrary;

    /// <summary>
    /// Unconnected Message Manager (UCMM) (EtherNet/IP specification 6-2) request (EtherNet/IP specification 3-2.1)
    /// </summary>
    public record UnconnectedMessageManagerRequest :
        UnconnectedMessageManagerMessage
    {
        public UnconnectedMessageManagerRequest(MessageRouterRequest request, params Item[] items) :
            base(new CommonPacket(
                new DataItem(DataItemType.Unconnected, request),
                items))
            => Value = request;

        public MessageRouterRequest Value { get; }

    }
}