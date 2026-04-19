using System;
using UnityEngine;

namespace Mmang.Game
{
    [Serializable]
    public struct GameplayTag : IEquatable<GameplayTag>
    {
        [SerializeField]
        private string m_Guid;
        public readonly string Guid => m_Guid;

        public GameplayTag(string guid)
        {
            m_Guid = guid;
        }

        public bool Equals(GameplayTag other)
        {
            return m_Guid == other.Guid;
        }

        public override int GetHashCode()
        {
            return m_Guid != null ? m_Guid.GetHashCode() : 0;
        }

        public static GameplayTag RootTag => new(string.Empty);

        public static GameplayTag CreateByName(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return RootTag;
            }

            return CreateByName(tagName.Split('.'));
        }

        public static GameplayTag CreateByName(params string[] tagNodeNames)
        {
            if (tagNodeNames == null)
            {
                return RootTag;
            }

            var tree = GameplayTagsSettings.Tree;

            int count = tagNodeNames.Length;
            var currentNode = tree.RootNode;

            for (int i = 0; i < count; i++)
            {
                var children = tree.GetDirectChildrenNodes(currentNode);
                int childIndex = children.FindIndex(o => o.NodeName == tagNodeNames[i]);
                if (childIndex == -1)
                {
                    return RootTag;
                }

                currentNode = children[childIndex];
            }

            return currentNode;
        }

        public static implicit operator GameplayTag(GameplayTagNode tagNode) => new(tagNode.Guid);
    }
}