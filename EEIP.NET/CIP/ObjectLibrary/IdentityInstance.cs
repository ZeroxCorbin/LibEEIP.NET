namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System;
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// <see cref="Assembly"/> instance.
    /// CIP Table 5-2.2 Identity Object Instance Attributes.
    /// </summary>
    public record IdentityInstance :
        Byteable
    {
        public IdentityInstance()
        { }

        public IdentityInstance(IReadOnlyList<byte> bytes, ref int index) :
            base(bytes, ref index)
        {
            VendorId = bytes.ToUshort(ref index);
            DeviceType = bytes.ToUshort(ref index);
            ProductCode = bytes.ToUshort(ref index);
            Revision = new(bytes, ref index);
            Status = bytes.ToUshort(ref index);
            SerialNumber = bytes.ToUint(ref index);
            ProductName = bytes.ToString(ref index, nameof(ProductName));
            State = (IdentityState)bytes.ToByte(ref index, nameof(State));
        }

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
        public Revision Revision
        {
            get => revision;
            init => revision = value ?? throw new ArgumentNullException(nameof(Revision));
        }
        /// <summary>
        /// Current status of device
        /// </summary>
        public ushort Status { get; init; }
        /// <summary>
        /// Serial number of device
        /// </summary>
        public uint SerialNumber { get; init; }
        public const int ProductNameLengthOffset = 14;
        /// <summary>
        /// Human readable description of device
        /// </summary>
        public string ProductName { get; init; }
        /// <summary>
        /// State
        /// </summary>
        public IdentityState State { get; init; }

        public const int ByteCountStatic = 16;
        public override ushort ByteCount => (ushort)(ByteCountStatic + ProductName?.Length ?? 0);

        private Revision revision;

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            VendorId.ToBytes(bytes, ref index);
            DeviceType.ToBytes(bytes, ref index);
            ProductCode.ToBytes(bytes, ref index);
            Revision.ToBytes(bytes, ref index);
            Status.ToBytes(bytes, ref index);
            SerialNumber.ToBytes(bytes, ref index);
            ProductName.ToBytes(bytes, ref index);
            ((byte)State).ToBytes(bytes, ref index);
        }
    }
}
