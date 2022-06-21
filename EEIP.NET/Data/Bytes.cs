namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Collections.Generic;

    public record Bytes :
        Byteable
    {
        public Bytes(IReadOnlyList<byte> data = null)
            => this.Data = data ?? EmptyArray;

        public Bytes(params byte[] data) :
            this((IReadOnlyList<byte>)data)
        { }

        public IReadOnlyList<byte> Data { get; }
        public override ushort ByteCount => (ushort)(Data?.Count);

        public static readonly Bytes Empty = new Bytes();
        public static readonly byte[] EmptyArray = Array.Empty<byte>();

        public static readonly Bytes Zero = new Bytes(0);

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            if (ByteCount == 0)
                return;
            foreach (var @byte in Data)
                bytes[index++] = @byte;
        }
    }
}
