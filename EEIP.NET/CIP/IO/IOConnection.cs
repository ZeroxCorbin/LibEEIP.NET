namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Implicit messaging (input/output UDP) connection base
    /// </summary>
    public abstract record IOConnection
    {
        protected IOConnection(
            ushort udpPort,
            bool ownerRedundant,
            ConnectionType type,
            ConnectionRealTimeFormat realTimeFormat,
            uint requestedPacketRate,
            ConnectionPriority priority,
            ushort? dataSize,
            ConnectionSizeType dataSizeType)
        {
            Port = udpPort;
            RequestedPacketRate = requestedPacketRate;
            OwnerRedundant = ownerRedundant;
            Type = type;
            Priority = priority;
            DataSizeType = dataSizeType;
            DataSize = dataSize;
            RealTimeFormat = realTimeFormat;
        }

        /// <summary>
        /// UDP port at connection origin
        /// </summary>
        public ushort Port { get; init; }
        /// <summary>
        /// Default UDP port: 0x08AE = 2222
        /// </summary>
        public const ushort DefaultPort = 0x08AE;

        public readonly uint Id = (uint)Random.Next();

        /// <summary>
        /// Requested packet rate in μs
        /// </summary>
        public uint RequestedPacketRate { get; init; }
        public const uint DefaultRequestedPacketRate = 0x7A120; // 500ms

        /// <summary>
        /// Whether multiple connections are allowed
        /// </summary>
        public bool OwnerRedundant { get; init; }
        public const bool DefaultOwnerRedundant = true;

        public ConnectionType Type { get; init; }

        public ConnectionPriority Priority { get; init; }
        public const ConnectionPriority DefaultPriority = ConnectionPriority.Scheduled;

        /// <summary>
        /// IO data size
        /// </summary>
        /// <value>null means autodetection</value>
        public ushort? DataSize { get; internal set; }

        public ConnectionSizeType DataSizeType { get; init; }
        public const ConnectionSizeType DefaultDataSizeType = ConnectionSizeType.Variable;

        public void ValidateDataSize(string name)
        {
            if (DataSize is null)
                throw new ArgumentException($"Missing {name}.{nameof(DataSize)}");
        }

        public byte[] CreateData(string name)
        {
            ValidateDataSize(name);
            return new byte[DataSize.Value];
        }

        public ConnectionRealTimeFormat RealTimeFormat { get; init; }

        // Zählt den Sequencecount und evtl 32bit header zu der Länge dazu
        public ushort HeaderOffset => RealTimeFormat == ConnectionRealTimeFormat.Heartbeat ?
            (ushort)0 :
            RealTimeFormat == ConnectionRealTimeFormat.Header32Bit ?
                (ushort)6 :
                (ushort)2;

        public IByteable GetNetworkParameters(bool large)
        {
            var ownerRedundant = (uint)(OwnerRedundant ? 1 : 0);
            var type = (uint)((byte)Type & 0x03);
            var size = (ushort)(DataSize + HeaderOffset);    //The maximum size in bytes of the data for each direction (were applicable) of the connection. For a variable -> maximum
            var sizeType = (uint)DataSizeType;
            var priority = (uint)((byte)Priority & 0x03);
            var result = large ?
                (uint)(size & 0xFFFF) | sizeType << 25 | priority << 26 | type << 29 | ownerRedundant << 31 :
                (ushort)((ushort)(size & 0x1FF) | sizeType << 9 | priority << 10 | type << 13 | ownerRedundant << 15);
            return large ?
                result.AsByteable() :
                ((ushort)result).AsByteable();
        }

        protected static readonly Random Random = new Random();
    }
}