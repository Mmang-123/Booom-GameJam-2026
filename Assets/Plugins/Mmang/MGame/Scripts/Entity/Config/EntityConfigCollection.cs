using System.Collections.Generic;
using System.Linq;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{
    [MGlobalConfig(configName = "Entity Config")]
    public class EntityConfigCollection : ScriptableObject
    {
        [SerializeField] private List<EntityConfig> m_Data = new();
        public IReadOnlyList<EntityConfig> Data => m_Data.MAsReadOnly();

        // Runtime
        [System.NonSerialized] private Dictionary<uint, EntityConfig> m_ConfigMap;
        [System.NonSerialized] private bool m_Inited = false;
        [System.NonSerialized] private bool m_Error = false;

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;
            m_Error = false;

#if UNITY_EDITOR
            if (!Check())
            {
                m_Error = true;
                return;
            }
#endif

            m_ConfigMap ??= new();
            m_ConfigMap.Clear();

            foreach (var config in m_Data)
            {
                m_ConfigMap.Add(config.ID, config);
            }
        }

        public void Refresh()
        {
            m_Inited = false;
        }

        public bool Check()
        {
            Dictionary<uint, EntityConfig> idSet = new();
            foreach (var config in m_Data)
            {
                if (idSet.ContainsKey(config.ID))
                {
                    Debug.LogError($"存在重复ID的Entity Config {idSet[config.ID]?.name} 与 {config?.name}");
                    return false;
                }
                idSet.Add(config.ID, config);
            }
            return true;
            // 去重
            // m_Data = m_Data.DistinctBy(a => a.ID).ToList();
        }

        public bool Contains(EntityConfig config)
        {
            Init();
            if (m_ConfigMap.TryGetValue(config.ID, out var configInMap))
            {
                return configInMap == config;
            }
            return false;
        }

        public bool ContainsID(uint id)
        {
            Init();
            return m_ConfigMap.ContainsKey(id);
        }

        public EntityConfig GetConfig(uint id)
        {
            Init();
            return m_ConfigMap[id];
        }

        public bool TryGetConfig(uint id, out EntityConfig outConfig)
        {
            Init();
            return m_ConfigMap.TryGetValue(id, out outConfig);
        }

        public bool IsError()
        {
            Init();
            return m_Error;
        }

#if UNITY_EDITOR

        public void Editor_AddConfig(EntityConfig config)
        {
            m_Data.Add(config);
            Refresh();
        }

        public void Editor_RemoveConfig(EntityConfig config)
        {
            m_Data.Remove(config);
            Refresh();
        }
        
#endif
    }
}