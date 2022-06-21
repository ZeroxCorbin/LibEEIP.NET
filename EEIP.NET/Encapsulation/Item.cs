namespace Sres.Net.EEIP.Encapsulation
{
    using System;
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    public abstract record Item :
        Byteable
    {
        protected Item(ushort type)
            => this.Type = type;

        public ushort Type { get; init; }
        public sealed override ushort ByteCount => (ushort)(4 + DataLength);
        public abstract ushort DataLength { get; }

        protected sealed override void DoToBytes(byte[] bytes, ref int index)
        {
            Type.AsByteable().ToBytes(bytes, ref index);
            DataLength.AsByteable().ToBytes(bytes, ref index);
            AddData(bytes, ref index);
        }

        protected abstract void AddData(byte[] bytes, ref int index);

        public static Item From(IReadOnlyList<byte> bytes, ref int index)
        {
            var type = bytes.ToUshort(ref index);
            var dataLength = bytes.ToUshort(ref index);
            bytes.ValidateEnoughBytes(4 + dataLength, nameof(Item) + " data", index);
            var data = dataLength == 0 ?
                null :
                bytes.Segment(ref index, dataLength);
            return type switch
            {
                NullAddressItem.Type => NullAddressItem.Instance,
                (ushort)DataItemType.Connected or
                (ushort)DataItemType.Unconnected => new DataItem((DataItemType)type, new Bytes(data)),
                (ushort)SocketAddressItemType.OriginatorToTarget or
                (ushort)SocketAddressItemType.TargetToOriginator => new SocketAddressItem((SocketAddressItemType)type,new SocketAddress(data)),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported item type " + type)
            };
        }
    }
}
