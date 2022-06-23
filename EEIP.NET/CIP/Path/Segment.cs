namespace Sres.Net.EEIP.CIP.Path
{
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// <see cref="EPath"/> segment
    /// </summary>
    public abstract record Segment :
        Byteable
    {
        protected Segment(SegmentType type)
            => this.Type = type;

        public SegmentType Type { get; }
        /// <summary>
        /// Segment format. Only first 5 bits are used.
        /// </summary>
        public abstract byte Format { get; }
        public byte TypeAndFormat => (byte)(
            ((byte)Type << 5) |
            (Format & 0b11111));

        public abstract bool Skip { get; }

        public sealed override ushort ByteCount => Skip ?
            byte.MinValue :
            (ushort)(1 + DataCount);

        public abstract ushort DataCount { get; }

        protected sealed override void DoToBytes(byte[] bytes, ref int index)
        {
            if (Skip)
                return;
            bytes[index++] = TypeAndFormat;
            DataToBytes(bytes, ref index);
        }

        protected abstract void DataToBytes(byte[] bytes, ref int index);
    }
}