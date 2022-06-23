using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using Sres.Net.EEIP.CIP.Path;

    /// <summary>
    /// Implicit messaging connection from target to originator
    /// </summary>
    public record TargetToOriginatorConnection :
        IOConnection
    {
        public TargetToOriginatorConnection(
            EPath dataPath = null,
            bool ownerRedundant = DefaultOwnerRedundant,
            ConnectionType type = ConnectionType.Multicast,
            ConnectionRealTimeFormat realTimeFormat = ConnectionRealTimeFormat.Modeless,
            TimeSpan? requestedPacketRate = null,
            ConnectionPriority priority = DefaultPriority,
            ushort? dataSize = null,
            ConnectionSizeType dataSizeType = DefaultDataSizeType) :
            base(ownerRedundant, type, realTimeFormat, requestedPacketRate, priority, dataSize, dataSizeType)
            => DataPath = dataPath;

        /// <summary>
        /// Production connection path. Usually <see cref="Assembly"/> instance which data is sent by target to originator via this connection.
        /// Production Inhibit Time can be specified with <see cref="EPath.WithProductionInhibitTime"/>.
        /// </summary>
        /// <remarks>If <see cref="EPath.ClassId"/> is <see cref="Assembly.ClassId"/> and <see cref="EPath.AttributeId"/> is not specified, <see cref="Assembly.DataAttributeId"/> is used.</remarks>
        public EPath DataPath { get; }
        public override EPath Path => IsNull ?
            null :
            DataPath;

        /// <summary>
        /// Raised before <see cref="IOConnection.Data"/> is received
        /// </summary>
        public event EventHandler<IOContext> DataReceiving;
        /// <summary>
        /// Raised after <see cref="IOConnection.Data"/> is received
        /// </summary>
        public event EventHandler<IOContext> DataReceived;

        internal void OnDataReceiving(IOContext context) => DataReceiving?.Invoke(this, context);
        internal void OnDataReceived(IOContext context)
        {
            SetLastDataTransferTime();
            DataReceived?.Invoke(this, context);
        }

        public override void Dispose() => DataReceived = null;
    }
}
