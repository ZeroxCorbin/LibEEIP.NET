namespace Sres.Net.EEIP.Data
{
    public abstract record Byteable :
        IByteable
    {
        public abstract ushort ByteCount { get; }

        public void ToBytes(byte[] bytes, ref int index)
        {
            ushort count = ByteCount;
            if (count == 0)
                return;
            bytes.ValidateEnoughBytes(count, GetType().Name, index);
            DoToBytes(bytes, ref index);
        }

        protected abstract void DoToBytes(byte[] bytes, ref int index);
    }
}
