using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mmang.Util;
using UnityEditor;
using UnityEngine;

namespace Mmang
{
    [CreateAssetMenu(fileName = "ConfigAsset", menuName = "Mmang/Config/ConfigAsset", order = 0)]
    public class ConfigAssets : ScriptableObject
    {
        [Serializable]
        public struct ConfigData
        {
            public string Name;
            public ScriptableObject SO;

            public readonly Type SOType => SO == null ? null : SO.GetType();

            public ConfigData(string name, ScriptableObject so)
            {
                Name = name;
                SO = so;
            }

            public readonly ConfigData GetRenamed(string newName)
            {
                return new(newName, SO);
            }
        }

        public struct RenameOperation
        {
            public string OldName;
            public string NewName;

            public RenameOperation(string oldName, string newName)
            {
                OldName = oldName;
                NewName = newName;
            }

            public readonly bool IsValid()
            {
                return !string.IsNullOrWhiteSpace(OldName)
                && !string.IsNullOrWhiteSpace(NewName)
                && OldName != NewName;
            }
        }

        [SerializeField] private List<ConfigData> m_Configs = new();
        public ReadOnlyCollection<ConfigData> Configs { get; private set; }

        public ConfigAssets()
        {
            Configs = new(m_Configs);
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                ClearMissingObjects();
#endif
        }

        protected virtual void OnDisable()
        {
            
        }


#if UNITY_EDITOR

        public virtual List<ConfigData> GetOrderedConfigs()
        {
            List<ConfigData> result = new();
            result.AddRange(Configs);
            result.Sort((a, b) => a.Name.CompareTo(b.Name));
            return result;
        }

        public void ClearMissingObjects()
        {
            bool refresh = false;
            for (int i = m_Configs.Count - 1; i >= 0; i--)
            {
                if (m_Configs[i].SO == null)
                {
                    try
                    {
                        AssetDatabase.RemoveObjectFromAsset(m_Configs[i].SO);
                    }
                    catch { }
                    m_Configs.RemoveAt(i);
                    refresh = true;
                }
            }

            if (refresh)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public void CreateConfig(string name, Type type)
        {
            // 不能存在同名的配置
            if (HasConfig(name))
            {
                return;
            }

            var instance = SubAssetUtils.CreateSubAsset(this, type, name);
            ScriptableObject so = SubAssetUtils.GetSubAssets<ScriptableObject>(this)
                .Where(o => o.GetType() == type)
                .First(o => o.name == name);

            m_Configs.Add(new(name, so));
        }

        public void CreateConfig<T>(string name) where T : ScriptableObject
        {
            CreateConfig(name, typeof(T));
        }

        private void InternalRemoveConfig(List<ConfigData> configs)
        {
            m_Configs.RemoveAll(o => configs.Contains(o));
            foreach (var config in configs)
            {
                try
                {
                    AssetDatabase.RemoveObjectFromAsset(config.SO);
                }
                catch { }
            }
        }

        public void RemoveConfig(string name)
        {
            var configs = m_Configs.FindAll(o => o.Name == name);
            InternalRemoveConfig(configs);
        }

        public void RemoveConfig(ScriptableObject so)
        {
            var configs = m_Configs.FindAll(o => o.SO == so);
            InternalRemoveConfig(configs);
        }
#endif

        public bool TryGetConfigData(string name, out ConfigData configData)
        {
            foreach (var _configData in m_Configs)
            {
                if (_configData.Name == name)
                {
                    configData = _configData;
                    return true;
                }
            }
            configData = default;
            return false;
        }

        public ConfigData GetConfigData(string name)
        {
            return m_Configs.Find(o => o.Name == name);
        }

        public ConfigData GetConfigData(Type type)
        {
            return m_Configs.Find(o => o.GetType() == type);
        }

        public ConfigData GetConfigData<T>() where T : ScriptableObject
            => GetConfigData(typeof(T));

        public List<ConfigData> GetConfigDatas(Type type)
        {
            return m_Configs.FindAll(o => o.SOType == type);
        }

        public virtual bool HasConfig(string name, Type type)
        {
            return m_Configs.Any(o => o.Name == name && o.SOType == type);
        }

        public virtual bool HasConfig(string name)
        {
            return m_Configs.Any(o => o.Name == name);
        }

        public virtual bool HasConfig(Type type)
        {
            return m_Configs.Any(o => o.SOType == type);
        }

        public bool HasConfig<T>() where T : ScriptableObject
        {
            return HasConfig(typeof(T));
        }

        public bool RenameConfig(RenameOperation renameOperation)
        {
            if (!renameOperation.IsValid())
            {
                return false;
            }
            
            int index = m_Configs.ConditionalIndexOf(o => o.Name == renameOperation.OldName);
            if (index != -1 && !HasConfig(renameOperation.NewName))
            {
                m_Configs[index] = m_Configs[index].GetRenamed(renameOperation.NewName);
                return true;
            }

            return false;
        }

        public bool RenameConfigs(List<RenameOperation> renameOperations)
        {
            if (renameOperations.Any(o => !o.IsValid()))
            {
                return false;
            }

            // todo: 批量重命名操作.. 考虑多个之间交换

            return false;
        }        
    }

}