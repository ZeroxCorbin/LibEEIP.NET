namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    public record IdentityInstance
    {
        /// <summary>
        /// Device manufacturers Vendor ID
        /// </summary>
        public ushort VendorId { get; init; }
        /// <summary>
        /// Device Type of product
        /// </summary>
        public ushort DeviceType { get; init; }
        /// <summary>
        /// Product Code assigned with respect to device type
        /// </summary>
        public ushort ProductCode { get; init; }
        /// <summary>
        /// Device revision
        /// </summary>
        public Revision Revision { get; init; }
        /// <summary>
        /// Current status of device
        /// </summary>
        public ushort Status { get; init; }
        /// <summary>
        /// Serial number of device
        /// </summary>
        public uint SerialNumber { get; init; }
        /// <summary>
        /// Human readable description of device
        /// </summary>
        public string ProductName { get; init; }
    }
}
