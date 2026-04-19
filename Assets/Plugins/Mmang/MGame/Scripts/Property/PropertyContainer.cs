using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Game
{
    public class PropertyContainerAttribute : UnityEngine.PropertyAttribute
    {
        public System.Type[] types;
        public PropertyContainerAttribute(params System.Type[] types)
        {
            this.types = types;
        }
    }

    [System.Serializable]
    public class PropertyContainer
    {
        [SerializeReference] private List<Property> m_Properties = new();
        public List<Property> Properties => m_Properties;

        public void AddRange<T>(List<T> properties) where T : Property
        {
            m_Properties.AddRange(properties);
        }

        public void Add(Property property)
        {
            m_Properties.Add(property);
        }

        public void Clear()
        {
            m_Properties.Clear();
        }

        public bool Contains(string name)
        {
            foreach (var property in m_Properties)
            {
                if (property.Name == name)
                    return true;
            }
            return false;
        }
    }
}