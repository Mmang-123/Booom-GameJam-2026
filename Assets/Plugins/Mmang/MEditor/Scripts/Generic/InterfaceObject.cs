using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Generic
{
    [System.Serializable]
    public class InterfaceObject<TInterface> where TInterface : class
    {
        [SerializeField] private Object m_Object;

        public TInterface Value => m_Object as TInterface;
        public Object GetObject => m_Object;
        public void SetObject<TObject>(TObject newValue) where TObject : Object, TInterface
        {
            m_Object = newValue;
        }
    }

    // 转换为IReadOnlyList<TInterface>
    public readonly struct InterfaceListCast<TWrapper, TInterface> : IReadOnlyList<TInterface>
        where TWrapper : InterfaceObject<TInterface>
        where TInterface : class
    {
        private readonly IReadOnlyList<TWrapper> m_Source;

        public InterfaceListCast(IReadOnlyList<TWrapper> source)
        {
            m_Source = source;
        }

        public TInterface this[int index] => m_Source[index].Value;

        public int Count => m_Source.Count;

        public IEnumerator<TInterface> GetEnumerator()
        {
            foreach (var item in m_Source)
            {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class InterfaceObjectExtensions
    {
        public static IReadOnlyList<TInterface> AsReadOnlyList<TInterface>(this List<InterfaceObject<TInterface>> list) where TInterface : class
        {
            if (list == null)
            {
                return null;
            }
            return new InterfaceListCast<InterfaceObject<TInterface>, TInterface>(list);
        }
    }
}