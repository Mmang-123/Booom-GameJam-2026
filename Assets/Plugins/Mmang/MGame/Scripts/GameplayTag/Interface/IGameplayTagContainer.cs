using System.Collections;
using System.Collections.Generic;

namespace Mmang.Game
{
    public interface IGameplayTagContainer : IEnumerable<GameplayTag>
    {
        // public GameplayTag this[int index] { get; }
        public int Count { get; }

        public bool DirectlyContains(GameplayTag tag);
        public void Add(GameplayTag tag);
        public void Remove(GameplayTag tag);
        public void AddFromContainer(IGameplayTagContainer container);
        public void RemoveFromContainer(IGameplayTagContainer container);
        public void Clear();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IReadOnlyGameplayTagContainer : IGameplayTagContainer
    {
        void IGameplayTagContainer.Add(GameplayTag tag)
        {
            throw new System.NotSupportedException("Collection is read-only.");
        }
        void IGameplayTagContainer.Remove(GameplayTag tag)
        {
            throw new System.NotSupportedException("Collection is read-only.");
        }
        void IGameplayTagContainer.AddFromContainer(IGameplayTagContainer container)
        {
            throw new System.NotSupportedException("Collection is read-only.");
        }
        void IGameplayTagContainer.RemoveFromContainer(IGameplayTagContainer container)
        {
            throw new System.NotSupportedException("Collection is read-only.");
        }
        void IGameplayTagContainer.Clear()
        {
            throw new System.NotSupportedException("Collection is read-only.");
        }
    }
}