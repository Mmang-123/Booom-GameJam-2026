using UnityEngine;

namespace Mmang.Game
{
    public interface IGlobalPropertyReference
    {
        public string PropertyName { get; set; }
    }

    public interface IGenericGlobalPropertyReference : IGlobalPropertyReference
    {
        public System.Type PropertyType { get; }
    }


    [System.Serializable]
    public struct GlobalPropertyReference : IGlobalPropertyReference
    {
        [SerializeField] private string m_PropertyName;
        public string PropertyName
        {
            readonly get => m_PropertyName;
            set => m_PropertyName = value;
        }

        public GlobalPropertyReference(string propertyName)
        {
            m_PropertyName = propertyName;
        }

        public readonly object GetValue()
        {
            return GlobalProperties.GetObjectValue(m_PropertyName);
        }

        public readonly T GetValue<T>()
        {
            return GlobalProperties.GetValue<T>(m_PropertyName);
        }

        public readonly void SetValue<T>(T newValue)
        {
            GlobalProperties.SetValue(m_PropertyName, newValue);
        }
    }

    [System.Serializable]
    public struct GlobalPropertyReference<T> : IGenericGlobalPropertyReference
    {
        public readonly System.Type PropertyType => typeof(T);

        [SerializeField] private string m_PropertyName;
        public string PropertyName
        {
            readonly get => m_PropertyName;
            set => m_PropertyName = value;
        }

        public GlobalPropertyReference(string propertyName)
        {
            m_PropertyName = propertyName;
        }

        public readonly T GetValue()
        {
            return GlobalProperties.GetValue<T>(m_PropertyName);
        }

        public readonly void SetValue(T newValue)
        {
            GlobalProperties.SetValue(m_PropertyName, newValue);
        }
    }

}