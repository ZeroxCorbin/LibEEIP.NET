using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;

    /// <summary>
    /// Implicit messaging connection from originator to target
    /// </summary>
    public record OriginatorToTargetConnection :
        IOConnection
    {
        public OriginatorToTargetConnection(
            ushort udpPort = DefaultPort,
            EPath configurationPath = null,
            EPath inputPath = null,
            bool ownerRedundant = DefaultOwnerRedundant,
            ConnectionType type = ConnectionType.PointToPoint,
            ConnectionRealTimeFormat realTimeFormat = ConnectionRealTimeFormat.Header32Bit,
            uint requestedPacketRate = DefaultRequestedPacketRate,
            ConnectionPriority priority = DefaultPriority,
            ushort? dataSize = null,
            ConnectionSizeType dataSizeType = DefaultDataSizeType) :
            base(udpPort, ownerRedundant, type, realTimeFormat, requestedPacketRate, priority, dataSize, dataSizeType)
        {
            this.ConfigurationPath = configurationPath ?? DefaultConfigurationPath;
            this.InputPath = inputPath;
        }

        public ushort SerialNumber { get; init; } = (ushort)Random.Next(ushort.MaxValue);
        /// <summary>
        /// Configuration path. Usually <see cref="Assembly"/> instance.
        /// </summary>
        public EPath ConfigurationPath
        {
            get => configurationPath;
            init => configurationPath = value ?? throw new ArgumentNullException(nameof(ConfigurationPath));
        }
        public static readonly EPath DefaultConfigurationPath = EPath.ToObject(Assembly.ClassId, ObjectBase.DefaultInstanceId);
        /// <summary>
        /// Input/Consumption connection path. Usually <see cref="Assembly"/> instance which data is set by originator via this connection.
        /// </summary>
        public EPath InputPath
        {
            get => Type == ConnectionType.Null ?
                null :
                inputPath ?? configurationPath;
            init => inputPath = value;
        }
        public EPath Path
        {
            get
            {
                var consumption = InputPath;
                return
                    consumption is null ||
                    ReferenceEquals(ConfigurationPath, consumption) ?
                        ConfigurationPath :
                    EPath.Concat(ConfigurationPath, consumption);
            }
        }

        private EPath configurationPath;
        private EPath inputPath;
    }
}
