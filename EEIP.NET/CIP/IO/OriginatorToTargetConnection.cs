using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using Sres.Net.EEIP.CIP.Path;

    /// <summary>
    /// Implicit messaging connection from originator to target
    /// </summary>
    public record OriginatorToTargetConnection :
        IOConnection
    {
        public OriginatorToTargetConnection(
            EPath configurationPath = null,
            EPath dataPath = null,
            bool ownerRedundant = DefaultOwnerRedundant,
            ConnectionType type = ConnectionType.PointToPoint,
            ConnectionRealTimeFormat realTimeFormat = ConnectionRealTimeFormat.Header32Bit,
            TimeSpan? requestedPacketRate = null,
            ConnectionPriority priority = DefaultPriority,
            ushort? dataSize = null,
            ConnectionSizeType dataSizeType = DefaultDataSizeType) :
            base(ownerRedundant, type, realTimeFormat, requestedPacketRate, priority, dataSize, dataSizeType)
        {
            DataPath = IsNull ?
                null:
                dataPath ?? configurationPath ?? DefaultDataPath;
            if (configurationPath?.HasData == false)
                throw new ArgumentException("Configuration path must have data", nameof(configurationPath));
            ConfigurationPath = configurationPath;
            //if (IsNull && configurationPath is null)
            //    throw new ArgumentNullException(nameof(configurationPath), "Configuration path must be specified when connection type is " + nameof(ConnectionType.Null));
        }

        public ushort SerialNumber { get; init; } = (ushort)Random.Next(ushort.MaxValue);

        /// <summary>
        /// Configuration path. Usually <see cref="Assembly"/> instance.
        /// Configuration data must be appended to this path with <see cref="EPath.AddData"/>.
        /// </summary>
        /// <remarks>If <see cref="EPath.ClassId"/> is <see cref="Assembly.ClassId"/> and <see cref="EPath.AttributeId"/> is not specified, <see cref="Assembly.DataAttributeId"/> is used.</remarks>
        public EPath ConfigurationPath { get; }

        /// <summary>
        /// Default <see cref="DataPath"/>: <see cref="Assembly.ClassId"/>, <see cref="ObjectBase.DefaultInstanceId"/>
        /// </summary>
        public static readonly EPath DefaultDataPath = EPath.ToObject(Assembly.ClassId, ObjectBase.DefaultInstanceId);
        /// <summary>
        /// Consumption connection path. Usually <see cref="Assembly"/> instance which data is sent by originator to target via this connection.
        /// </summary>
        /// <remarks>If <see cref="EPath.ClassId"/> is <see cref="Assembly.ClassId"/> and <see cref="EPath.AttributeId"/> is not specified, <see cref="Assembly.DataAttributeId"/> is used.</remarks>
        public EPath DataPath { get; }

        public override EPath Path =>
            IsNull ||
            DataPath is null ||
            ReferenceEquals(ConfigurationPath, DataPath) ?
                ConfigurationPath :
                EPath.Concat(ConfigurationPath, DataPath);

        /// <summary>
        /// Raised before <see cref="IOConnection.Data"/> is sent to target
        /// </summary>
        public event EventHandler<IOContext> DataSending;
        /// <summary>
        /// Raised after <see cref="IOConnection.Data"/> is sent to target
        /// </summary>
        /// <remarks><see cref="EventHandler{TEventArgs}"/> arguments are <see cref="IOConnection.Data"/> sent</remarks>
        public event EventHandler<IOContext> DataSent;

        internal void OnDataSending(IOContext context) => DataSending?.Invoke(this, context);
        internal void OnDataSent(IOContext context)
        {
            SetLastDataTransferTime();
            DataSent?.Invoke(this, context);
        }

        public override void Dispose()
        {
            DataSending = null;
            DataSent = null;
        }
    }
}
