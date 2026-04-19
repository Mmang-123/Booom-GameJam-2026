using System;
using UnityEngine;

namespace Mmang.Util
{
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>, new()
    {
        public static bool InstanceValid => m_Instance != null;
        private static T m_Instance;
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindAnyObjectByType<T>();
                    if (m_Instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        T component = go.AddComponent<T>();
                        m_Instance = component;
                    }
                }

                if (!m_Instance.m_Inited)
                {
                    m_Instance.m_Inited = true;
                    m_Instance.OnInit();
                }

                return m_Instance;
            }
        }

        public static T InstanceCanbeNull
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindAnyObjectByType<T>();
                }

                if (m_Instance == null)
                    return m_Instance;

                if (!m_Instance.m_Inited)
                {
                    m_Instance.m_Inited = true;
                    m_Instance.OnInit();
                }

                return m_Instance;
            }
        }

        [NonSerialized] private bool m_Inited = false;

        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                if (m_Instance != null && m_Instance != this)
                {
                    DestroyImmediate(gameObject);
                    return;
                }
            }

            m_Instance = this as T;
            if (!m_Instance.m_Inited)
            {
                m_Instance.m_Inited = true;
                m_Instance.OnInit();
            }
            OnAwake();
        }

        protected virtual void OnAwake() { }
        protected virtual void OnInit() { }
    }

}