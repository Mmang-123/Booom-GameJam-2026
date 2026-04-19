using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Generic
{
    public class CountingReference
    {
        private int m_Count;

        public event System.Action OnEnable;
        public event System.Action OnDisable;

        public void Add()
        {
            m_Count++;
            if (m_Count == 1)
            {
                OnEnable?.Invoke();
            }
        }

        public void Remove()
        {
            if (m_Count == 0)
            {
                return;
            }
            m_Count--;
            if (m_Count == 0)
            {
                OnDisable?.Invoke();
            }
        }

        public bool IsValid()
        {
            return m_Count > 0;
        }
    }

    public class CountingReferenceCollection<T> : IEnumerable<T>
    {
        private Dictionary<T, int> m_Map = new();

        public void AddReference(T obj, int changeCount = 1)
        {
            if (changeCount <= 0)
            {
                return;
            }

            if (m_Map.TryGetValue(obj, out int oldCount))
            {
                m_Map[obj] += changeCount;
                if (oldCount == 0)
                {
                    OnElementEnable(obj);
                }
            }
            else
            {
                m_Map.Add(obj, changeCount);
                OnElementEnable(obj);
            }
        }

        public void RemoveReference(T obj, int changeCount = 1)
        {
            if (changeCount <= 0)
            {
                return;
            }

            if (m_Map.TryGetValue(obj, out int oldCount))
            {
                m_Map[obj] = Mathf.Max(0, m_Map[obj] - changeCount);
                if (oldCount > 0 && m_Map[obj] == 0)
                {
                    OnElementDisable(obj);
                }
            }
        }

        public bool ContainsReference(T obj)
        {
            if (m_Map.TryGetValue(obj, out int count))
            {
                return count > 0;
            }
            return false;
        }

        public int GetReferenceCount(T obj)
        {
            if (m_Map.TryGetValue(obj, out int count))
            {
                return count;
            }
            return 0;
        }

        public void ClearReference()
        {
            m_Map.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var pair in m_Map)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }
                yield return pair.Key;
            }
        }

        protected virtual void OnElementEnable(T element)
        {
            
        }

        protected virtual void OnElementDisable(T element)
        {
            
        }
    }
}