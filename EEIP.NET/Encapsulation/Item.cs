namespace Sres.Net.EEIP.Encapsulation
{
    using System;
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// <see cref="CommonPacket"/> item (Table 2-6.2)
    /// </summary>
    public abstract record Item :
        Byteable
    {
        protected Item(ushort type)
            => this.Type = type;

        public static Item From(IReadOnlyList<byte> bytes, ref int index)
        {
            var type = bytes.ToUshort(ref index);
            var dataLength = bytes.ToUshort(ref index);
            bytes.ValidateEnoughBytes(MinByteCount - 4 + dataLength, nameof(Item) + " data", index);
            var data = dataLength == 0 ?
                null :
                bytes.Segment(ref index, dataLength);
            int localIndex = 0;
            Item result = type switch
            {
                NullAddressItem.Type => NullAddressItem.Instance,
                (ushort)DataItemType.Connected or
                (ushort)DataItemType.Unconnected
                => new DataItem((DataItemType)type, new Bytes(data)),
                (ushort)SocketAddressItemType.OriginatorToTarget or
                (ushort)SocketAddressItemType.TargetToOriginator
                => new SocketAddressItem((SocketAddressItemType)type, new SocketAddress(data, ref localIndex)),
                IdentityItem.Type => new IdentityItem(data, ref localIndex),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported item type " + type),
            };
            index += dataLength;
            return result;
        }

        public ushort Type { get; }

        public const byte MinByteCount = 4;
        public sealed override ushort ByteCount => (ushort)(MinByteCount + DataLength);
        public abstract ushort DataLength { get; }
        protected ushort? DataLengthFromBytes { get; }

        protected sealed override void DoToBytes(byte[] bytes, ref int index)
        {
            Type.ToBytes(bytes, ref index);
            DataLength.ToBytes(bytes, ref index);
            AddData(bytes, ref index);
        }

        protected abstract void AddData(byte[] bytes, ref int index);
    }
}
