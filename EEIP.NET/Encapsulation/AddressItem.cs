namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// 2-6.2 Address Item
    /// </summary>
    public abstract record AddressItem :
        Item
    {
        protected AddressItem(ushort type) :
            base(type)
        { }
    }
}