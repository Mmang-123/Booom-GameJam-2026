using UnityEngine;

namespace Mmang.Game
{
    [CreateAbilityMenu("Entity Ability", Path = "Generic/Entity Ability")]
    public class EntityAbility : GameplayAbility
    {
        public override GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner)
        {
            if (owner is not IEntity entity)
            {
                throw new System.Exception($"{name} 的Owner应为IEntity类型");
            }

            var spec = ScriptAbilitySpec<EntityAbilitySpec>.Create(owner, this);
            return spec;
        }
    }

    public abstract class EntityAbility<T> : EntityAbility where T : EntityAbilitySpec, new()
    {
        public override GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner)
        {
            if (owner is not IEntity entity)
            {
                throw new System.Exception($"{name} 的Owner应为IEntity类型");
            }

            var spec = ScriptAbilitySpec<T>.Create(owner, this);
            return spec;
        }
    }

    public class EntityAbilitySpec : GameplayAbilitySpec
    {
        public IEntity Entity => Owner as IEntity;
    }
    public class EntityAbilitySpec<T> : EntityAbilitySpec where T : EntityAbility
    {
        public T TAbility => Ability as T;
    }

    #region Playable Entity
    [CreateAbilityMenu("Entity Ability", Path = "Generic/PlayableEntity Ability")]
    public class PlayableEntityAbility : EntityAbility
    {
        public override GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner)
        {
            if (owner is not IPlayableEntity entity)
            {
                throw new System.Exception($"{name} 的Owner应为IPlayableEntity类型");
            }

            var spec = ScriptAbilitySpec<PlayableEntityAbilitySpec>.Create(owner, this);
            return spec;
        }
    }

    public abstract class PlayableEntityAbility<T> : PlayableEntityAbility where T : PlayableEntityAbilitySpec, new()
    {
        public override GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner)
        {
            if (owner is not IPlayableEntity entity)
            {
                throw new System.Exception($"{name} 的Owner应为IPlayableEntity类型");
            }

            var spec = ScriptAbilitySpec<T>.Create(owner, this);
            return spec;
        }
    }

    public class PlayableEntityAbilitySpec : GameplayAbilitySpec
    {
        public IPlayableEntity Entity => Owner as IPlayableEntity;
    }
    public class PlayableEntityAbilitySpec<T> : PlayableEntityAbilitySpec where T : PlayableEntityAbility
    {
        public T TAbility => Ability as T;
    }

    #endregion
}