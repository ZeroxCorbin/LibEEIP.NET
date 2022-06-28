namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Message router response. CIP specification 2-4.2.
    /// </summary>
    public record MessageRouterResponse :
        ByteCountBase
    {
        public MessageRouterResponse(IReadOnlyList<byte> bytes, int index = 0) :
            base(bytes, ref index)
        {
            bytes.ValidateEnoughBytes(4, nameof(MessageRouterResponse) + " header");
            Service = bytes[index++];
            index++; // Reserved
            Status = bytes[index++];
            ExtendedStatuses = bytes.ToUshortListWithByteCount(ref index);
            Data = bytes.Segment(ref index);
        }

        /// <summary>
        /// Service code
        /// </summary>
        public byte Service { get; init; }
        /// <summary>
        /// General status
        /// </summary>
        public byte Status { get; init; }
        /// <summary>
        /// Extended/Additional statuses
        /// </summary>
        public IReadOnlyList<ushort> ExtendedStatuses { get; }
        /// <summary>
        /// Data
        /// </summary>
        public IReadOnlyList<byte> Data { get; }

        public override ushort ByteCount => (ushort)(
            4 +
            ExtendedStatuses?.Count * 2 ?? 0 +
            Data?.Count ?? 0);
    }
}