namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Byteables
    {
        public static byte[] ToBytes(this IByteable item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            var bytes = new byte[item.ByteCount];
            int index = 0;
            item.ToBytes(bytes, ref index);
            return bytes;
        }

        public static IReadOnlyList<byte> ToBytesReadOnly(this IByteable item) => item is Bytes bytes ?
            bytes.Data :
            item.ToBytes();

        public static BytesConcatenation Concat(this IByteable item1, IByteable item2)
        {
            if (item1 is null)
                throw new ArgumentNullException(nameof(item1));
            if (item2 is null)
                throw new ArgumentNullException(nameof(item2));
            return new BytesConcatenation(item1, item2);
        }

        public static byte ToByte(this IReadOnlyList<byte> bytes, ref int index, string name = null)
        {
            bytes.ValidateEnoughBytes(1, name ?? nameof(Byte), index);
            var result = bytes[index++];
            return result;
        }

        /// <summary>
        /// Converts <paramref name="bytes"/> (received e.g. via <see cref="EEIPClient.GetAttributeSingle"/>) to ushort
        /// </summary>
        /// <param name="bytes">Bytes to convert</param> 
        public static ushort ToUshort(this IReadOnlyList<byte> bytes, ref int index, bool littleEndian = true, string name = null)
        {
            bytes.ValidateEnoughBytes(2, name ?? nameof(UInt16), index);
            var bytes2 = (bytes[index++], bytes[index++]);
            if (!littleEndian)
                bytes2 = (bytes2.Item2, bytes2.Item1);
            ushort result = bytes2.Item1;
            result |= (ushort)(bytes2.Item2 << 8);
            return result;
        }

        /// <summary>
        /// Converts <paramref name="bytes"/> (received e.g. via <see cref="EEIPClient.GetAttributeSingle"/>) to uint
        /// </summary>
        /// <param name="bytes">Bytes to convert</param> 
        public static uint ToUint(this IReadOnlyList<byte> bytes, ref int index, bool littleEndian = true, string name = null)
        {
            bytes.ValidateEnoughBytes(4, name ?? nameof(UInt32), index);
            var bytes4 = (bytes[index++], bytes[index++], bytes[index++], bytes[index++]);
            if (!littleEndian)
                bytes4 = (bytes4.Item4, bytes4.Item3, bytes4.Item2, bytes4.Item1);
            uint result = bytes4.Item1;
            result |= (uint)bytes4.Item2 << 8;
            result |= (uint)bytes4.Item3 << 16;
            result |= (uint)bytes4.Item4 << 24;
            return result;
        }

        /// <summary>
        /// Returns the "Bool" State of <paramref name="byte"/> (received e.g. via <see cref="EEIPClient.GetAttributeSingle"/>)
        /// </summary>
        /// <param name="byte">Byte to convert</param>
        /// <param name="bitPosition">Bit position to convert (First bit = bitposition 0)</param> 
        /// <returns>Converted bool value</returns>
        public static bool ToBool(this byte @byte, int bitPosition)
            => ((@byte >> bitPosition) & 0x01) != 0;

        public static Byte AsByteable(this byte value) => new Byte(value);
        public static Ushort AsByteable(this ushort value) => new Ushort(value);
        public static Uint AsByteable(this uint value) => new Uint(value);

        public static ushort ByteCount(this IEnumerable<IByteable> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));
            return (ushort)items.Sum(i => i.ByteCount);
        }

        public static void ToBytes(this IEnumerable<IByteable> items, byte[] bytes, ref int index)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
                item.ToBytes(bytes, ref index);
        }

        public static void Copy(this IReadOnlyList<byte> source, ref int sourceIndex, byte[] target, int targetIndex, int count)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            if (source is byte[] array)
                Array.Copy(array, sourceIndex, target, targetIndex, count);
            else
            {
                for (int i = sourceIndex; i < sourceIndex + count; i++)
                    target[targetIndex + i] = source[i];
            }
            sourceIndex += count;
        }

        public static IReadOnlyList<byte> Segment(this IReadOnlyList<byte> bytes, ref int index, int? count = null)
        {
            bytes.Validate(nameof(bytes), index, nameof(index), ref count);
            if (count.Value == 0)
                return Bytes.EmptyArray;
            index += count.Value;
            return bytes is byte[] array ?
                new ArraySegment<byte>(array, index, count.Value) :
                new ListSegment<byte>(bytes, index, count.Value);
        }

        public static void ValidateEnoughBytes(this IReadOnlyList<byte> bytes, int length, string purpose, int index = 0)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Count - index < length)
                throw new ArgumentException("Not enough bytes for " + purpose, nameof(bytes));
        }

        public static void Validate<T>(this IReadOnlyList<T> list, string listName, int startIndex, string startIndexName, ref int? count)
        {
            if (list is null)
                throw new ArgumentNullException(listName);
            ValidateIndex(list, startIndex, startIndexName);
            if (count.HasValue)
                ValidateIndex(list, startIndex + count.Value, $"{startIndexName} + {nameof(count)}");
            else
                count = list.Count - startIndex;
        }

        private static void ValidateIndex<T>(IReadOnlyList<T> list, int index, string name)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(name, index, $"Index {index} is negative");
            if (index >= list.Count)
                throw new ArgumentOutOfRangeException(name, index, $"Index {index} must be < " + list.Count);
        }
    }
}
