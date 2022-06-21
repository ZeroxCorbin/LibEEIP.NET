namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Collections.Generic;

    public record BytesConcatenation :
        Byteable
    {
        public BytesConcatenation(params IByteable[] items) :
            this((IReadOnlyList<IByteable>)items)
        { }

        public BytesConcatenation(IReadOnlyList<IByteable> items)
            => this.Items = items ?? throw new ArgumentNullException(nameof(items));

        public IReadOnlyList<IByteable> Items { get; }

        public override ushort ByteCount => Items.ByteCount();

        protected override void DoToBytes(byte[] bytes, ref int index) => Items.ToBytes(bytes, ref index);
    }
}
