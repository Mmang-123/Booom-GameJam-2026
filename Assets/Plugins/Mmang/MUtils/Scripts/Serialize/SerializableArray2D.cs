using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Util
{
    [System.Serializable]
    public class SerializableArray2D<T> : IEnumerable<T>
    {
        [SerializeField] private T[] m_List;
        [SerializeField] private Vector2Int m_Range;

        public Vector2Int Range => m_Range;

        public SerializableArray2D(Vector2Int range) : this(range.x, range.y) { }
        public SerializableArray2D(int x, int y)
        {
            m_Range = new(Mathf.Max(0, x), Mathf.Max(0, y));
            m_List = new T[x * y];
        }

        public T this[Vector2Int coord]
        {
            get => this[coord.x, coord.y];
            set => m_List[coord.x + coord.y * m_Range.x] = value;
        }

        public T this[int x, int y]
        {
            get => m_List[x + y * m_Range.x];
            set => m_List[x + y * m_Range.x] = value;
        }

        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < m_List.Length; i++)
                yield return m_List[i];
        }

        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}