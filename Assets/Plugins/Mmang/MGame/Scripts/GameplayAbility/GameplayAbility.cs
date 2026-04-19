using UnityEngine;

namespace Mmang.Game
{
    [CreateAbilityMenu("Ability", Path = "Generic/Ability", Order = -99)]
    public class GameplayAbility : ScriptableObject, IGameplayAbility
    {
        [SerializeField] private GameplayTagContainer m_AbilityTags = new();
        [SerializeField] private GameplayTagContainer m_ActivationOwnedTags = new();
        [SerializeField] private GameplayTagContainer m_ActivationRequiredTags = new();
        [SerializeField] private GameplayTagContainer m_ActivationBlockTags = new();
        [SerializeField] private GameplayTagContainer m_CancelTags = new();
        [SerializeField] private GameplayTagContainer m_BlockTags = new();

        #region IGameplayAbility

        public IReadOnlyGameplayTagContainer AbilityTags => m_AbilityTags.AsReadOnly();
        public IReadOnlyGameplayTagContainer ActivationOwnedTags => m_ActivationOwnedTags.AsReadOnly();
        public IReadOnlyGameplayTagContainer ActivationRequiredTags => m_ActivationRequiredTags.AsReadOnly();
        public IReadOnlyGameplayTagContainer ActivationBlockTags => m_ActivationBlockTags.AsReadOnly();
        public IReadOnlyGameplayTagContainer CancelTags => m_CancelTags.AsReadOnly();
        public IReadOnlyGameplayTagContainer BlockTags => m_BlockTags.AsReadOnly();

        #endregion

        public virtual GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner)
        {
            var spec = ScriptAbilitySpec<GameplayAbilitySpec>.Create(owner, this);
            return spec;
        }

        public virtual bool CanActivateAbility(GameplayAbilitySpec spec)
        {
            var tags = spec.Owner.Tags;
            if (tags.ContainsAny(m_ActivationBlockTags)
            || !tags.ContainsAll(m_ActivationRequiredTags))
            {
                return false;
            }

            var blockTags = spec.Owner.BlockTags;
            if (m_AbilityTags.ContainsAny(blockTags))
            {
                return false;
            }

            return true;
        }

    }

    public abstract class GameplayAbility<T> : GameplayAbility where T : GameplayAbilitySpec, new()
    {
        public override GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner)
        {
            var spec = ScriptAbilitySpec<T>.Create(owner, this);
            return spec;
        }
    }
}