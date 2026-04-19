using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Game
{
    [MGlobalConfig(configName = "Global Properties")]
    public class GlobalPropertiesConfig : ScriptableObject
    {
        [SerializeReference] private List<PropertyBase> m_PropertyElements = new();
        public static readonly string FieldName_PropertyElements = nameof(m_PropertyElements);

        // Runtime
        [System.NonSerialized] private Dictionary<string, PropertyBase> m_PropertyMap;
        [System.NonSerialized] private bool m_Inited = false;

        private void Init()
        {
            if (m_Inited)
            {
                return;
            }

            m_Inited = true;
            m_PropertyMap = new();
            foreach (var property in m_PropertyElements)
            {
                m_PropertyMap.Add(property.Name, property);
            }
        }

        public bool ContainsProperty(string name)
        {
            Init();
            return m_PropertyMap.ContainsKey(name);
        }

        public void AddProperty(PropertyBase property)
        {
            if (!GlobalPropertiesExtensions.IsValid(property) || ContainsProperty(property.Name))
            {
                return;
            }

            m_PropertyElements.Add(property);
            m_PropertyMap.Add(property.Name, property);
        }

        public PropertyBase GetProperty(string name)
        {
            Init();
            if (m_PropertyMap.TryGetValue(name, out var result))
            {
                return result;
            }
            return null;
        }

        public bool TryGetProperty(string name, out PropertyBase outProperty)
        {
            Init();
            return m_PropertyMap.TryGetValue(name, out outProperty);
        }

        public void SetValue<T>(string name, T newValue)
        {
            if (TryGetProperty(name, out var property) && property is ISettableProperty<T> propertyT)
            {
                propertyT.SetPropertyValue(newValue);
            }
        }

        public T GetValue<T>(string name)
        {
            if (TryGetProperty(name, out var property) && property is IGettableProperty<T> propertyT)
            {
                return propertyT.GetPropertyValue();
            }
            return default;
        }

        public object GetValue(string name)
        {
            if (TryGetProperty(name, out var property) && property is IGettableProperty propertyGettable)
            {
                return propertyGettable.GetPropertyObjectValue();
            }
            return null;
        }
    }
}