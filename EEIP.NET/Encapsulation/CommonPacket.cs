using System;
using System.Collections.Generic;
using System.Linq;
using Sres.Net.EEIP.Data;

namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Common Packet Format (Table 2-6.1)
    /// </summary>
    public record CommonPacket :
        Byteable
    {
        public CommonPacket(AddressItem address, DataItem data, params Item[] items)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (items?.Length > 0)
            {
                var localItems = new Item[2 + items.Length];
                this.Items = localItems;
                localItems[0] = address;
                localItems[1] = data;
                Array.Copy(items, 0, localItems, 2, items.Length);
            }
            else
            {
                this.Items = new Item[2]
                {
                    address,
                    data
                };
            }
        }

        public CommonPacket(DataItem data, params Item[] items) :
            this(NullAddressItem.Instance, data, items)
        { }

        public CommonPacket(IReadOnlyList<byte> bytes, ref int index) :
            base(bytes, ref index)
        {
            var itemCount = bytes.ToUshort(ref index);
            var items = new Item[itemCount];
            Items = items;
            for (int i = 0; i < itemCount; i++)
                items[i] = Item.From(bytes, ref index);
        }

        public IReadOnlyList<Item> Items { get; }
        public ushort ItemCount => (ushort)Items.Count;

        public AddressItem Address => (AddressItem)Items.First();
        public DataItem Data => (DataItem)Items.Skip(1).First();
        public IEnumerable<Item> OptionalItems => Items.Skip(2);

        public IdentityItem IdentityItem => OptionalItems.
            OfType<IdentityItem>().
            SingleOrDefault();

        public SocketAddressItem GetSocketAddress(SocketAddressItemType type) => OptionalItems.
            OfType<SocketAddressItem>().
            SingleOrDefault(i => i.Type == (ushort)type);

        public override ushort ByteCount => (ushort)(
            2 + 
            Items?.ByteCount() ??
            Item.MinByteCount * 2 // Address + Data
            );

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            ItemCount.ToBytes(bytes, ref index);
            Items.ToBytes(bytes, ref index);
        }
    }
}
