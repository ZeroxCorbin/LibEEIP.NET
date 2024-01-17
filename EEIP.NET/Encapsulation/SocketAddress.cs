using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Sres.Net.EEIP.Data;

namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Socket Address
    /// </summary>
    public record SocketAddress :
        Byteable
    {
        [JsonConstructor]
        public SocketAddress() => EndPoint = new IPEndPoint(IPAddress.Any, 0);

        public SocketAddress(uint address, ushort port) => EndPoint = new(GetAddress(address), port);

        public SocketAddress(IReadOnlyList<byte> bytes, ref int index)
        {
            bytes.ValidateEnoughBytes(ByteCount, nameof(SocketAddress), index);
            index += 2; // skip family
            var port = bytes.ToUshort(ref index, false);
            var address = bytes.ToUint(ref index, false);
            EndPoint = new(
                GetAddress(address),
                port);
        }

        [JsonIgnore]
        public IPEndPoint EndPoint { get; }
        [JsonIgnore]
        public IPAddress IPAddress => EndPoint.Address;
        public string Address { get => EndPoint.Address.ToString(); set => EndPoint.Address = IPAddress.Parse(value); }

        public ushort Port { get=> (ushort)EndPoint.Port; set => EndPoint.Port = value; }

        public ushort Family => (ushort)EndPoint.AddressFamily;

        public static IPAddress GetAddress(uint address) => new IPAddress(address);

        public static uint GetAddress(IPAddress address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
        }

        public const int ByteCountStatic = 16;
        public override ushort ByteCount => ByteCountStatic;

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            // sin_family
            Family.ToBytes(bytes, ref index, false);
            // sin_port
            Port.ToBytes(bytes, ref index, false);
            // sin_addr
            var address = IPAddress.GetAddressBytes();
            bytes[index++] = address[3];
            bytes[index++] = address[2];
            bytes[index++] = address[1];
            bytes[index++] = address[0];
            // sin_zero
            Zero.ToBytes(bytes, ref index);
        }

        private static readonly Bytes Zero = new(0, 0, 0, 0, 0, 0, 0, 0);
    }
}
