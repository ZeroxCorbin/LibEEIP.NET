namespace Sres.Net.EEIP.Data
{
    public record Ushort :
        ValueBase<ushort>
    {
        public Ushort(ushort value) :
            base(value)
        { }

        public override ushort ByteCount => 2;

        protected override void DoToBytes(byte[] bytes, ref int index) => Value.ToBytes(bytes, ref index);
    }
}
