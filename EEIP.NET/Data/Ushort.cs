namespace Sres.Net.EEIP.Data
{
    public record Ushort :
        ValueBase<ushort>
    {
        public Ushort(ushort value) :
            base(value)
        { }

        public override ushort ByteCount => 2;

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            bytes[index++] = (byte)Value;
            bytes[index++] = (byte)(Value >> 8);
        }

        public static implicit operator Ushort(ushort value) => new Ushort(value);
    }
}
