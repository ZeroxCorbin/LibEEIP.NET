namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Object class.
    /// CIP Table 4-4.2 Reserved Class Attributes for All Object Class Definitions
    /// </summary>
    public record ObjectClass :
        ByteCountBase
    {
        public ObjectClass()
        { }

        public ObjectClass(IReadOnlyList<byte> bytes, ref int index) :
            base(bytes, ref index)
        {
            Revision = bytes.ToUshort(ref index);
            MaxInstanceCount = bytes.ToUshort(ref index);
            InstanceCount = bytes.ToUshort(ref index);
            OptionalAttributes = bytes.ToUshortListWithUshortCount(ref index);
            OptionalServices = bytes.ToUshortListWithUshortCount(ref index);
            LastClassAttributeId = bytes.ToUshort(ref index);
            LastInstanceAttributeId = bytes.ToUshort(ref index);
        }

        public ushort Revision { get; init; }
        public ushort MaxInstanceCount { get; init; }
        public ushort InstanceCount { get; init; }
        public IReadOnlyList<ushort> OptionalAttributes { get; init; }
        public IReadOnlyList<ushort> OptionalServices { get; init; }
        public ushort LastClassAttributeId { get; init; }
        public ushort LastInstanceAttributeId { get; init; }

        public override ushort ByteCount => (ushort)(
            14 +
            OptionalAttributes?.Count * 2 ?? 0 +
            OptionalServices?.Count * 2 ?? 0);
    }
}