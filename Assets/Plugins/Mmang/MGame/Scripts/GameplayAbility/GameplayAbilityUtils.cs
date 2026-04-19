using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{
    public static class GameplayAbilityUtils
    {
        
    }

    public static class GameplayAbilityOwnerExtensions
    {
        public static void RegisterActiveAbility(this IGameplayAbilityOwner owner, GameplayAbilitySpec abilitySpec)
        {
            owner.Tags.AddFromContainer(abilitySpec.Ability.ActivationOwnedTags);
            owner.BlockTags.AddFromContainer(abilitySpec.Ability.BlockTags);
            owner.OnRegisterActiveAbility(abilitySpec);

            if (owner is IGameplayAbilityUpdateHandler updateHandler)
            {
                if (abilitySpec is IGameplayAbilityUpdate abilityUpdate)
                {
                    updateHandler.AbilitiesUpdate.Add(abilityUpdate);
                }
                
                if (abilitySpec is IGameplayAbilityFixedUpdate abilityFixedUpdate)
                {
                    updateHandler.AbilitiesFixedUpdate.Add(abilityFixedUpdate);
                }

                if (abilitySpec is IGameplayAbilityLateUpdate abilityLateUpdate)
                {
                    updateHandler.AbilitiesLateUpdate.Add(abilityLateUpdate);
                }
            }
        }

        public static void UnregisterActiveAbility(this IGameplayAbilityOwner owner, GameplayAbilitySpec abilitySpec)
        {
            owner.Tags.RemoveFromContainer(abilitySpec.Ability.ActivationOwnedTags);
            owner.BlockTags.RemoveFromContainer(abilitySpec.Ability.BlockTags);
            owner.OnUnregisterActiveAbility(abilitySpec);

            if (owner is IGameplayAbilityUpdateHandler updateHandler)
            {
                if (abilitySpec is IGameplayAbilityUpdate abilityUpdate)
                {
                    updateHandler.AbilitiesUpdate.Remove(abilityUpdate);
                }
                
                if (abilitySpec is IGameplayAbilityFixedUpdate abilityFixedUpdate)
                {
                    updateHandler.AbilitiesFixedUpdate.Remove(abilityFixedUpdate);
                }

                if (abilitySpec is IGameplayAbilityLateUpdate abilityLateUpdate)
                {
                    updateHandler.AbilitiesLateUpdate.Remove(abilityLateUpdate);
                }
            }
        }

        public static void HandleAbilityUpdate(this IGameplayAbilityUpdateHandler handler)
        {
            float dt = Time.deltaTime;
            foreach (var spec in handler.AbilitiesUpdate)
            {
                spec.OnUpdate(dt);
            }
        }

        public static void HandleAbilityFixedUpdate(this IGameplayAbilityUpdateHandler handler)
        {
            float dt = Time.deltaTime;
            foreach (var spec in handler.AbilitiesFixedUpdate)
            {
                spec.OnFixedUpdate(dt);
            }
        }

        public static void HandleAbilityLateUpdate(this IGameplayAbilityUpdateHandler handler)
        {
            foreach (var spec in handler.AbilitiesLateUpdate)
            {
                spec.OnLateUpdate();
            }
        }
    }

    public struct ScriptAbilitySpec<T> where T : GameplayAbilitySpec, new()
    {
        public static T Create(IGameplayAbilityOwner owner, IGameplayAbility ability)
        {
            var instance = ReferencePool.Acquire<T>();
            instance.Init(owner, ability);
            return instance;
        }
    }
}