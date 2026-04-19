using System.Collections;
using System.Collections.Generic;

namespace Mmang.Util
{

    public struct ReadOnlyListCast<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> m_Source;

        public ReadOnlyListCast(IReadOnlyList<T> source)
        {
            m_Source = source;
        }

        public readonly T this[int index] => m_Source[index];

        public readonly int Count => m_Source.Count;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in m_Source)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class ListUtil
    {
        public static bool TryGetFirst<T>(this IEnumerable<T> enumerable, out T firstValue)
        {
            var it = enumerable.GetEnumerator();
            if (it.MoveNext())
            {
                firstValue = it.Current;
                return true;
            }

            firstValue = default;
            return false;
        }

        public static bool IsNotEmpty(this IEnumerable enumerable)
        {
            var it = enumerable.GetEnumerator();
            return it.MoveNext();
        }

        public delegate bool ListGetIndexCondition<T>(T element);
        public static int ConditionalIndexOf<T>(this List<T> list, ListGetIndexCondition<T> condition)
        {
            if (list == null || list.Count == 0)
            {
                return -1;
            }
            if (condition == null)
            {
                return 0;
            }

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (condition(list[i]))
                    return i;
            }

            return -1;
        }

        // 返回struct包装, 0GC
        public static IReadOnlyList<T> MAsReadOnly<T>(this IReadOnlyList<T> list)
        {
            return new ReadOnlyListCast<T>(list);
        }
    }   
}