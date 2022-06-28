namespace Sres.Net.EEIP.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// <see cref="IByteable"/> base
    /// </summary>
    public abstract record Byteable :
        ByteCountBase,
        IByteable
    {
        protected Byteable()
        { }

        protected Byteable(IReadOnlyList<byte> bytes, ref int index) :
            base(bytes, ref index)
        { }


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
