namespace Sres.Net.EEIP.CIP.IO
{
    using System;

    /// <summary>
    /// Provides access to Class 1 real-time IO data
    /// </summary>
    public class IOData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"><see cref="Data"/></param>
        public IOData(byte[] data)
            => this.Data = data ?? throw new ArgumentNullException(nameof(data));

        /// <summary>
        /// IO data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Last time of received/sent <see cref="Data"/>
        /// </summary>        
        public DateTime? LastDataTransferTime { get; internal set; }
    }
}
