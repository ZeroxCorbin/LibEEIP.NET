namespace Sres.Net.EEIP.Encapsulation
{
    public abstract record AddressItem :
        Item
    {
        protected AddressItem(ushort type) :
            base(type)
        { }
    }
}