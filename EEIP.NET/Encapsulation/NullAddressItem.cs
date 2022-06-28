namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// 2-6.2.1 Null Address Item
    /// </summary>
    public record NullAddressItem :
        AddressItem
    {
        private NullAddressItem() :
            base(Type)
        { }

        public new const ushort Type = 0;

        public static readonly NullAddressItem Instance = new NullAddressItem();

        public override ushort DataLength => 0;

        protected override void AddData(byte[] bytes, ref int index)
        { }
    }
}
