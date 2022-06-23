namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// IO connection originator/target
    /// </summary>
    public abstract record IOConnectionEndPoint
    {
        protected IOConnectionEndPoint(ushort port)
            => this.Port = port;

        /// <summary>
        /// UDP port at connection origin
        /// </summary>
        public ushort Port { get; internal set; }

        /// <summary>
        /// Default UDP port: 0x08AE = 2222
        /// </summary>
        public const ushort DefaultPort = 0x08AE;
    }
}
