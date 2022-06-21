namespace Sres.Net.EEIP.Encapsulation
{
    using Sres.Net.EEIP.Data;

    public record DataItem :
        Item
    {
        public DataItem(DataItemType type, IByteable data = null) :
            base((ushort)type)
            => Data = data ?? Bytes.Empty;

        public IByteable Data { get; }

        public override ushort DataLength => Data.ByteCount;

        protected override void AddData(byte[] bytes, ref int index)
            => Data.ToBytes(bytes, ref index);
    }
}
