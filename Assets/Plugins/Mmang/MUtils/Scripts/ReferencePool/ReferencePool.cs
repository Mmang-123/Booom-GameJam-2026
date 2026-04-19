using System;
using System.Collections.Generic;

namespace Mmang.Util
{
    public static class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> m_References;

            private readonly Type m_ReferenceType;

            private int m_UsingReferenceCount;

            private int m_AcquireReferenceCount;

            private int m_ReleaseReferenceCount;

            private int m_AddReferenceCount;

            private int m_RemoveReferenceCount;

            public int UnusedReferenceCount => m_References.Count;

            public int UsingReferenceCount => m_UsingReferenceCount;

            public int AcquireReferenceCount => m_AcquireReferenceCount;

            public int ReleaseReferenceCount => m_ReleaseReferenceCount;

            public int AddReferenceCount => m_AddReferenceCount;

            public int RemoveReferenceCount => m_RemoveReferenceCount;

            public ReferenceCollection(Type referenceType)
            {
                m_References = new Queue<IReference>();
                m_ReferenceType = referenceType;
                m_UsingReferenceCount = 0;
                m_AcquireReferenceCount = 0;
                m_ReleaseReferenceCount = 0;
                m_AddReferenceCount = 0;
                m_RemoveReferenceCount = 0;
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if ((object)typeof(T) != m_ReferenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return (T)m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return (IReference)Activator.CreateInstance(m_ReferenceType);
            }

            public void Release(IReference reference)
            {
                reference.Clear();
                lock (m_References)
                {
                    if (m_EnableStrictCheck && m_References.Contains(reference))
                    {
                        throw new Exception("The reference has been released.");
                    }

                    m_References.Enqueue(reference);
                }

                m_ReleaseReferenceCount++;
                m_UsingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if ((object)typeof(T) != m_ReferenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue((IReference)Activator.CreateInstance(m_ReferenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (m_References)
                {
                    if (count > m_References.Count)
                    {
                        count = m_References.Count;
                    }

                    m_RemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (m_References)
                {
                    m_RemoveReferenceCount += m_References.Count;
                    m_References.Clear();
                }
            }
        }

        private static bool m_EnableStrictCheck;
        private static readonly Dictionary<Type, ReferenceCollection> s_ReferenceCollections = new();

        public static bool EnableStrictCheck { get => m_EnableStrictCheck; set => m_EnableStrictCheck = value; }
        public static int Count => s_ReferenceCollections.Count;

        /// <summary>
        /// 清空所有引用池
        /// </summary>
        public static void ClearAll()
        {
            lock (s_ReferenceCollections)
            {
                foreach (KeyValuePair<Type, ReferenceCollection> s_ReferenceCollection in s_ReferenceCollections)
                {
                    s_ReferenceCollection.Value.RemoveAll();
                }

                s_ReferenceCollections.Clear();
            }
        }

        /// <summary>
        /// 从引用池获取引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <returns>引用</returns>
        public static T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        /// <summary>
        /// 从引用池获取引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        /// <returns>引用</returns>
        public static IReference Acquire(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            return GetReferenceCollection(referenceType).Acquire();
        }

        /// <summary>
        /// 将引用归还引用池
        /// </summary>
        /// <param name="reference">引用</param>
        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                throw new Exception("Reference is invalid.");
            }

            Type type = reference.GetType();
            InternalCheckReferenceType(type);
            GetReferenceCollection(type).Release(reference);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="count">追加数量</param>
        public static void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        /// <param name="count">追加数量</param>
        public static void Add(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="count">移除数量</param>
        public static void Remove<T>(int count) where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        /// <param name="count">移除数量</param>
        public static void Remove(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        public static void RemoveAll(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).RemoveAll();
        }

        private static void InternalCheckReferenceType(Type referenceType)
        {
            if (m_EnableStrictCheck)
            {
                if ((object)referenceType == null)
                {
                    throw new Exception("Reference type is invalid.");
                }

                if (!referenceType.IsClass || referenceType.IsAbstract)
                {
                    throw new Exception("Reference type is not a non-abstract class type.");
                }

                if (!typeof(IReference).IsAssignableFrom(referenceType))
                {
                    throw new Exception($"Reference type '{referenceType.FullName}' is invalid.");
                }
            }
        }
        private static ReferenceCollection GetReferenceCollection(Type referenceType)
        {
            if ((object)referenceType == null)
            {
                throw new Exception("ReferenceType is invalid.");
            }

            lock (s_ReferenceCollections)
            {
                if (!s_ReferenceCollections.TryGetValue(referenceType, out var value))
                {
                    value = new ReferenceCollection(referenceType);
                    s_ReferenceCollections.Add(referenceType, value);
                    return value;
                }

                return value;
            }
        }
        
        
        /// <summary>
        /// 获取所有引用池的信息
        /// </summary>
        /// <returns>所有引用池的信息</returns>
        public static ReferencePoolInfo[] GetAllReferencePoolInfos()
        {
            int num = 0;
            lock (s_ReferenceCollections)
            {
                var array = new ReferencePoolInfo[s_ReferenceCollections.Count];
                foreach (KeyValuePair<Type, ReferenceCollection> s_ReferenceCollection in s_ReferenceCollections)
                {
                    array[num++] = new ReferencePoolInfo(s_ReferenceCollection.Key, s_ReferenceCollection.Value.UnusedReferenceCount, s_ReferenceCollection.Value.UsingReferenceCount, s_ReferenceCollection.Value.AcquireReferenceCount, s_ReferenceCollection.Value.ReleaseReferenceCount, s_ReferenceCollection.Value.AddReferenceCount, s_ReferenceCollection.Value.RemoveReferenceCount);
                }

                return array;
            }
        }
    }
}
