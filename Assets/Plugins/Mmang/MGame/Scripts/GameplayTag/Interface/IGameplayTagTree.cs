using System.Collections.Generic;

namespace Mmang.Game
{
    public interface IGameplayTagTree
    {
        public GameplayTagNode RootNode { get; }

        public bool ContainsTag(GameplayTag tag);
        public bool ContainsTagNode(GameplayTagNode node);
        public GameplayTagNode GetTagNode(GameplayTag tag);
        public bool TryGetTagNode(GameplayTag tag, out GameplayTagNode outNode);

        public string GetTagName(GameplayTag tag);

        public List<GameplayTagNode> GetDirectChildrenNodes(GameplayTagNode node);
        public List<GameplayTagNode> GetParentNodes(GameplayTagNode node, bool withRootNode = false);
        public GameplayTagNode GetDirectParent(GameplayTagNode node);

        public bool IsLeaf(GameplayTagNode node);
        public bool AIsChildOfB(GameplayTagNode nodeA, GameplayTagNode nodeB);
        public bool AIsParentOfB(GameplayTagNode nodeA, GameplayTagNode nodeB);
        public bool AContainsB(GameplayTagNode nodeA, GameplayTagNode nodeB);
    }
}