using UnityEngine;

namespace Mmang.Game
{
    public interface IGameplayAbility
    {
        public IReadOnlyGameplayTagContainer AbilityTags { get; }
        
        // 激活后拥有
        public IReadOnlyGameplayTagContainer ActivationOwnedTags { get; }
        // 激活时需要拥有的Tag
        public IReadOnlyGameplayTagContainer ActivationRequiredTags { get; }
        // 激活时不应拥有的Tag
        public IReadOnlyGameplayTagContainer ActivationBlockTags { get; }

        // 激活后取消带有目标Tag的Ability
        public IReadOnlyGameplayTagContainer CancelTags { get; }
        // 激活后阻挡带有目标Tag的Ability
        public IReadOnlyGameplayTagContainer BlockTags { get; }

        //
        public GameplayAbilitySpec CreateSpec(IGameplayAbilityOwner owner);
        public bool CanActivateAbility(GameplayAbilitySpec spec);
    }
}