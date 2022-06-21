using System;
using System.Collections.Generic;
using System.Net;
using Sres.Net.EEIP.Data;

namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Socket Address
    /// </summary>
    public record SocketAddress :
        Byteable
    {
        public SocketAddress(uint address, ushort port)
        {
            Address = GetAddress(address);
            Port = port;
        }

        public SocketAddress(IReadOnlyList<byte> bytes, int index = 0)
        {
            index += 2; // skip family
            bytes.ValidateEnoughBytes(16, nameof(SocketAddress), index);
            Port = bytes.ToUshort(ref index, false);
            var address = bytes.ToUint(ref index, false);
            Address = GetAddress(address);
        }

        public IPAddress Address { get; }
        public ushort Port { get; init; }

        public static IPAddress GetAddress(uint address) => new IPAddress(address);

        public static uint GetAddress(IPAddress address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
        }

        public override ushort ByteCount => 16;

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            // sin_family
            bytes[index++] = (byte)(((ushort)Address.AddressFamily) >> 8);
            bytes[index++] = (byte)Address.AddressFamily;
            // sin_port
            bytes[index++] = (byte)(Port >> 8);
            bytes[index++] = (byte)Port;
            // sin_addr
            var address = Address.GetAddressBytes();
            bytes[index++] = address[3];
            bytes[index++] = address[2];
            bytes[index++] = address[1];
            bytes[index++] = address[0];
            // sin_zero
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
            bytes[index++] = byte.MinValue;
        }
    }
}
