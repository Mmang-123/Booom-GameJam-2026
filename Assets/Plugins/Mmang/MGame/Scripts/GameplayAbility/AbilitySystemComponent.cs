using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine;

namespace Mmang.Game
{
    public class AbilitySystemComponent : MonoBehaviour, IGameplayAbilityOwner, IGameplayAbilityUpdateHandler
    {
        [SerializeField] private bool m_AutoActivateAbilities;
        [SerializeField] private List<InterfaceObject<IGameplayAbility>> m_Abilities = new();
        [SerializeField] private GameplayTagContainer m_OwnedTags = new();

        // Runtime
        private bool m_Valid = false;
        private List<GameplayAbilitySpec> m_AbilitySpecs;
        private GameplayTagCountContainer m_Tags;
        private GameplayTagCountContainer m_BlockTags;

        #region IGameplayAbilityOwner

        public bool Valid => m_Valid && this != null;
        public IReadOnlyList<GameplayAbilitySpec> AbilitySpecs => m_AbilitySpecs;
        public IGameplayTagContainer Tags => m_Tags;
        public IGameplayTagContainer BlockTags => m_BlockTags;

        public virtual void OnRegisterActiveAbility(GameplayAbilitySpec abilitySpec) { }
        public void OnUnregisterActiveAbility(GameplayAbilitySpec abilitySpec) { }

        #endregion

        #region IGameplayAbilityUpdateHandler

        public List<IGameplayAbilityUpdate> AbilitiesUpdate { get; } = new();
        public List<IGameplayAbilityFixedUpdate> AbilitiesFixedUpdate { get; } = new();
        public List<IGameplayAbilityLateUpdate> AbilitiesLateUpdate { get; } = new();

        #endregion

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            m_Valid = true;
            m_AbilitySpecs = new();
            m_Tags = new(m_OwnedTags);
            m_BlockTags = new();

            foreach (var abilityObj in m_Abilities)
            {
                m_AbilitySpecs.Add(abilityObj.Value.CreateSpec(this));
            }
        }

        private void Dispose()
        {
            m_Valid = false;
            m_AbilitySpecs.Clear();
            m_Tags.Clear();
            m_BlockTags.Clear();
        }

        private void Update()
        {
            if (m_AutoActivateAbilities)
            {
                AutoActivateAbilities();
            }
            this.HandleAbilityUpdate();
        }

        private void FixedUpdate()
        {
            this.HandleAbilityFixedUpdate();
        }

        private void LateUpdate()
        {
            this.HandleAbilityLateUpdate();
        }

        private void AutoActivateAbilities()
        {
            foreach (var spec in m_AbilitySpecs)
            {
                if (!spec.Valid || spec.Active)
                {
                    continue;
                }

                spec.TryActivateAbility();
            }
        }

        #region IGameplayAbilityOwner
        
        public void AddAbilitySpec(GameplayAbilitySpec abilitySpec)
            => m_AbilitySpecs.Add(abilitySpec);

        public void RemoveAbilitySpec(GameplayAbilitySpec abilitySpec)
        {
            if (abilitySpec.Active)
            {
                abilitySpec.EndAbility(EndAbilityInfo.Cancelled);
            }
            m_AbilitySpecs.Remove(abilitySpec);
        }

        #endregion
    }
}