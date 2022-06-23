namespace Sres.Net.EEIP.Data
{
    public record Byte :
        ValueBase<byte>
    {
        public Byte(byte value) :
            base(value)
        { }

        public override ushort ByteCount => 1;

        protected override void DoToBytes(byte[] bytes, ref int index) => bytes[index++] = Value;
    }
}
