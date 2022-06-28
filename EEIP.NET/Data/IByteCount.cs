namespace Sres.Net.EEIP.Data
{
    /// <summary>
    /// Specifies this item data size
    /// </summary>
    public interface IByteCount
    {
        /// <summary>
        /// Number of data bytes
        /// </summary>
        ushort ByteCount { get; }
    }
}