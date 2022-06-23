namespace Sres.Net.EEIP.CIP.Path
{
    using System;
    using Sres.Net.EEIP.Data;

    public record SimpleDataSegment :
        DataSegment
    {
        public SimpleDataSegment(params byte[] data) :
            base(DataType.Simple)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Data cannot be empty", nameof(data));
            if (data.Length % 2 > 0)
                throw new ArgumentException("Data length must be even", nameof(data));
            if (data.Length / 2 > byte.MaxValue)
                throw new ArgumentException("Data length must be smaller than " + byte.MaxValue * 2, nameof(data));
            this.data = new Bytes(data);
        }
        public byte[] Data { get; }
        public override bool Skip => false;
        public sealed override ushort DataCount => (ushort)(1 + Data.Length);

        protected override void DataToBytes(byte[] bytes, ref int index)
        {
            bytes[index++] = (byte)(Data.Length / 2); // number of 16 bit words
            data.ToBytes(bytes, ref index);
        }

        private readonly Bytes data;
    }
}
