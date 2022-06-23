namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// Implicit connection originator = IO Scanner = client
    /// </summary>
    public record Originator :
        IOConnectionEndPoint
    {
        public Originator(
            ushort port = DefaultPort,
            ushort vendorId = 0xFF,
            uint serialNumber = ushort.MaxValue) :
            base(port)
        {
            this.VendorId = vendorId;
            this.SerialNumber = serialNumber;
        }

        public ushort VendorId { get; init; }
        public uint SerialNumber { get; init; }
    }
}
