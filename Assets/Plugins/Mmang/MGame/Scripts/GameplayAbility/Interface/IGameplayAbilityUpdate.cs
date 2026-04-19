
using System.Collections.Generic;

namespace Mmang.Game
{
    public interface IGameplayAbilityUpdate
    {
        public void OnUpdate(float dt);
    }

    public interface IGameplayAbilityFixedUpdate
    {
        public void OnFixedUpdate(float dt);
    }

    public interface IGameplayAbilityLateUpdate
    {
        public void OnLateUpdate();
    }

    public interface IGameplayAbilityUpdateHandler
    {
        public List<IGameplayAbilityUpdate> AbilitiesUpdate { get; }
        public List<IGameplayAbilityFixedUpdate> AbilitiesFixedUpdate { get; }
        public List<IGameplayAbilityLateUpdate> AbilitiesLateUpdate { get; }
    }
}