using System;
using System.Text;
using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Table 2-4.4 CIP Identity Item
    /// </summary>
    public record IdentityItem
    {
        public IdentityItem(IdentityInstance identity, SocketAddress socketAddress)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            SocketAddress = socketAddress ?? throw new ArgumentNullException(nameof(socketAddress));
        }

        ///// <summary>
        ///// Code indicating item type of CIP Identity (0x0C)
        ///// </summary>
        //public ushort ItemTypeCode { get; init; }
        ///// <summary>
        ///// Number of bytes in item which follow (length varies depending on Product Name string)
        ///// </summary>
        //public ushort ItemLength { get; init; }
        /// <summary>
        /// Encapsulation Protocol Version supported (also returned with Register Session reply)
        /// </summary>
        public ushort EncapsulationProtocolVersion { get; init; }
        /// <summary>
        /// Socket Address (see section 2-6.3.2)
        /// </summary>
        public SocketAddress SocketAddress { get; }
        /// <summary>
        /// Identity
        /// </summary>
        public IdentityInstance Identity { get; }
        /// <summary>
        /// Current state of device
        /// </summary>
        public byte State { get; init; }

        internal static IdentityItem From(int startingByte, byte[] receivedData)
        {
            startingByte += 2;    //Skipped ItemCount
            var productNameLength = receivedData[36 + startingByte];
            var identity = new IdentityInstance
            {
                VendorId = Convert.ToUInt16(receivedData[22 + startingByte]
                | receivedData[23 + startingByte] << 8),
                DeviceType = Convert.ToUInt16(receivedData[24 + startingByte]
                | receivedData[25 + startingByte] << 8),
                ProductCode = Convert.ToUInt16(receivedData[26 + startingByte]
                | receivedData[27 + startingByte] << 8),
                Status = Convert.ToUInt16(receivedData[30 + startingByte]
                | receivedData[31 + startingByte] << 8),
                SerialNumber = (uint)(receivedData[32 + startingByte]
                | receivedData[33 + startingByte] << 8
                | receivedData[34 + startingByte] << 16
                | receivedData[35 + startingByte] << 24),
                ProductName = Encoding.ASCII.GetString(receivedData, 37 + startingByte, productNameLength),
                Revision = new() {
                    Major = receivedData[28 + startingByte],
                    Minor = receivedData[29 + startingByte]
                }
            };
            var socketAddress = new SocketAddress(
                (uint)(receivedData[13 + startingByte]
                | receivedData[12 + startingByte] << 8
                | receivedData[11 + startingByte] << 16
                | receivedData[10 + startingByte] << 24),
                // Family is IPv4
                // Family = Convert.ToUInt16(receivedData[7 + startingByte]
                // | (receivedData[6 + startingByte] << 8))
                Convert.ToUInt16(receivedData[9 + startingByte]
                | receivedData[8 + startingByte] << 8));
            var result = new IdentityItem(identity, socketAddress)
            {
                //ItemTypeCode = Convert.ToUInt16(receivedData[0 + startingByte]
                //| receivedData[1 + startingByte] << 8),
                //ItemLength = Convert.ToUInt16(receivedData[2 + startingByte]
                //| receivedData[3 + startingByte] << 8),
                EncapsulationProtocolVersion = Convert.ToUInt16(receivedData[4 + startingByte]
                | receivedData[5 + startingByte] << 8),
                State = receivedData[receivedData.Length - 1]
            };
            return result;
        }
    }
}