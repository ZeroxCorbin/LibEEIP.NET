using System;
using System.Collections.Generic;
using Sres.Net.EEIP.CIP.ObjectLibrary;
using Sres.Net.EEIP.Data;

namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Table 2-4.4 CIP Identity Item
    /// </summary>
    public record IdentityItem :
        Item
    {
        public IdentityItem(IdentityInstance identity, SocketAddress socketAddress) :
            base(Type)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            SocketAddress = socketAddress ?? throw new ArgumentNullException(nameof(socketAddress));
        }

        internal IdentityItem(IReadOnlyList<byte> bytes, ref int index) :
            base(Type)
        {
            EncapsulationProtocolVersion = bytes.ToUshort(ref index);
            SocketAddress = new(bytes, ref index);
            Identity = new(bytes, ref index);
            State = bytes.ToByte(ref index, nameof(State));
        }

        public new const ushort Type = 0x0C;

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

        public override ushort DataLength => (ushort)(3 + SocketAddress.ByteCountStatic + Identity?.ByteCount ?? IdentityInstance.ByteCountStatic);

        protected override void AddData(byte[] bytes, ref int index)
        {
            EncapsulationProtocolVersion.ToBytes(bytes, ref index);
            SocketAddress.ToBytes(bytes, ref index);
            Identity.ToBytes(bytes, ref index);
            State.ToBytes(bytes, ref index);
        }
    }
}