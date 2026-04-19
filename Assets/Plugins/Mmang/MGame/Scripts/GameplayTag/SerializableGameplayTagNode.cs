using UnityEngine;
using System.Collections.Generic;

namespace Mmang.Game
{
    [System.Serializable]
    public class SerializableGameplayTagNode
    {
        [SerializeField] private string m_Guid;
        public string Guid => m_Guid;

        public string NodeName;

        [SerializeReference] private List<SerializableGameplayTagNode> m_Children = new();
        public List<SerializableGameplayTagNode> Children => m_Children;

        public SerializableGameplayTagNode(string nodeName)
        {
            m_Guid = System.Guid.NewGuid().ToString();
            NodeName = nodeName;
        }

        public SerializableGameplayTagNode(string nodeName, List<SerializableGameplayTagNode> nodes)
        {
            m_Guid = System.Guid.NewGuid().ToString();
            NodeName = nodeName;
            m_Children.AddRange(nodes);
        }
    }
}