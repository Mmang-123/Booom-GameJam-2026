using System.Collections.Generic;
using Mmang.Game;
using UnityEngine;

namespace Game
{
    public class FishAIComponent : MonoBehaviour
    {
        [SerializeField] private Fish m_Fish;
        [SerializeField] private List<FishAIAbility> m_Abilities = new();
        [SerializeField] private GameplayTagContainer m_OwnedTags = new();

        // Runtime
        public Fish Fish => m_Fish;
        public GameplayTagCountContainer Tags { get; private set; }
        private HashSet<FishAIAbility> m_ActiveAbilities = new();

        private void Start()
        {
            if (m_Fish == null)
            {
                enabled = false;
                return;
            }

            Init(m_Fish);
        }

        public void Init(Fish fish)
        {
            m_Fish = fish;
            Tags = new(m_OwnedTags);
            SortAbilities();
            foreach (var ability in m_Abilities)
            {
                ability.Init(fish);
            }
        }

        public void Dispose()
        {
            
        }

        private void FixedUpdate()
        {
            UpdateAbility(Time.fixedDeltaTime);
        }

        private void UpdateAbility(float dt)
        {
            foreach (var ability in m_Abilities)
            {
                if (!ability.Active && ability.CanActivateAbility())
                {
                    ActivateAbility(ability);
                }
            }

            foreach (var ability in m_ActiveAbilities)
            {
                ability.OnUpdate(dt);
            }
        }

        private void ActivateAbility(FishAIAbility ability)
        {
            ability.ActivateAbility();
            m_ActiveAbilities.Add(ability);
        }

        #region 
        private void SortAbilities()
        {
            m_Abilities.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }


        #endregion

    }
}