using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;

    /// <summary>
    /// Implicit messaging connection from target to originator
    /// </summary>
    public record TargetToOriginatorConnection :
        IOConnection
    {
        public TargetToOriginatorConnection(
            ushort udpPort = DefaultPort,
            EPath outputPath = null,
            bool ownerRedundant = DefaultOwnerRedundant,
            ConnectionType type = ConnectionType.Multicast,
            ConnectionRealTimeFormat realTimeFormat = ConnectionRealTimeFormat.Modeless,
            uint requestedPacketRate = DefaultRequestedPacketRate,
            ConnectionPriority priority = DefaultPriority,
            ushort? dataSize = null,
            ConnectionSizeType dataSizeType = DefaultDataSizeType) :
            base(udpPort, ownerRedundant, type, realTimeFormat, requestedPacketRate, priority, dataSize, dataSizeType)
        {
            this.OutputPath = type == ConnectionType.Null ?
                null :
                outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        }

        /// <summary>
        /// Output/Production connection path. Usually <see cref="Assembly"/> instance which data is sent by target via this connection.
        /// </summary>
        public EPath OutputPath { get; }
    }
}
