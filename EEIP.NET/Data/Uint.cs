namespace Sres.Net.EEIP.Data
{
    public record Uint :
        ValueBase<uint>
    {
        public Uint(uint value) :
            base(value)
        { }

        public override ushort ByteCount => 4;

        protected override void DoToBytes(byte[] bytes, ref int index) => Value.ToBytes(bytes, ref index);
    }
}
