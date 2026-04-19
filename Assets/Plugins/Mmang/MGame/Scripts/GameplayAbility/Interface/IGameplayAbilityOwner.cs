using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Game
{
    public interface IGameplayAbilityOwner
    {
        public IReadOnlyList<GameplayAbilitySpec> AbilitySpecs { get; }
        public IGameplayTagContainer Tags { get; }
        public IGameplayTagContainer BlockTags { get; }
        public bool Valid { get; }

        public void OnRegisterActiveAbility(GameplayAbilitySpec abilitySpec);
        public void OnUnregisterActiveAbility(GameplayAbilitySpec abilitySpec);

        public void AddAbilitySpec(GameplayAbilitySpec abilitySpec);
        public void RemoveAbilitySpec(GameplayAbilitySpec abilitySpec);
        //public void AddAbility<T>(T ability) where T : IGameplayAbility;
        //public void RemoveAbility<T>(T ability) where T : IGameplayAbility;
    }
}