using System.Collections.Generic;
using System.Linq;
using Mmang.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;

namespace Mmang.Game
{
    public class EntityComponent : MonoBehaviour, IPlayableEntity, IGameplayAbilityUpdateHandler
    {
        [Header("Entity")]
        [SerializeField, EntityID]
        private uint m_EntityID;

        [Header("Playable")]
        [SerializeField] private PlayableDirector m_PlayableDirector;

        [Header("Ability")]
        [SerializeField] private bool m_AutoActivateAbilities;
        [SerializeField] private GameplayTagContainer m_AdditionalOwnedTags = new();
        [SerializeField] private List<InterfaceObject<IGameplayAbility>> m_AdditionalAbilities = new();

        // Runtime
        private bool m_Valid = false;
        private List<GameplayAbilitySpec> m_AbilitySpecs;
        private GameplayTagCountContainer m_Tags;
        private GameplayTagCountContainer m_BlockTags;

        #region IEntity

        public uint EntityID => m_EntityID;

        #endregion

        #region IGameplayAbilityOwner

        public bool Valid => m_Valid && this != null;
        public IReadOnlyList<GameplayAbilitySpec> AbilitySpecs => m_AbilitySpecs;
        public IGameplayTagContainer Tags => m_Tags;
        public IGameplayTagContainer BlockTags => m_BlockTags;

        public virtual void OnRegisterActiveAbility(GameplayAbilitySpec abilitySpec) { }
        public void OnUnregisterActiveAbility(GameplayAbilitySpec abilitySpec) { }

        #endregion

        #region IGameplayAttributeOwner

        public GameplayAttributeCollection GameplayAttributes { get; } = new();

        #endregion

        #region IPlayableEntity

        public PlayableDirector PlayableDirector => m_PlayableDirector;

        #endregion

        #region IGameplayAbilityUpdateHandler

        public List<IGameplayAbilityUpdate> AbilitiesUpdate { get; } = new();
        public List<IGameplayAbilityFixedUpdate> AbilitiesFixedUpdate { get; } = new();
        public List<IGameplayAbilityLateUpdate> AbilitiesLateUpdate { get; } = new();

        #endregion

        // Config Cache
        private uint m_ConfigIDCache = 0;
        private EntityConfig m_ConfigCache;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            m_Valid = true;
            m_AbilitySpecs = new();
            m_Tags = new(m_AdditionalOwnedTags);
            m_BlockTags = new();

            // 加载Config
            if (TryGetEntityConfig(out var config))
            {
                // Entity Tags
                m_Tags.AddFromContainer(config.EntityTags);

                // 组件初始化
                var initList = ListPool<IEntityInitConfig>.Get();
                foreach (var component in config.EComponents)
                {
                    if (component is IEntityInitConfig initConfig)
                    {
                        initList.Add(initConfig);
                    }
                }
                initList.Sort((a, b) => a.InitOrder.CompareTo(b.InitOrder));
                foreach (var initConfig in initList)
                {
                    initConfig.OnEntityInit(this);
                }
            }

            // 加载额外配置的Ability
            foreach (var abilityObj in m_AdditionalAbilities)
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

        #region IEntity

        public EntityConfig GetEntityConfig()
        {
            if (m_ConfigIDCache != m_EntityID)
            {
                m_ConfigIDCache = m_EntityID;
                
                var collection = GlobalConfigAssets.GetConfigInstance<EntityConfigCollection>();
                m_ConfigCache = collection.GetConfig(m_EntityID);
            }

            return m_ConfigCache;
        }

        public bool TryGetEntityConfig(out EntityConfig outConfig)
        {
            var config = GetEntityConfig();
            outConfig = config;
            return outConfig != null;
        }

        public List<EntityConfigComponent> GetEntityConfigComponents()
        {
            var config = GetEntityConfig();
            if (config != null)
            {
                return config.EComponents.ToList();
            }
            return new();
        }

        public T GetEntityConfigComponent<T>() where T : EntityConfigComponent
        {
            if (TryGetEntityConfig(out var config))
            {
                return config.GetComponent<T>();
            }
            return null;
        }

        public bool TryGetEntityConfigComponent<T>(out T outComponent) where T : EntityConfigComponent
        {
            if (TryGetEntityConfig(out var config))
            {
                return config.TryGetComponent<T>(out outComponent);
            }
            outComponent = null;
            return false;
        }

        #endregion

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