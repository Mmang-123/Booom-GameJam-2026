
namespace Mmang.Game
{
    public static class GameplayTagUtils
    {
        /// A Contains B 意味着A是B或者A是B的子节点 (1.2.3 Contains 1.2)


        #region Node
        public static bool IsValid(this GameplayTagNode tagNode)
        {
            return tagNode != null;
        }

        public static bool IsRoot(this GameplayTagNode tagNode)
        {
            return GameplayTagsSettings.Tree.RootNode == tagNode;
        }

        public static bool IsLeaf(this GameplayTagNode tagNode)
        {
            return GameplayTagsSettings.Tree.IsLeaf(tagNode);
        }
        
        public static bool IsChildOf(this GameplayTagNode tagNode, GameplayTagNode otherNode)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.AIsChildOfB(tagNode, otherNode);
        }

        public static bool IsParentOf(this GameplayTagNode tagNode, GameplayTagNode otherNode)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.AIsParentOfB(tagNode, otherNode);
        }

        public static bool Contains(this GameplayTagNode tagNode, GameplayTagNode otherNode)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.AContainsB(tagNode, otherNode);
        }

        #endregion


        #region Tag
        public static string GetTagName(this GameplayTag tag)
        {
            return GameplayTagsSettings.Tree.GetTagName(tag);
        }

        public static bool IsValid(this GameplayTag tag)
        {
            return GameplayTagsSettings.Tree.ContainsTag(tag);
        }

        public static bool IsRoot(this GameplayTag tag)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.RootNode == tree.GetTagNode(tag);
        }

        public static bool IsLeaf(this GameplayTag tag)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.IsLeaf(tree.GetTagNode(tag));
        }

        public static GameplayTagNode GetTagNode(this GameplayTag tag)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.GetTagNode(tag);
        }

        public static bool TryGetTagNode(this GameplayTag tag, out GameplayTagNode outNode)
        {
            var tree = GameplayTagsSettings.Tree;
            return tree.TryGetTagNode(tag, out outNode);
        }

        public static bool IsChildOf(this GameplayTag tag, GameplayTag otherTag)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tag) || !tree.ContainsTag(otherTag))
            {
                return false;
            }

            var tagNode = tree.GetTagNode(tag);
            var otherNode = tree.GetTagNode(otherTag);
            return tree.AIsChildOfB(tagNode, otherNode);
        }

        public static bool IsChildOf(this GameplayTag tag, GameplayTagNode otherNode)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tag) || !tree.ContainsTagNode(otherNode))
            {
                return false;
            }

            var tagNode = tree.GetTagNode(tag);
            return tree.AIsChildOfB(tagNode, otherNode);
        }

        public static bool IsParentOf(this GameplayTag tag, GameplayTag otherTag)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tag) || !tree.ContainsTag(otherTag))
            {
                return false;
            }

            var tagNode = tree.GetTagNode(tag);
            var otherNode = tree.GetTagNode(otherTag);
            return tree.AIsParentOfB(tagNode, otherNode);
        }

        public static bool IsParentOf(this GameplayTag tag, GameplayTagNode otherNode)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tag) || !tree.ContainsTagNode(otherNode))
            {
                return false;
            }

            var tagNode = tree.GetTagNode(tag);
            return tree.AIsParentOfB(tagNode, otherNode);
        }

        public static bool Contains(this GameplayTag tag, GameplayTag otherTag)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tag) || !tree.ContainsTag(otherTag))
            {
                return false;
            }

            var tagNode = tree.GetTagNode(tag);
            var otherNode = tree.GetTagNode(otherTag);
            return tree.AContainsB(tagNode, otherNode);
        }

        public static bool Contains(this GameplayTag tag, GameplayTagNode otherNode)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tag) || !tree.ContainsTagNode(otherNode))
            {
                return false;
            }

            var tagNode = tree.GetTagNode(tag);
            return tree.AContainsB(tagNode, otherNode);
        }

        #endregion

        #region Container

        public static bool Contains(this IGameplayTagContainer container, GameplayTag otherTag)
            => Contains(container, otherTag, GameplayTagsSettings.Tree);
        public static bool Contains(this IGameplayTagContainer container, GameplayTag otherTag, IGameplayTagTree tree)
        {
            if (!tree.TryGetTagNode(otherTag, out var otherNode))
            {
                return false;
            }

            /*
            for (int i = container.Count - 1; i >= 0; i--)
            {
                // todo: 可以尝试在container内进行某种缓存以提升搜索效率?
                if (tree.TryGetTagNode(container[i], out var tagNode))
                {
                    if (tree.AContainsB(tagNode, otherNode))
                    {
                        return true;
                    }
                }
            }
            */

            foreach (var element in container)
            {
                if (tree.TryGetTagNode(element, out var tagNode))
                {
                    if (tree.AContainsB(tagNode, otherNode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ContainsAll(this IGameplayTagContainer container, IGameplayTagContainer otherContainer)
        {
            var tree = GameplayTagsSettings.Tree;

            foreach (var otherTag in otherContainer)
            {
                if (!container.Contains(otherTag, tree))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ContainsAny(this IGameplayTagContainer container, IGameplayTagContainer otherContainer)
        {
            var tree = GameplayTagsSettings.Tree;

            if (container.Count == 0)
            {
                return false;
            }

            foreach (var otherTag in otherContainer)
            {
                if (container.Contains(otherTag, tree))
                {
                    return true;
                }
            }

            return false;
        }

        public static ReadOnlyGameplayTagContainerWrapper AsReadOnly(this IGameplayTagContainer container)
        {
            return new ReadOnlyGameplayTagContainerWrapper(container);
        }

        #endregion
    }
}