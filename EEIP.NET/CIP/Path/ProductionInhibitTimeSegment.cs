namespace Sres.Net.EEIP.CIP.Path
{
    using Sres.Net.EEIP.CIP.IO;

    public record ProductionInhibitTimeSegment :
        NetworkSegment
    {
        public ProductionInhibitTimeSegment(byte value) :
            base(NetworkType.ProductionInhibitTime)
            => this.Value = value;

        /// <summary>
        /// Minimum time, in milliseconds, between successive transmissions of connected data for the specified connection.
        /// For example, if a production inhibit time of 10 milliseconds is specified, new data shall be sent no sooner than 10 milliseconds after the previous data.
        /// 0 means no production inhibit time when a default value of <see cref="IOConnection.RequestedPacketRate"/> / 4 shall be used.
        /// </summary>
        public byte Value { get; init; }
        
        public override bool Skip => Value == 0;

        public override ushort DataCount => 1;

        protected override void DataToBytes(byte[] bytes, ref int index)
            => bytes[index++] = Value;
    }
}
