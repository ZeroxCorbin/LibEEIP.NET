using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Forward close request for implicit communication. Table 3-5.19 (Vol. 1).
    /// </summary>
    public record ForwardCloseRequest :
        MessageRouterRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ForwardCloseRequest(EPath connectionPath, ForwardOpenResponse response) :
            base(
                0x4E,
                EPath.ToObject(ConnectionManager.ClassId, ObjectBase.DefaultInstanceId),
                new BytesConcatenation(
                    ForwardOpenRequest.TimeTick.AsByteable(),
                    ForwardOpenRequest.TimeoutTicks.AsByteable(),
                    (response?.ConnectionSerialNumber ?? throw new ArgumentNullException(nameof(response))).AsByteable(),
                    response.OriginatorVendorId.AsByteable(),
                    response.OriginatorSerialNumber.AsByteable(),
                    (connectionPath ?? throw new ArgumentNullException(nameof(connectionPath))).Size.AsByteable(),
                    Reserved,
                    new BytesConcatenation(connectionPath.Segments)))
        { }

        private static readonly Bytes Reserved = Bytes.Zero;
    }
}
