namespace Sres.Net.EEIP.CIP.IO
{
    using Sres.Net.EEIP.CIP.ObjectLibrary;
    using Sres.Net.EEIP.CIP.Path;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Forward open/close request
    /// </summary>
    /// </summary>
    public abstract record ConnectionRequest :
        MessageRouterRequest
    {
        protected ConnectionRequest(byte service, EPath path, IByteable data, Timeout timeout = null) :
            base(service, path, data)
            => Timeout = timeout ?? Timeout.Default;

        /// <summary>
        /// Request processing timeout
        /// </summary>
        /// <value>Default: <see cref="Timeout.Default"/></value>
        public Timeout Timeout { get; }
    }
}
