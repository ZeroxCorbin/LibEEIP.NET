namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ByteToUshortList :
        IReadOnlyList<ushort>
    {
        public ByteToUshortList(IReadOnlyList<byte> list)
        {
            List = list ?? throw new ArgumentNullException(nameof(list));
            if (list.Count % 2 > 0)
                throw new ArgumentOutOfRangeException(nameof(list.Count), list.Count, "List count must be even");
        }

        public IReadOnlyList<byte> List { get; }
        public int Count => List.Count / 2;
        public ushort this[int index]
        {
            get
            {
                index *= 2;
                return List.ToUshort(ref index);
            }
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            for (int i = 0; i < List.Count; i++)
                yield return List.ToUshort(ref i);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
