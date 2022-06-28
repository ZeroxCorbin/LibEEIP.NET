namespace Sres.Net.EEIP.Data
{
    using System;

    /// <summary>
    /// Allows to fill this item data bytes to an array
    /// </summary>
    public interface IByteable :
        IByteCount
    {
        /// <summary>
        /// Fills this item data bytes to <paramref name="bytes"/> starting at <paramref name="index"/>
        /// </summary>
        /// <param name="bytes">Bytes to fill</param>
        /// <param name="index">Start index to <paramref name="bytes"/> shifted after setting data</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or invalid</exception>
        void ToBytes(byte[] bytes, ref int index);
    }
}