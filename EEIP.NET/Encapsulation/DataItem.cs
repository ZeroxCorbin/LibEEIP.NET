namespace Sres.Net.EEIP.Encapsulation
{
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// 2-6.3 Data Item
    /// </summary>
    public record DataItem :
        Item
    {
        public DataItem(DataItemType type, IByteable data = null) :
            base((ushort)type)
            => Data = data ?? Bytes.Empty;

        public IByteable Data { get; }

        public override ushort DataLength => Data?.ByteCount ?? 0;

        protected override void AddData(byte[] bytes, ref int index)
            => Data.ToBytes(bytes, ref index);
    }
}
