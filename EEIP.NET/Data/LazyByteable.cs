namespace Sres.Net.EEIP.Data
{
    using System;

    /// <summary>
    /// Wraps another <see cref="IByteable"/> which is lazily created when <see cref="IByteable"/> member is called
    /// </summary>
    public record LazyByteable :
        Byteable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="create">Creates <see cref="Byteable"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="create"/> is null</exception>
        public LazyByteable(Func<IByteable> create)
        {
            if (create is null)
                throw new ArgumentNullException(nameof(create));
            byteable = new Lazy<IByteable>(create);
        }

        /// <summary>
        /// Lazily created <see cref="IByteable"/>
        /// </summary>
        public IByteable Byteable => byteable.Value;

        /// <inheritdoc/>
        public override ushort ByteCount => Byteable.ByteCount;

        /// <inheritdoc/>
        protected override void DoToBytes(byte[] bytes, ref int index) => Byteable.ToBytes(bytes, ref index);

        private readonly Lazy<IByteable> byteable;
    }
}
