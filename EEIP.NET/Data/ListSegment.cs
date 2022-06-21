namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ListSegment<T> :
        IReadOnlyList<T>
    {
        public ListSegment(IReadOnlyList<T> list, int startIndex, int? count = null)
        {
            list.Validate(nameof(list), startIndex, nameof(startIndex), ref count);
            List = list;
            StartIndex = startIndex;
            Count = count.Value;
        }

        public IReadOnlyList<T> List { get; }
        public int StartIndex { get; }
        public int Count { get; }
        public T this[int index] => List[StartIndex + index];

        public IEnumerator<T> GetEnumerator() => List.
            Skip(StartIndex).
            Take(Count).
            GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
