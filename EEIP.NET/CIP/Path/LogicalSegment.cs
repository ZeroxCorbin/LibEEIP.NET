namespace Sres.Net.EEIP.CIP.Path
{
    using Sres.Net.EEIP.Data;

    public record LogicalSegment :
        Segment
    {
        public LogicalSegment(uint value, bool optional, LogicalType logicalType) :
            base(SegmentType.Logical)
        {
            this.Value = value;
            this.Optional = optional;
            this.LogicalType = logicalType;
        }

        public override byte Format => (byte)(
            ((byte)LogicalType) << 2 |
            (byte)LogicalFormat);

        public uint Value { get; init; }
        public bool Optional { get; init; }

        public LogicalType LogicalType { get; init; }

        public LogicalFormat LogicalFormat =>
            Value <= byte.MaxValue ?
                LogicalFormat.Bit8 :
                Value <= ushort.MaxValue ?
                    LogicalFormat.Bit16 :
                    LogicalFormat.Bit32;

        public override bool Skip => Optional && Value == 0;

        public override ushort DataCount
        {
            get
            {
                if (Skip)
                    return byte.MinValue;
                var format = LogicalFormat;
                switch (format)
                {
                    case LogicalFormat.Bit8:
                        return 1;
                    case LogicalFormat.Bit16:
                        return 3;
                    case LogicalFormat.Bit32:
                    default:
                        return 5;
                }
            }
        }

        protected override void DataToBytes(byte[] bytes, ref int index)
        {
            var format = LogicalFormat;
            switch (format)
            {
                case LogicalFormat.Bit8:
                    bytes[index++] = (byte)Value;
                    break;
                case LogicalFormat.Bit16:
                    bytes[index++] = 0; //Padded Byte
                    ((ushort)Value).ToBytes(bytes, ref index);
                    break;
                case LogicalFormat.Bit32:
                default:
                    bytes[index++] = 0; //Padded Byte
                    Value.ToBytes(bytes, ref index);
                    break;
            }
        }
    }
}