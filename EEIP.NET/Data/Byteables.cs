namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// <see cref="IByteable"/> helper
    /// </summary>
    public static class Byteables
    {
        #region IByteable

        /// <summary>
        /// Converts <paramref name="item"/> to bytes
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Bytes</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null</exception>
        public static byte[] ToBytes(this IByteable item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            var bytes = new byte[item.ByteCount];
            int index = 0;
            item.ToBytes(bytes, ref index);
            return bytes;
        }

        /// <summary>
        /// Converts <paramref name="item"/> to read only bytes
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Bytes</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null</exception>
        public static IReadOnlyList<byte> ToBytesReadOnly(this IByteable item) => item is Bytes bytes ?
            bytes.Data :
            item.ToBytes();

        /// <summary>
        /// Concatenates two items
        /// </summary>
        /// <param name="item1">First item</param>
        /// <param name="item2">Second item</param>
        /// <returns>Concatenation</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item1"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="item2"/> is null</exception>
        public static BytesConcatenation Concat(this IByteable item1, IByteable item2)
        {
            if (item1 is null)
                throw new ArgumentNullException(nameof(item1));
            if (item2 is null)
                throw new ArgumentNullException(nameof(item2));
            return new BytesConcatenation(item1, item2);
        }

        /// <summary>
        /// Sums <paramref name="items"/>` <see cref="IByteCount.ByteCount"/>s
        /// </summary>
        /// <param name="items">Items</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is null</exception>
        public static ushort ByteCount(this IEnumerable<IByteable> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));
            return (ushort)items.Sum(i => i.ByteCount);
        }

        /// <summary>
        /// Writes <paramref name="items"/> to <paramref name="bytes"/> starting at <paramref name="index"/>
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="bytes">Bytes</param>
        /// <param name="index">Index incremented by <see cref="ByteCount"/> after writing</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is null</exception>
        public static void ToBytes(this IEnumerable<IByteable> items, byte[] bytes, ref int index)
        {
            bytes.ValidateEnoughBytes(items.ByteCount(), nameof(items), index);
            foreach (var item in items)
                item.ToBytes(bytes, ref index);
        }
        
        public static Byte AsByteable(this byte value) => new Byte(value);
        public static Ushort AsByteable(this ushort value) => new Ushort(value);
        public static Uint AsByteable(this uint value) => new Uint(value);

        #endregion

        #region ToBytes

        public static byte[] ToBytes<T>(this T value, Func<T, byte[]> convert, bool littleEndian = true)
            where T : struct
        {
            if (convert is null)
                throw new ArgumentNullException(nameof(convert));
            var bytes = convert(value);
            if (BitConverter.IsLittleEndian != littleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static byte[] ToBytes(this int value, bool littleEndian = true) => value.ToBytes(BitConverter.GetBytes, littleEndian);
        public static byte[] ToBytes(this uint value, bool littleEndian = true) => value.ToBytes(BitConverter.GetBytes, littleEndian);
        public static byte[] ToBytes(this short value, bool littleEndian = true) => value.ToBytes(BitConverter.GetBytes, littleEndian);
        public static byte[] ToBytes(this ushort value, bool littleEndian = true) => value.ToBytes(BitConverter.GetBytes, littleEndian);
        public static byte[] ToBytes(this byte value) => new[] { value };
        public static byte[] ToBytes(this sbyte value) => new[] { (byte)value };

        public static void ToBytes<T>(this T value, byte[] bytes, ref int index, Func<T, byte[]> convert, bool littleEndian = true, string name = null)
            where T : struct
            => value.
                ToBytes(convert, littleEndian).
                ToBytes(bytes, ref index, name: name ?? typeof(T).Name);

        public static void ToBytes(this int value, byte[] bytes, ref int index, bool littleEndian = true, string name = null)
            => value.ToBytes(bytes, ref index, BitConverter.GetBytes, littleEndian, name);

        public static void ToBytes(this uint value, byte[] bytes, ref int index, bool littleEndian = true, string name = null)
            => value.ToBytes(bytes, ref index, BitConverter.GetBytes, littleEndian, name);

        public static void ToBytes(this short value, byte[] bytes, ref int index, bool littleEndian = true, string name = null)
           => value.ToBytes(bytes, ref index, BitConverter.GetBytes, littleEndian, name);

        public static void ToBytes(this ushort value, byte[] bytes, ref int index, bool littleEndian = true, string name = null)
           => value.ToBytes(bytes, ref index, BitConverter.GetBytes, littleEndian, name);

        public static void ToBytes(this byte value, byte[] bytes, ref int index, string name = null)
        {
            bytes.ValidateEnoughBytes(1, name ?? nameof(Byte), index);
            bytes[index++] = value;
        }

        public static void ToBytes(this sbyte value, byte[] bytes, ref int index, string name = null) => ((byte)value).ToBytes(bytes, ref index, name ?? nameof(SByte));

        public static void ToBytes(this string value, byte[] bytes, ref int index, string name = null)
        {
            name ??= nameof(String);
            byte length = Convert.ToByte(value?.Length ?? 0);
            length.ToBytes(bytes, ref index, $"{name}.{nameof(string.Length)}");
            if (length > 0)
            {
                var text = Encoding.UTF8.GetBytes(value);
                text.ToBytes(bytes, ref index, name: name);
            }
        }

        public static void ToBytes(this IReadOnlyList<byte> source, byte[] target, ref int targetIndex, int sourceIndex = 0, int? count = null, string name = null)
        {
            Validate(source, nameof(source), sourceIndex, nameof(sourceIndex), ref count);
            if (count == 0)
                return;
            target.ValidateEnoughBytes(count.Value, name ?? nameof(source), targetIndex);
            foreach (var sourceByte in source)
                target[targetIndex++] = sourceByte;
        }

        #endregion

        #region To Type

        public static byte[] ToBytes(this IReadOnlyList<byte> source, ref int sourceIndex, byte[] target = null, int targetIndex = 0, int? count = null)
        {
            Validate(source, nameof(source), sourceIndex, nameof(sourceIndex), ref count);
            if (target is null)
                target = new byte[count.Value];
            if (count > 0)
            {
                if (source is byte[] array)
                    Array.Copy(array, sourceIndex, target, targetIndex, count.Value);
                else
                {
                    for (int i = 0; i < count; i++)
                        target[targetIndex + i] = source[sourceIndex + i];
                }
                sourceIndex += count.Value;
            }
            return target;
        }
        
        public static string ToString(this IReadOnlyList<byte> bytes, ref int index, string name = null)
        {
            byte length = bytes.ToByte(ref index, $"{name ?? nameof(String)}.{nameof(string.Length)}");
            string result;
            if (length > 0)
            {
                var text = bytes.ToBytes(ref index, count: length);
                result = Encoding.UTF8.GetString(text);
            }
            else
                result = null;
            return result;
        }

        /// <summary>
        /// Converts 1 byte from <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to byte
        /// </summary>
        /// <param name="bytes">Bytes to convert</param> 
        /// <param name="index">Starting index</param>
        /// <param name="name">Result name for <see cref="ValidateEnoughBytes"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have enough bytes</exception>
        public static byte ToByte(this IReadOnlyList<byte> bytes, ref int index, string name = null)
        {
            bytes.ValidateEnoughBytes(1, name ?? nameof(Byte), index);
            var result = bytes[index++];
            return result;
        }

        /// <summary>
        /// Converts 1 byte from <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to sbyte
        /// </summary>
        /// <param name="bytes">Bytes to convert</param> 
        /// <param name="index">Starting index</param>
        /// <param name="name">Result name for <see cref="ValidateEnoughBytes"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have enough bytes</exception>
        public static sbyte ToSbyte(this IReadOnlyList<byte> bytes, ref int index, string name = null)
        {
            bytes.ValidateEnoughBytes(1, name ?? nameof(SByte), index);
            var result = (sbyte)bytes[index++];
            return result;
        }

        /// <summary>
        /// Converts <paramref name="count"/> <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to <typeparamref name="T"/>
        /// </summary>
        /// <param name="bytes">Bytes to convert</param>
        /// <param name="index">Starting index</param>
        /// <param name="count">Number of bytes to convert</param>
        /// <param name="convert">Conversion method from <see cref="BitConverter"/>.ToType</param>
        /// <param name="littleEndian">Byte order</param>
        /// <param name="name">Result name for <see cref="ValidateEnoughBytes"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="convert"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have enough bytes</exception>
        public static T To<T>(this IReadOnlyList<byte> bytes, ref int index, byte count, Func<byte[], int, T> convert, bool littleEndian = true, string name = null)
            where T : struct
        {
            bytes.ValidateEnoughBytes(count, name ?? nameof(T), index);
            if (convert is null)
                throw new ArgumentNullException(nameof(convert));
            var array = new byte[count];
            if (BitConverter.IsLittleEndian == littleEndian)
            {
                for (int i = 0; i < count; i++)
                    array[i] = bytes[index + i];
            }
            else
            {
                for (int i = 0; i < count; i++)
                    array[count - i - 1] = bytes[index + 1];
            }
            T result = convert(array, 0);
            index += count;
            return result;
        }

        /// <summary>
        /// Converts 2 <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to ushort
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
        /// Converts 2 <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to short
        /// </summary>
        /// <param name="bytes">Bytes to convert</param> 
        public static short ToShort(this IReadOnlyList<byte> bytes, ref int index, bool littleEndian = true, string name = null)
            => bytes.To(ref index, 2, BitConverter.ToInt16, littleEndian, name);

        public static short ToShort(this IReadOnlyList<byte> bytes, int index = 0, bool littleEndian = true, string name = null)
            => bytes.ToShort(ref index, littleEndian, name);

        /// <summary>
        /// Converts 4 <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to uint
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

        public static uint ToUint(this IReadOnlyList<byte> bytes, int index = 0, bool littleEndian = true, string name = null)
            => bytes.ToUint(ref index, littleEndian, name);

        /// <summary>
        /// Converts 4 <paramref name="bytes"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>) to int
        /// </summary>
        /// <param name="bytes">Bytes to convert</param> 
        public static int ToInt(this IReadOnlyList<byte> bytes, ref int index, bool littleEndian = true, string name = null)
            => bytes.To(ref index, 4, BitConverter.ToInt32, littleEndian, name);

        public static int ToInt(this IReadOnlyList<byte> bytes, int index = 0, bool littleEndian = true, string name = null)
            => bytes.ToInt(ref index, littleEndian, name);

        /// <summary>
        /// Returns the "Bool" State of <paramref name="byte"/> (received e.g. via <see cref="EIPClient.GetAttributeSingle"/>)
        /// </summary>
        /// <param name="byte">Byte to convert</param>
        /// <param name="bitPosition">Bit position to convert (First bit = bitposition 0)</param> 
        /// <returns>Converted bool value</returns>
        public static bool ToBool(this byte @byte, int bitPosition)
            => ((@byte >> bitPosition) & 0x01) != 0;

        public static ByteToUshortList ToUshortListWithByteCount(this IReadOnlyList<byte> bytes, ref int index, string name = null)
        {
            name ??= nameof(ByteToUshortList);
            var count = bytes.ToByte(ref index, $"{name}.{nameof(ByteToUshortList.Count)}");
            return bytes.ToUshortList(ref index, count, name);
        }

        public static ByteToUshortList ToUshortListWithUshortCount(this IReadOnlyList<byte> bytes, ref int index, string name = null)
        {
            name ??= nameof(ByteToUshortList);
            var count = bytes.ToUshort(ref index, name: $"{name}.{nameof(ByteToUshortList.Count)}");
            return bytes.ToUshortList(ref index, count, name);
        }

        public static ByteToUshortList ToUshortList(this IReadOnlyList<byte> bytes, ref int index, ushort itemCount, string name = null)
        {
            int byteCount = itemCount * 2;
            ValidateEnoughBytes(bytes, byteCount, name ?? nameof(ByteToUshortList), index);
            var data = bytes.Segment(ref index, byteCount);
            return ByteToUshortList.From(data);
        }

        #endregion

        #region Segment

        public static IReadOnlyList<byte> Segment(this IReadOnlyList<byte> bytes, int index = 0, int? count = null) => bytes.Segment(ref index, count);

        public static IReadOnlyList<byte> Segment(this IReadOnlyList<byte> bytes, ref int index, int? count = null)
        {
            if (index == 0 &&
                (
                    count == null ||
                    (bytes?.Count ?? throw new ArgumentNullException(nameof(bytes))) == count
                ))
            {
                return bytes;
            }
            bytes.Validate(nameof(bytes), index, nameof(index), ref count);
            if (count.Value == 0)
                return Bytes.EmptyArray;
            IReadOnlyList<byte> result = bytes is byte[] array ?
                new ArraySegment<byte>(array, index, count.Value) :
                new ListSegment<byte>(bytes, index, count.Value);
            index += count.Value;
            return result;
        }

        #endregion

        #region Validate

        /// <summary>
        /// Validates whether <paramref name="bytes"/> has at least <paramref name="length"/> bytes starting at <paramref name="index"/> for <paramref name="purpose"/>
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="length">Required byte count</param>
        /// <param name="purpose">Purpose of required bytes</param>
        /// <param name="index">Starting index</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have enough bytes</exception>
        public static void ValidateEnoughBytes(this IReadOnlyList<byte> bytes, int length, string purpose, int index = 0)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index {index} is negative");
            if (bytes.Count - index < length)
                throw new ArgumentException("Not enough bytes for " + purpose, nameof(bytes));
        }

        public static void Validate<T>(this IReadOnlyList<T> list, string listName, int startIndex, string startIndexName, ref int? count)
        {
            if (list is null)
                throw new ArgumentNullException(listName);
            var hadCount = count.HasValue;
            count ??= list.Count - startIndex;
            if (count == 0)
                return;
            ValidateIndex(list, startIndex, startIndexName);
            if (hadCount)
                ValidateIndex(list, startIndex + count.Value - 1, $"{startIndexName} + {nameof(count)}");
        }

        private static void ValidateIndex<T>(IReadOnlyList<T> list, int index, string name)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(name, index, $"Index {index} is negative");
            if (index >= list.Count)
                throw new ArgumentOutOfRangeException(name, index, $"Index {index} must be < " + list.Count);
        }

        #endregion
    }
}
