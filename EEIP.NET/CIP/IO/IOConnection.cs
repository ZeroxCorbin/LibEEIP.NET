namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using System.Net;
    using Sres.Net.EEIP.CIP.Path;
    using Sres.Net.EEIP.Data;
    using Sres.Net.EEIP.Encapsulation;

    /// <summary>
    /// Implicit messaging (input/output UDP) connection base
    /// </summary>
    public abstract record IOConnection :
        IDisposable
    {
        protected IOConnection(
            bool ownerRedundant,
            ConnectionType type,
            ConnectionRealTimeFormat realTimeFormat,
            TimeSpan? requestedPacketRate,
            ConnectionPriority priority,
            ushort? dataSize,
            ConnectionSizeType dataSizeType)
        {
            RequestedPacketRate = requestedPacketRate ?? DefaultRequestedPacketRate;
            OwnerRedundant = ownerRedundant;
            Type = type;
            Priority = priority;
            DataSizeType = dataSizeType;
            DataSize = dataSize;
            RealTimeFormat = realTimeFormat;
        }

        public readonly uint Id = (uint)Random.Next();

        /// <summary>
        /// Connection path
        /// </summary>
        public abstract EPath Path { get; }

        /// <summary>
        /// Requested packet rate with μs resolution
        /// </summary>
        /// <value>0 means no cyclic messages</value>
        /// <exception cref="ArgumentOutOfRangeException">set: Value is &lt; <see cref="TimeSpan.Zero"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/>.<see cref="TimeSpans.TotalMicroseconds"/> is &gt; <see cref="uint.MaxValue"/></exception>
        public TimeSpan RequestedPacketRate
        {
            get => requestedPacketRate;
            init => requestedPacketRate = value.ValidatePositiveOrZeroUint(out _);
        }
        /// <summary>
        /// Default <see cref="RequestedPacketRate"/> = 500 ms
        /// </summary>
        public static readonly TimeSpan DefaultRequestedPacketRate = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Whether multiple connections are allowed
        /// </summary>
        public bool OwnerRedundant { get; init; }
        public const bool DefaultOwnerRedundant = true;

        /// <summary>
        /// Type
        /// </summary>
        public ConnectionType Type { get; init; }
        /// <summary>
        /// Whether <see cref="Type"/> is <see cref="ConnectionType.Null"/>
        /// </summary>
        public bool IsNull => Type == ConnectionType.Null;

        /// <summary>
        /// Socket address from <see cref="SocketAddressItem"/>
        /// </summary>
        public IPEndPoint SocketAddress { get; internal set; }

        public ConnectionPriority Priority { get; init; }
        public const ConnectionPriority DefaultPriority = ConnectionPriority.Scheduled;

        #region Data

        /// <summary>
        /// IO data size
        /// </summary>
        /// <value>null means autodetection</value>
        public ushort? DataSize { get; internal set; }

        public ConnectionSizeType DataSizeType { get; init; }
        public const ConnectionSizeType DefaultDataSizeType = ConnectionSizeType.Variable;

        public void ValidateDataSize(ushort maxDataSize)
        {
            string name = $"{GetType().Name}.{nameof(DataSize)}";
            if (DataSize is null)
                throw new ArgumentNullException(name);
            if (DataSize > maxDataSize)
                throw new ArgumentOutOfRangeException(name, DataSize, "Value must be < " + maxDataSize);
        }

        /// <summary>
        /// Class 1 real-time IO data
        /// </summary>
        public byte[] Data { get; private set; }

        internal void CreateData(ushort maxDataSize)
        {
            ValidateDataSize(maxDataSize);
            Data = new byte[DataSize.Value];
        }

        /// <summary>
        /// Last time of received/sent <see cref="Data"/>
        /// </summary>        
        public DateTime? LastDataTransferTime { get; private set; }
        /// <summary>
        /// Time elapsed since <see cref="LastDataTransferTime"/>
        /// </summary>
        public TimeSpan? TimeSinceLastDataTransfer => DateTime.Now - LastDataTransferTime;

        protected void SetLastDataTransferTime() => LastDataTransferTime = DateTime.Now;

        #endregion

        public ConnectionRealTimeFormat RealTimeFormat { get; init; }

        // Zählt den Sequencecount und evtl 32bit header zu der Länge dazu
        public ushort HeaderOffset => RealTimeFormat == ConnectionRealTimeFormat.Heartbeat ?
            (ushort)0 :
            RealTimeFormat == ConnectionRealTimeFormat.Header32Bit ?
                (ushort)6 :
                (ushort)2;

        public IByteable GetNetworkParameters(bool large) => new LazyByteable(() =>
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
        });

        #region Timeout

        public TimeSpan Timeout => new(RequestedPacketRate.Ticks * TimeoutMultiplier);

        /// <summary>
        /// <see cref="ConnectionTimeoutMultiplier"/> value from <see cref="SetTimeoutMultiplier"/>
        /// </summary>
        public ushort TimeoutMultiplier { get; private set; }

        /// <summary>
        /// Sets <see cref="TimeoutMultiplier"/> to the result of <see cref="GetTimeoutMultiplier"/>
        /// </summary>
        /// <param name="multiplier"></param>
        internal void SetTimeoutMultiplier(ConnectionTimeoutMultiplier multiplier) => TimeoutMultiplier = GetTimeoutMultiplier(multiplier);

        /// <summary>
        /// Gets <paramref name="multiplier"/>`s value
        /// </summary>
        /// <param name="multiplier">Multiplier</param>
        /// <returns>2 ^ (2 + <paramref name="multiplier"/>) </returns>
        public static ushort GetTimeoutMultiplier(ConnectionTimeoutMultiplier multiplier) => (ushort)(1 << (2 + (byte)multiplier));

        #endregion

        public abstract void Dispose();

        protected static readonly Random Random = new Random();
        private TimeSpan requestedPacketRate;
    }
}