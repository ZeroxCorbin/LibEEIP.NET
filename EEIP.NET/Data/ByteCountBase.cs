namespace Sres.Net.EEIP.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// <see cref="IByteCount"/> base
    /// </summary>
    public abstract record ByteCountBase :
        IByteCount
    {
        protected ByteCountBase()
        { }

        /// <summary>
        /// Sets this instance from <paramref name="bytes"/>
        /// </summary>
        /// <remarks>Object supporting this optional construction from <paramref name="bytes"/> should use this constructor and add its own specific implementation</remarks>
        /// <param name="bytes">Data</param>
        /// <param name="index">Starting index to <paramref name="bytes"/> shifted after reading data</param>
        protected ByteCountBase(IReadOnlyList<byte> bytes, ref int index)
            => bytes.ValidateEnoughBytes(ByteCount, GetType().Name, index);

        protected ByteCountBase(IReadOnlyList<byte> bytes, int index = 0) :
            this(bytes, ref index)
        { }

        /// <inheritdoc/>
        public abstract ushort ByteCount { get; }
    }
}
