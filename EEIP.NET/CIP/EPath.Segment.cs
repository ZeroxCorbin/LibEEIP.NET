namespace Sres.Net.EEIP.CIP
{
    using Sres.Net.EEIP.Data;

    public partial record EPath
    {
        public partial record Segment :
            Byteable
        {
            public Segment(uint value, bool optional, LogicalType type)
            {
                this.Value = value;
                this.Optional = optional;
                this.Type = type;
            }

            /// <summary>
            /// Logical segment type
            /// </summary>
            public const byte TypePrefix = 0b00100000; // = 0x20

            public uint Value { get; init; }
            public bool Optional { get; init; }
            public LogicalType Type { get; init; }

            public LogicalFormat Format =>
                Value <= byte.MaxValue ?
                    LogicalFormat.Bit8 :
                    Value <= ushort.MaxValue ?
                        LogicalFormat.Bit16 :
                        LogicalFormat.Bit32;

            public bool Skip => Optional && Value == 0;

            public override ushort ByteCount
            {
                get
                {
                    if (Skip)
                        return byte.MinValue;
                    var format = Format;
                    switch (format)
                    {
                        case LogicalFormat.Bit8:
                            return 2;
                        case LogicalFormat.Bit16:
                            return 4;
                        case LogicalFormat.Bit32:
                        default:
                            return 6;
                    }
                }
            }

            protected override void DoToBytes(byte[] bytes, ref int index)
            {
                if (Skip)
                    return;
                var format = Format;
                var segmentType = (byte)(TypePrefix + (byte)this.Type + (byte)format);
                bytes[index++] = segmentType;
                switch (format)
                {
                    case LogicalFormat.Bit8:
                        bytes[index++] = (byte)Value;
                        break;
                    case LogicalFormat.Bit16:
                        bytes[index++] = 0; //Padded Byte
                        ((ushort)Value).AsByteable().ToBytes(bytes, ref index);
                        break;
                    case LogicalFormat.Bit32:
                    default:
                        bytes[index++] = 0; //Padded Byte
                        Value.AsByteable().ToBytes(bytes, ref index);
                        break;
                }
            }
        }
    }
}
