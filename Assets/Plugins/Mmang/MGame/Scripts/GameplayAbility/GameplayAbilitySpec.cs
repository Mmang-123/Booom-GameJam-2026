using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{

    public struct ActivateAbilityInfo
    {
        
    }

    public struct EndAbilityInfo
    {
        public bool IsCancelled;

        public static EndAbilityInfo Cancelled => new()
        {
            IsCancelled = true
        };
    }


    public class GameplayAbilitySpec : IReference
    {
        public IGameplayAbilityOwner Owner { get; private set; }
        public IGameplayAbility Ability { get; private set; }
        public bool Active { get; private set; }
        public bool Valid => Owner.Valid && Ability != null;

        public void Clear()
        {
            Owner = null;
            Ability = null;
            Active = false;
        }

        public void Init(IGameplayAbilityOwner owner, IGameplayAbility ability)
        {
            Owner = owner;
            Ability = ability;

            OnInit();
        }

        public bool TryActivateAbility(ActivateAbilityInfo info = default)
        {
            if (!Active && Ability.CanActivateAbility(this))
            {
                ActivateAbility(info);
                return true;
            }

            return false;
        }

        private void ActivateAbility(ActivateAbilityInfo info = default)
        {
            {
                string abilityName = Ability is Object abilityObj ? abilityObj.name : "#";
                string ownerName = Owner is Object ownerObj ? ownerObj.name : "unknown";
                Debug.Log($"[{abilityName}] Activate (Owner: {ownerName})");
            }
            
            // 先取消冲突Ability
            CancelAbilities(Ability.CancelTags);

            Active = true;

            // 注册
            Owner.RegisterActiveAbility(this);

            OnActivation(info);
        }

        public void EndAbility(EndAbilityInfo info = default)
        {
            {
                string abilityName = Ability is Object abilityObj ? abilityObj.name : "#";
                string ownerName = Owner is Object ownerObj ? ownerObj.name : "unknown";
                Debug.Log($"[{abilityName}] End (Owner: {ownerName})");
            }

            Active = false;

            // 移除
            Owner.UnregisterActiveAbility(this);
        
            OnEnd(info);
        }

        private void CancelAbilities(IGameplayTagContainer cancelTags)
        {
            foreach (var specs in Owner.AbilitySpecs)
            {
                if (specs == this || !specs.Active)
                    continue;
                
                if (specs.Ability.AbilityTags.ContainsAny(cancelTags))
                {
                    // todo: 后续添加队列处理.. 防止终止逻辑里再次激活/取消ability
                    specs.EndAbility(EndAbilityInfo.Cancelled);
                }
            }
        }


        protected virtual void OnInit() { }
        protected virtual void OnActivation(ActivateAbilityInfo info) { }
        protected virtual void OnEnd(EndAbilityInfo info) { }
    }

    public class GameplayAbilitySpec<T> : GameplayAbilitySpec where T : GameplayAbility
    {
        public T TAbility => Ability as T;
    }
}