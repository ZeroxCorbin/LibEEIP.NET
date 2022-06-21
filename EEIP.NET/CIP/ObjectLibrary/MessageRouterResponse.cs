namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Message router response. CIP specification 2-4.2.
    /// </summary>
    public record MessageRouterResponse
    {
        public MessageRouterResponse(IReadOnlyList<byte> bytes)
        {
            bytes.ValidateEnoughBytes(4, nameof(MessageRouterResponse) + " header");
            int index = 0;
            Service = bytes[index++];
            index++; // Reserved
            Status = bytes[index++];
            var additionalStatusSize = bytes[index++];
            if (additionalStatusSize > 0)
                ExtendedStatuses = new ByteToUshortList(bytes.Segment(ref index, additionalStatusSize));
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
    }
}