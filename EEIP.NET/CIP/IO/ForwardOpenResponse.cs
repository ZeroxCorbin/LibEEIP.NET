namespace Sres.Net.EEIP.CIP.IO
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Forward open response for implicit communication. Table 3-5.16 (Vol. 1).
    /// </summary>
    public record ForwardOpenResponse
    {
        public ForwardOpenResponse(IReadOnlyList<byte> bytes)
        {
            bytes.ValidateEnoughBytes(26, nameof(ForwardOpenResponse) + " header");
            int index = 0;
            OriginatorToTargetConnectionId = bytes.ToUint(ref index);
            TargetToOriginatorConnectionId = bytes.ToUint(ref index);
            ConnectionSerialNumber = bytes.ToUshort(ref index);
            OriginatorVendorId = bytes.ToUshort(ref index);
            OriginatorSerialNumber = bytes.ToUint(ref index);
            OriginatorToTargetActualPacketRate = bytes.ToUint(ref index);
            TargetToOriginatorActualPacketRate = bytes.ToUint(ref index);
            var applicationReplySize = bytes[index++] * 2;
            index++; // Reserved
            ApplicationReply = bytes.Segment(ref index);
        }

        public uint OriginatorToTargetConnectionId { get; init; }
        public uint TargetToOriginatorConnectionId { get; init; }
        public ushort ConnectionSerialNumber { get; init; }
        public ushort OriginatorVendorId { get; init; }
        public uint OriginatorSerialNumber { get; init; }
        /// <summary>
        /// Originator to target actual packet rate in μs
        /// </summary>
        public uint OriginatorToTargetActualPacketRate { get; init; }
        /// <summary>
        /// Target to originator actual packet rate in μs
        /// </summary>
        public uint TargetToOriginatorActualPacketRate { get; init; }
        public IReadOnlyList<byte> ApplicationReply { get; }
    }
}
