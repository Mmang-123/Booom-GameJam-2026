
using System.Collections.Generic;

namespace Mmang.Game
{
    public interface IEntity : IGameplayAbilityOwner, IGameplayAttributeOwner
    {
        public uint EntityID { get; }

        public EntityConfig GetEntityConfig();
        public bool TryGetEntityConfig(out EntityConfig outEntityConfig);
        public List<EntityConfigComponent> GetEntityConfigComponents();
        public T GetEntityConfigComponent<T>() where T : EntityConfigComponent;
        public bool TryGetEntityConfigComponent<T>(out T outComponent) where T : EntityConfigComponent;
    }
}