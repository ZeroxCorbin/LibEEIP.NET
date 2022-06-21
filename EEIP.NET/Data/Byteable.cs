namespace Sres.Net.EEIP.Data
{
    using System;

    public abstract record Byteable :
        IByteable
    {
        public abstract ushort ByteCount { get; }

        public void ToBytes(byte[] bytes, ref int index)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || index + ByteCount >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (ByteCount > 0)
                DoToBytes(bytes, ref index);
        }

        protected abstract void DoToBytes(byte[] bytes, ref int index);
    }
}
