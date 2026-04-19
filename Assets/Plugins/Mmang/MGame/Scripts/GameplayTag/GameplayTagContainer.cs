using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Mmang.Game
{
    [System.Serializable]
    public class GameplayTagContainer : IGameplayTagContainer
    {
        [SerializeField] private List<GameplayTag> m_Tags;

        #region IGameplayTagContainer
        public GameplayTag this[int index] => m_Tags[index];
        public int Count => m_Tags.Count;

        public bool DirectlyContains(GameplayTag tag) => m_Tags.Contains(tag);
        public int IndexOf(GameplayTag tag) => m_Tags.IndexOf(tag);
        public void Add(GameplayTag tag) => m_Tags.Add(tag);
        public void Remove(GameplayTag tag) => m_Tags.Remove(tag);
        public void AddFromContainer(IGameplayTagContainer container) => m_Tags.AddRange(container);
        public void RemoveFromContainer(IGameplayTagContainer container) => m_Tags.RemoveAll(tag => container.DirectlyContains(tag));
        public void Clear() => m_Tags.Clear();

        public IEnumerator<GameplayTag> GetEnumerator()
        {
            for (int i = 0; i < m_Tags.Count; i++)
            {
                yield return m_Tags[i];
            }
        }

        #endregion


        /// <summary>
        /// 保留最简节点
        /// </summary>
        public void Normalize()
        {
            var tree = GameplayTagsSettings.Tree;
            List<GameplayTag> result = ListPool<GameplayTag>.Get();

            m_Tags.RemoveAll(i => !tree.ContainsTag(i));

            int tagCount = m_Tags.Count;
            for (int i = 0; i < tagCount; i++)
            {
                var tag = m_Tags[i];
                var node = tree.GetTagNode(tag);

                if (tree.IsLeaf(node))
                {
                    result.Add(tag);
                    continue;
                }
                
                // 检查是否存在子节点
                bool hasChild = false;
                for (int j = 0; j < tagCount; j++)
                {
                    if (i == j)
                        continue;

                    var otherTag = m_Tags[j];
                    var otherNode = tree.GetTagNode(otherTag);

                    if (tree.AIsChildOfB(otherNode, node))
                    {
                        hasChild = true;
                        break;
                    }
                }

                if (!hasChild)
                {
                    result.Add(tag);
                }
            }

            m_Tags.Clear();
            m_Tags.AddRange(result);
            ListPool<GameplayTag>.Release(result);
        }
    }


    #region ReadOnly Version

    public class ReadOnlyGameplayTagContainer : IReadOnlyGameplayTagContainer
    {
        private IGameplayTagContainer m_Container;

        #region IGameplayTagContainer
        public int Count => m_Container.Count;

        public bool DirectlyContains(GameplayTag tag) => m_Container.DirectlyContains(tag);

        public IEnumerator<GameplayTag> GetEnumerator() => m_Container.GetEnumerator();

        #endregion

        public ReadOnlyGameplayTagContainer(IGameplayTagContainer container)
        {
            m_Container = container;
        }
    }

    public struct ReadOnlyGameplayTagContainerWrapper : IReadOnlyGameplayTagContainer
    {
        private IGameplayTagContainer m_Container;

        #region IGameplayTagContainer
        public readonly int Count => m_Container.Count;

        public readonly bool DirectlyContains(GameplayTag tag) => m_Container.DirectlyContains(tag);

        public IEnumerator<GameplayTag> GetEnumerator() => m_Container.GetEnumerator();

        #endregion

        public ReadOnlyGameplayTagContainerWrapper(IGameplayTagContainer container)
        {
            m_Container = container;
        }
    }

    #endregion
}