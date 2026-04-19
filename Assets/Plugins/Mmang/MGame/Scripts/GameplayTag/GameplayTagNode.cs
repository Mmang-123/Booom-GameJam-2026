using System.Collections.Generic;
using System.Linq;

namespace Mmang.Game
{
    // GameplayTagNode提供给外部访问, 不直接储存数据, 在tree中通过字典映射到具体数据上 
    public class GameplayTagNode
    {
        private string m_Guid;
        private string m_TagName;
        private string m_NodeName;
        public string Guid => m_Guid;
        public string TagName => m_TagName;
        public string NodeName => m_NodeName;

        public GameplayTagNode(string tagName, string nodeName)
        {
            m_Guid = string.Empty;
            m_TagName = tagName;
            m_NodeName = nodeName;
        }

        public GameplayTagNode(string guid, string tagName, string nodeName)
        {
            m_Guid = guid;
            m_TagName = tagName;
            m_NodeName = nodeName;
        }

        public override string ToString()
        {
            return $"Tag({m_TagName})";
        }
    }

    //
    public class GameplayTagNodeData
    {
        private Dictionary<string, GameplayTagNode> m_DirectChildrenMap = new();
        private HashSet<GameplayTagNode> m_ParentSet = new();
        private GameplayTagNode m_DirectParent;

        public GameplayTagNode GetOrCreateNode(string guid, string tagName, string nodeName, out bool createdNew)
        {
            if (m_DirectChildrenMap.TryGetValue(nodeName, out var result))
            {
                createdNew = false;
                return result;
            }

            var newNode = new GameplayTagNode(guid, tagName, nodeName);
            m_DirectChildrenMap.Add(nodeName, newNode);
            
            createdNew = true;
            return newNode;
        }

        public List<GameplayTagNode> GetDirectChildren()
        {
            return m_DirectChildrenMap.Values.ToList();
        }

        public List<GameplayTagNode> GetParents()
        {
            return m_ParentSet.ToList();
        }

        public GameplayTagNode GetDirectParent()
        {
            return m_DirectParent;
        }

        public void CacheDirectParent(GameplayTagNode node)
        {
            m_DirectParent = node;
        }

        public void CacheParent(GameplayTagNode node)
        {
            m_ParentSet.Add(node);
        }

        public void CacheParents(List<GameplayTagNode> nodes)
        {
            foreach (var node in nodes)
            {
                m_ParentSet.Add(node);
            }
        }

        public bool IsChildOf(GameplayTagNode node)
        {
            return m_ParentSet.Contains(node);
        }

        public bool IsLeaf()
        {
            return m_DirectChildrenMap.Count == 0;
        }

        public void RemoveChild(string childNodeName)
        {
            m_DirectChildrenMap.Remove(childNodeName);
        }

    }
}