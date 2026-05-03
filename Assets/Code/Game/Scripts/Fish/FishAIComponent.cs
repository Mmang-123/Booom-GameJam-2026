using System.Collections.Generic;
using System.Linq;
using Mmang.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FishAIComponent : MonoBehaviour, IFishController
    {
        [SerializeField] private Fish m_Fish;
        [SerializeField] private List<FishAIAbility> m_Abilities = new();
        [SerializeField] private GameplayTagContainer m_OwnedTags = new();

        // Runtime
        public Fish Fish => m_Fish;
        public GameplayTagCountContainer Tags { get; private set; }
        private HashSet<FishAIAbility> m_ActiveAbilities = new();

        private List<FishAIAbility> m_AbilitiesPendingToActivate = new();
        private List<(FishAIAbility ability, EEndAbilityType endType)> m_AbilitiesPendingToEnd = new();
        private Dictionary<GameplayTag, FishAIAbility> m_SingletonAbilitiesMap = new();

        private void Start()
        {
            if (m_Fish == null)
            {
                enabled = false;
                return;
            }

            ControlFish(m_Fish);
            Init(m_Fish);
        }

        public void ControlFish(Fish fish)
        {
            m_Fish = fish;
            m_Fish?.SetController(this);
        }

        public void LoseControl(IFishController otherController)
        {
            m_Fish = null;
            gameObject.SetActive(false);
        }

        public void Init(Fish fish)
        {
            Tags = new(m_OwnedTags);
            SortAbilities();
            foreach (var ability in m_Abilities)
            {
                ability.Init(this);
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
            // Update
            foreach (var ability in m_ActiveAbilities)
            {
                ability.OnUpdate(dt);
            }

            for (int i = m_Abilities.Count - 1; i >= 0; i--)
            {
                var ability = m_Abilities[i];
                if (!ability.Active && ability.CanActivateAbility())
                {
                    //Debug.Log(ability);
                    // CancelAbilitiesWithTags(ability.CancelTags, ability.Priority);
                    //m_AbilitiesPendingToActivate.Add(ability);
                    PendingActivateAbility(ability);
                }
            }

            HandlePendingToEnd();
            HandlePendingToActivate();
        }

        private void HandlePendingToActivate()
        {
            var dict = DictionaryPool<GameplayTag, List<FishAIAbility>>.Get();

            void AddSingletonAbility(FishAIAbility ability)
            {
                if (dict.TryGetValue(ability.SingletonTag, out var list))
                {
                    list.Add(ability);
                }
                else
                {
                    var newList = ListPool<FishAIAbility>.Get();
                    newList.Add(ability);
                    dict.Add(ability.SingletonTag, newList);
                }
            }

            // 找到最大优先级的
            FishAIAbility GetResultAbility(List<FishAIAbility> list)
            {
                if (list.Count == 1)
                    return list[0];
                int maxPriority = list[0].Priority;
                FishAIAbility result = list[0];
                
                for (int i = 1; i < list.Count; i++)
                {
                    var ability = list[i];
                    if (ability.Priority > maxPriority)
                    {
                        maxPriority = ability.Priority;
                        result = ability;
                    }
                }

                return result;
            }

            foreach (var ability in m_AbilitiesPendingToActivate)
            {
                if (!ability.SingletonTag.IsRoot())
                {
                    AddSingletonAbility(ability);
                }
                else
                {
                    ability.ActivateAbility();
                    m_ActiveAbilities.Add(ability);
                }
            }

            foreach (var pair in dict)
            {
                var toActivate = GetResultAbility(pair.Value);
                if (m_SingletonAbilitiesMap.TryGetValue(pair.Key, out var otherAbility)
                && otherAbility != null)
                {
                    if (toActivate.Priority < otherAbility.Priority)
                    {
                        continue;                        
                    }
                    // 取消当前的
                    otherAbility.EndAbility(EEndAbilityType.Cancelled);
                    m_ActiveAbilities.Remove(otherAbility);
                }
                toActivate.ActivateAbility();
                m_ActiveAbilities.Add(toActivate);
                SetSingletonTag(toActivate);
            }

            foreach (var list in dict.Values)
            {
                ListPool<FishAIAbility>.Release(list);
            }
            DictionaryPool<GameplayTag, List<FishAIAbility>>.Release(dict);
            m_AbilitiesPendingToActivate.Clear();
        }

        private void HandlePendingToEnd()
        {
            foreach (var pair in m_AbilitiesPendingToEnd)
            {
                pair.ability.EndAbility(pair.endType);
                m_ActiveAbilities.Remove(pair.ability);

                if (!pair.ability.SingletonTag.IsRoot())
                {
                    CancelSingletonTag(pair.ability);
                }
            }

            m_AbilitiesPendingToEnd.Clear();
        }

        public void PendingActivateAbility(FishAIAbility ability)
        {
            if (!ability.SingletonTag.IsRoot() && !CanActiveSingletonTag(ability.SingletonTag, ability.Priority))
                return;

            if (!m_AbilitiesPendingToActivate.Contains(ability))
                m_AbilitiesPendingToActivate.Add(ability);
        }

        public void PendingEndAbility(FishAIAbility ability, EEndAbilityType endAbilityType)
        {
            if (!m_AbilitiesPendingToEnd.Any(i => i.ability == ability))
                m_AbilitiesPendingToEnd.Add(new(ability, endAbilityType));
        }

        public void CancelAbilitiesWithTags(IReadOnlyGameplayTagContainer tags, int priority)
        {
            foreach (var ability in m_ActiveAbilities)
            {
                if (ability.Priority <= priority && ability.Tags.ContainsAny(tags))
                {
                    m_AbilitiesPendingToEnd.Add(new(ability, EEndAbilityType.Cancelled));
                }
            }

            List<FishAIAbility> toCancelActivate = ListPool<FishAIAbility>.Get();
            foreach (var ability in m_AbilitiesPendingToActivate)
            {
                if (ability.Priority <= priority && ability.Tags.ContainsAny(tags))
                {
                    toCancelActivate.Add(ability);
                }
            }
            if (toCancelActivate.Count > 0)
                m_AbilitiesPendingToActivate.RemoveAll(i => toCancelActivate.Contains(i));
            ListPool<FishAIAbility>.Release(toCancelActivate);
        }


        #region Singleton Tag

        public bool CanActiveSingletonTag(GameplayTag tag, int priority)
        {
            if (m_SingletonAbilitiesMap.TryGetValue(tag, out var otherAbility))
            {
                return otherAbility.Priority < priority;
            }
            return true;
        }

        public void SetSingletonTag(FishAIAbility ability)
        {
            if (m_SingletonAbilitiesMap.ContainsKey(ability.SingletonTag))
                m_SingletonAbilitiesMap[ability.SingletonTag] = ability;
            else
                m_SingletonAbilitiesMap.Add(ability.SingletonTag, ability);
            
        }

        public void CancelSingletonTag(FishAIAbility ability)
        {
            if (m_SingletonAbilitiesMap.ContainsKey(ability.SingletonTag)
                && m_SingletonAbilitiesMap[ability.SingletonTag] == ability)
                m_SingletonAbilitiesMap.Remove(ability.SingletonTag);
        }

        #endregion

        #region 
        private void SortAbilities()
        {
            m_Abilities.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }


        #endregion

    }
}