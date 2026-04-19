using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mmang
{
    public sealed class GlobalConfigAssets : ConfigAssets
    {
        private static GlobalConfigAssets s_Instance;
        public static GlobalConfigAssets Instance
        {
            get
            {
                if (s_Instance == null)
                {
#if UNITY_EDITOR
                    s_Instance = AssetDatabase.LoadAssetAtPath<GlobalConfigAssets>(MEditorPathStorage.GlobalConfigAssetsPath);
                    if (s_Instance == null)
                    {
                        GlobalConfigAssetsHelper_InitAssets.MakeSureAssetsExist();
                        s_Instance = AssetDatabase.LoadAssetAtPath<GlobalConfigAssets>(MEditorPathStorage.GlobalConfigAssetsPath);
                    }
#else
                    s_Instance = Resources.Load<GlobalConfigAssets>(MEditorPathStorage.GlobalConfigResourcesPath);
#endif
                }
                return s_Instance;
            }
        }

#if UNITY_EDITOR
        [System.NonSerialized] private Dictionary<ScriptableObject, int> m_OrderMap = new();

        private int GetConfigOrder(ScriptableObject obj)
        {
            if (m_OrderMap.TryGetValue(obj, out var result))
            {
                return result;
            }
            var configAttribute = obj.GetType().GetCustomAttribute<MGlobalConfig>();
            if (configAttribute != null)
            {
                m_OrderMap.Add(obj, configAttribute.order);
                return configAttribute.order;
            }

            return 0;
        }

        public override List<ConfigData> GetOrderedConfigs()
        {
            List<ConfigData> result = new();
            result.AddRange(Configs);
            result.Sort((a, b) =>
            {
                int orderComparation = GetConfigOrder(a.SO).CompareTo(GetConfigOrder(b.SO));
                if (orderComparation != 0)
                {
                    return orderComparation;
                }
                return a.Name.CompareTo(b.Name);
            });
            return result;
        }
#endif

        private Dictionary<string, ScriptableObject> m_NameToConfigMap;
        private Dictionary<System.Type, ScriptableObject> m_TypeToConfigMap;
        [System.NonSerialized] private bool m_MapInited = false;

#if UNITY_EDITOR
        private Dictionary<System.Type, SerializedObject> m_TypeToSOMap;
#endif

        private void InitConfigMap()
        {
            if (m_MapInited)
            {
                return;
            }

            m_MapInited = true;
            m_NameToConfigMap ??= new();
            m_TypeToConfigMap ??= new();
            m_NameToConfigMap.Clear();
            m_TypeToConfigMap.Clear();
#if UNITY_EDITOR
            m_TypeToSOMap ??= new();
            m_TypeToSOMap.Clear();
#endif

            foreach (var config in Configs)
            {
                if (config.SO == null)
                {
                    continue;
                }

                if (!m_NameToConfigMap.ContainsKey(config.Name))
                {
                    m_NameToConfigMap.Add(config.Name, config.SO);
                }

                var type = config.SO.GetType();
                if (!m_TypeToConfigMap.ContainsKey(type))
                {
                    m_TypeToConfigMap.Add(type, config.SO);
                }
            }
        }

        public void LazyRefreshMap()
        {
            m_MapInited = false;
        }

        public ScriptableObject GetConfig(string name)
        {
            InitConfigMap();
            if (m_NameToConfigMap.TryGetValue(name, out var result))
            {
                return result;
            }
            return null;
        }

        public ScriptableObject GetConfig(System.Type type)
        {
            InitConfigMap();
            if (m_TypeToConfigMap.TryGetValue(type, out var result))
            {
                return result;
            }
            return null;
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            return GetConfig(typeof(T)) as T;
        }

        // Get
        public static ScriptableObject GetConfigInstance(string name)
        {
            return Instance.GetConfig(name);
        }

        public static ScriptableObject GetConfigInstance(System.Type type)
        {
            return Instance.GetConfig(type);
        }

        public static T GetConfigInstance<T>() where T : ScriptableObject
        {
            return Instance.GetConfig<T>();
        }

#if UNITY_EDITOR

        public static SerializedObject GetConfigSerializedObject(System.Type type)
        {
            if (Instance.m_TypeToSOMap.TryGetValue(type, out var result))
            {
                if (result != null)
                {
                    result.Update();
                    return result;
                }
                else
                {
                    Instance.m_TypeToSOMap.Remove(type);
                }
            }
            var instance = GetConfigInstance(type);
            SerializedObject so = new(instance);
            Instance.m_TypeToSOMap.Add(type, so);
            return so;
        }

        public static SerializedObject GetConfigSerializedObject<T>() where T : ScriptableObject
        {
            return GetConfigSerializedObject(typeof(T));
        }

#endif
    }




#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class GlobalConfigAssetsHelper_InitAssets
    {
        static GlobalConfigAssetsHelper_InitAssets()
        {
#if UNITY_EDITOR
            // 这里延时很重要, 在不正确的时机使用assetdatabase编辑文件会导致文件导入出现问题
            EditorApplication.delayCall += () => MakeSureAssetsExist();
#endif
        }

#if UNITY_EDITOR
        public static void MakeSureAssetsExist()
        {
            bool createAssets = false;
            if (!AssetDatabase.AssetPathExists(MEditorPathStorage.GlobalConfigAssetsPath))
            {
                createAssets = true;
            }
            else
            {
                var obj = AssetDatabase.LoadAssetAtPath<GlobalConfigAssets>(MEditorPathStorage.GlobalConfigAssetsPath);
                if (obj == null)
                {
                    AssetDatabase.DeleteAsset(MEditorPathStorage.GlobalConfigAssetsPath);
                    createAssets = true;
                }
            }

            if (createAssets)
            {
                CreateAssets();
            }
        }

        public static void CreateAssets()
        {
            if (!Directory.Exists(MEditorPathStorage.ResourcesFolderPath))
            {
                Directory.CreateDirectory(MEditorPathStorage.ResourcesFolderPath);
            }

            if (AssetDatabase.AssetPathExists(MEditorPathStorage.GlobalConfigAssetsPath))
            {
                return;
            }

            var instance = ScriptableObject.CreateInstance<GlobalConfigAssets>();
            AssetDatabase.CreateAsset(instance, MEditorPathStorage.GlobalConfigAssetsPath);
            EditorUtility.SetDirty(instance);
            AssetDatabase.Refresh();
        }

#endif
    
    }

}