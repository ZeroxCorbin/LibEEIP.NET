namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// Implicit connection originator = IO Scanner = client
    /// </summary>
    public record Originator
    {
        public Originator(OriginatorToTargetConnection connectionToTarget, ushort vendorId = 0xFF, uint serialNumber = ushort.MaxValue)
        {
            this.ConnectionToTarget = connectionToTarget;
            this.VendorId = vendorId;
            this.SerialNumber = serialNumber;
        }

        public ushort VendorId { get; init; }
        public uint SerialNumber { get; init; }
        public OriginatorToTargetConnection ConnectionToTarget
        {
            get => connectionToTarget;
            init => connectionToTarget = value ?? throw new System.ArgumentNullException(nameof(connectionToTarget));
        }

        private OriginatorToTargetConnection connectionToTarget;
    }
}
