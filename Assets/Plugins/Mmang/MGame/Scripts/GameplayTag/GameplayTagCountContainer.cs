using Mmang.Generic;

namespace Mmang.Game
{
    public class GameplayTagCountContainer : CountingReferenceCollection<GameplayTag>, IGameplayTagContainer
    {
        public int Count { get; private set; } = 0;

        public GameplayTagCountContainer() { }
        public GameplayTagCountContainer(IGameplayTagContainer container)
        {
            AddFromContainer(container);
        }

        protected override void OnElementEnable(GameplayTag element)
        {
            base.OnElementEnable(element);
            Count++;
        }

        protected override void OnElementDisable(GameplayTag element)
        {
            base.OnElementDisable(element);
            Count--;
        }

        public bool DirectlyContains(GameplayTag tag) => ContainsReference(tag);
        public void Add(GameplayTag tag) => AddReference(tag, 1);
        public void Remove(GameplayTag tag) => RemoveReference(tag, 1);
        public void Clear() => ClearReference();

        public void AddFromContainer(IGameplayTagContainer container)
        {
            foreach (var element in container)
            {
                AddReference(element, 1);
            }
        }

        public void RemoveFromContainer(IGameplayTagContainer container)
        {
            foreach (var element in container)
            {
                RemoveReference(element, 1);
            }
        }
    }

}