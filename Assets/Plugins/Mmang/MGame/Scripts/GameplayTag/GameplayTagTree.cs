using System.Collections.Generic;

namespace Mmang.Game
{
    public class GameplayTagTree : IGameplayTagTree
    {
        private GameplayTagNode m_RootNode = new("", "");
        
        private readonly Dictionary<string, GameplayTagNode> m_NodeMap = new();
        private readonly Dictionary<GameplayTagNode, GameplayTagNodeData> m_DataMap = new();

        public GameplayTagNode RootNode => m_RootNode;

        public void Clear()
        {
            m_NodeMap.Clear();
            m_DataMap.Clear();
            m_NodeMap.Add(string.Empty, m_RootNode); // 空字符可查找到根节点
        }

        public void BuildWithNodes(List<SerializableGameplayTagNode> nodes)
        {
            SerializableGameplayTagNode rootNode = new("", nodes);

            BuildWithRootNode(rootNode);
        }

        public void BuildWithRootNode(SerializableGameplayTagNode rootNode)
        {
            Clear();
            
            UpdateWithNode(rootNode, new(), string.Empty);
        }

        public void UpdateWithNode(SerializableGameplayTagNode node, List<GameplayTagNode> parents, string currentTag)
        {
            GameplayTagNode currentNode;
            GameplayTagNode preNode = parents.Count == 0 ? null : parents[^1];

            if (parents.Count == 0)
            {
                currentTag = "";
                currentNode = m_RootNode;
            }
            else
            {
                if (parents.Count == 1)
                {
                    currentTag = node.NodeName;
                }
                else
                {
                    currentTag += $".{node.NodeName}";
                }

                currentNode = GetOrCreateNodeData(preNode).GetOrCreateNode(node.Guid, currentTag, node.NodeName, out bool created);
                m_NodeMap.Add(currentNode.Guid, currentNode);
            }

            var currentNodeData = GetOrCreateNodeData(currentNode);
            if (preNode != null)
            {
                currentNodeData.CacheDirectParent(preNode);
                currentNodeData.CacheParents(parents);   
            }

            parents.Add(currentNode);
            foreach (var child in node.Children)
            {
                UpdateWithNode(child, parents, currentTag);
            }
            parents.RemoveAt(parents.Count - 1);
        }

        private GameplayTagNodeData GetOrCreateNodeData(GameplayTagNode node)
        {
            if (m_DataMap.TryGetValue(node, out var result))
            {
                return result;
            }

            var data = new GameplayTagNodeData();
            m_DataMap.Add(node, data);

            return data;
        }

        #region Interface

        public bool ContainsTag(GameplayTag tag)
        {
            return m_NodeMap.ContainsKey(tag.Guid);
        }

        public bool ContainsTagNode(GameplayTagNode node)
        {
            return node != null && m_NodeMap.ContainsKey(node.Guid);
        }

        public GameplayTagNode GetTagNode(GameplayTag tag)
        {
            if (m_NodeMap.TryGetValue(tag.Guid, out var result))
            {
                return result;
            }
            return null;
        }

        public bool TryGetTagNode(GameplayTag tag, out GameplayTagNode outNode)
        {
            return m_NodeMap.TryGetValue(tag.Guid, out outNode);
        }

        public string GetTagName(GameplayTag tag)
        {
            if (m_NodeMap.TryGetValue(tag.Guid, out var result))
            {
                return result.TagName;
            }
            return string.Empty;
        }

        public List<GameplayTagNode> GetDirectChildrenNodes(GameplayTagNode node)
        {
            return GetOrCreateNodeData(node).GetDirectChildren();
        }

        public List<GameplayTagNode> GetParentNodes(GameplayTagNode node, bool withRootNode = false)
        {
            if (node == null || node == m_RootNode
            || !m_DataMap.TryGetValue(node, out var data))
            {
                return new();
            }

            var result = data.GetParents();

            if (!withRootNode)
            {
                result.Remove(m_RootNode);
            }

            return result;
        }

        public GameplayTagNode GetDirectParent(GameplayTagNode node)
        {
            return GetOrCreateNodeData(node).GetDirectParent();
        }

        #region 节点判断

        public bool IsLeaf(GameplayTagNode node)
        {
            return m_DataMap[node].IsLeaf();
        }

        public bool AIsChildOfB(GameplayTagNode nodeA, GameplayTagNode nodeB)
        {
            var aData = m_DataMap[nodeA];
            return aData.IsChildOf(nodeB);
        }

        public bool AIsParentOfB(GameplayTagNode nodeA, GameplayTagNode nodeB)
        {
            var bData = m_DataMap[nodeB];
            return bData.IsChildOf(nodeA);
        }

        public bool AContainsB(GameplayTagNode nodeA, GameplayTagNode nodeB)
        {
            return nodeA == nodeB || AIsChildOfB(nodeA, nodeB);
        }

        #endregion

        #endregion
    }
}