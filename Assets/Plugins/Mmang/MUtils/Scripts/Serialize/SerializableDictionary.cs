using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mmang.Util
{
    public class SerializableDictionary { }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        SerializableDictionary,
        ISerializationCallbackReceiver,
        IReadOnlyDictionary<TKey, TValue>,
        IDictionary<TKey, TValue>
    {
        [SerializeField] private List<SerializableKeyValuePair> m_List = new();

        [Serializable]
        private struct SerializableKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        private Dictionary<TKey, int> KeyPositions => m_KeyPositions.Value;
        private Lazy<Dictionary<TKey, int>> m_KeyPositions;

        public SerializableDictionary()
        {
            m_KeyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
        }

        private Dictionary<TKey, int> MakeKeyPositions()
        {
            var dictionary = new Dictionary<TKey, int>(m_List.Count);
            for (var i = m_List.Count - 1; i >= 0; i--) // 倒序遍历, 重复的Key默认取到第一个
            {
                dictionary[m_List[i].Key] = i;
            }
            return dictionary;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            m_KeyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
        }

        #region IDictionary<TKey, TValue>

        public TValue this[TKey key]
        {
            get => m_List[KeyPositions[key]].Value;
            set
            {
                var pair = new SerializableKeyValuePair(key, value);
                if (KeyPositions.ContainsKey(key))
                {
                    m_List[KeyPositions[key]] = pair;
                }
                else
                {
                    KeyPositions[key] = m_List.Count;
                    m_List.Add(pair);
                }
            }
        }

        public ICollection<TKey> Keys => m_List.Select(tuple => tuple.Key).ToArray();
        public ICollection<TValue> Values => m_List.Select(tuple => tuple.Value).ToArray();

        public void Add(TKey key, TValue value)
        {
            if (KeyPositions.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            else
            {
                KeyPositions[key] = m_List.Count;
                m_List.Add(new SerializableKeyValuePair(key, value));
            }
        }

        public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (KeyPositions.TryGetValue(key, out var index))
            {
                KeyPositions.Remove(key);

                m_List.RemoveAt(index);
                for (var i = index; i < m_List.Count; i++)
                    KeyPositions[m_List[i].Key] = i;

                return true;
            }
            else
                return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (KeyPositions.TryGetValue(key, out var index))
            {
                value = m_List[index].Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        #endregion

        #region ICollection <KeyValuePair<TKey, TValue>>

        public int Count => m_List.Count;
        public bool IsReadOnly => false;

        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        public void Clear() => m_List.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var numKeys = m_List.Count;
            if (array.Length - arrayIndex < numKeys)
                throw new ArgumentException("arrayIndex");
            for (var i = 0; i < numKeys; i++, arrayIndex++)
            {
                var entry = m_List[i];
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

        #endregion

        #region IEnumerable <KeyValuePair<TKey, TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_List.Select(ToKeyValuePair).GetEnumerator();

            static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
            {
                return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IReadOnlyDictionary

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        #endregion
    }
}