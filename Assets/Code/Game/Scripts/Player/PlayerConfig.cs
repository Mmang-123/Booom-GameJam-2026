using UnityEngine;
using Mmang;
using System.Collections.Generic;
using Mmang.Game;

namespace Game
{
    [System.Serializable]
    public class ControlFishConfig
    {
        [SerializeField] private GameplayTag m_FishTypeTag;

        [Header("饥饿")]
        [SerializeField] private float m_ReduceSaturationRate = 1f;

        [Header("捕食")]
        [SerializeField] private GameplayTagContainer m_CanEatTags = new();
        [SerializeField] private float m_OpenMouseDistance = 4.5f;
        [SerializeField] private float m_OpenMouseAngle = 40f;
        [SerializeField] private float m_EatDistance = 1.8f;

        public GameplayTag FishTypeTag => m_FishTypeTag;
        public float ReduceSaturationRate => m_ReduceSaturationRate;
        public IReadOnlyGameplayTagContainer CanEatTags => m_CanEatTags.AsReadOnly();
        public float OpenMouseDistance => m_OpenMouseDistance;
        public float OpenMouseAngle => m_OpenMouseAngle;
        public float EatDistance => m_EatDistance;
    }

    [MGlobalConfig(configName = "Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] private List<ControlFishConfig> m_ControlFishConfigs = new();
        public IReadOnlyList<ControlFishConfig> ControlFishConfigs => m_ControlFishConfigs;
    
        // Runtime
        [System.NonSerialized] private bool m_Inited = false;
        [System.NonSerialized] private Dictionary<GameplayTag, ControlFishConfig> m_Map;

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            m_Map ??= new();
            m_Map.Clear();
        
            foreach (var config in m_ControlFishConfigs)
            {
                if (config.FishTypeTag.IsRoot())
                    continue;
                
                if (m_Map.ContainsKey(config.FishTypeTag))
                {
                    Debug.LogWarning("存在重复键值");
                    continue;
                }

                m_Map.Add(config.FishTypeTag, config);
            }
        }

        public static ControlFishConfig GetConfig(GameplayTag tag)
        {
            var instance = GlobalConfigAssets.GetConfigInstance<PlayerConfig>();
            instance.Init();

            if (instance.m_Map.TryGetValue(tag, out var result))
                return result;
            return null;
        }

        public static bool TryGetConfig(GameplayTag tag, out ControlFishConfig outConfig)
        {
            var instance = GlobalConfigAssets.GetConfigInstance<PlayerConfig>();
            instance.Init();

            return instance.m_Map.TryGetValue(tag, out outConfig);
        }
    
    }
}