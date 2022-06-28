using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using Sres.Net.EEIP.CIP.Path;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Forward close request for implicit communication.
    /// CIP Table 3-5.19.
    /// </summary>
    public record ForwardCloseRequest :
        ConnectionRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ForwardCloseRequest(EPath connectionPath, ForwardOpenResponse response, Timeout timeout = null) :
            base(
                0x4E,
                EPath.ToObject(ConnectionManager.ClassId, ObjectBase.DefaultInstanceId),
                GetData(connectionPath, response, timeout ?? Timeout.Default),
                timeout)
        { }

        private static BytesConcatenation GetData(EPath connectionPath, ForwardOpenResponse response, Timeout timeout)
        {
            if (connectionPath is null)
                throw new ArgumentNullException(nameof(connectionPath));
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            return new(
                timeout,
                response.ConnectionSerialNumber.AsByteable(),
                response.OriginatorVendorId.AsByteable(),
                response.OriginatorSerialNumber.AsByteable(),
                connectionPath.Size.AsByteable(),
                Reserved,
                new BytesConcatenation(connectionPath.Segments));
        }

        private static readonly Bytes Reserved = Bytes.Zero;
    }
}
