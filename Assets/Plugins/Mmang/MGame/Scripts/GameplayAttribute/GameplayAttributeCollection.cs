using System.Collections.Generic;

namespace Mmang.Game
{
    public class GameplayAttributeCollection
    {
        private Dictionary<GameplayAttributeIdentifier, GameplayAttribute> m_AttributeMap = new();

        public void AddAttriute(GameplayAttribute attribute)
            => AddAttriute(attribute, attribute.ID);
        public void AddAttriute(GameplayAttribute attribute, GameplayAttributeIdentifier id)
        {
            if (!m_AttributeMap.ContainsKey(id))
            {
                m_AttributeMap.Add(id, attribute);
            }
        }

        public GameplayAttribute GetAttribute(GameplayAttributeIdentifier id)
        {
            if (m_AttributeMap.TryGetValue(id, out var result))
            {
                return result;
            }
            return null;
        }

        public GameplayAttribute GetAttribute<T>(string additionalInfo = null) where T : GameplayAttributeModel
        {
            return GetAttribute(new(typeof(T), additionalInfo));
        }

        public float GetValue(GameplayAttribute attribute)
        {
            return attribute.Model.ComputeValue(attribute, this);
        }

        public float GetValue(GameplayAttributeIdentifier id)
        {
            var attribute = GetAttribute(id);
            return GetValue(attribute);
        }

        public float GetValue<T>(string additionalInfo = null) where T : GameplayAttributeModel
        {
            return GetValue(GetAttribute<T>(additionalInfo));
        }
    }
}