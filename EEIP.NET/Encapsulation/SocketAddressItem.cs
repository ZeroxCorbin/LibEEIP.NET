namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// 2-6.3.3 Sockaddr Info Item
    /// </summary>
    public partial record SocketAddressItem :
        Item
    {
        public SocketAddressItem(SocketAddressItemType type, SocketAddress value) :
            base((ushort)type)  // Volume 2 Table 2-6.9
            => this.Value = value ?? throw new System.ArgumentNullException(nameof(value));

        public SocketAddress Value { get; }
        public override ushort DataLength => 16;

        protected override void AddData(byte[] bytes, ref int index) => Value.ToBytes(bytes, ref index);
    }
}
