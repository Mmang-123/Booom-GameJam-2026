using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mmang.Util;
using UnityEditor;
using UnityEngine;

namespace Mmang.Editors
{
    [InitializeOnLoad]
    [InitializeAfterTypeCollection]
    public static class GlobalConfigAssetsHelper_InitConfigs
    {
        [MenuItem("Tools/Helper/Init Global Configs")]
        public static void Init()
        {
            //EditorApplication.delayCall += () => InitConfigs();
        }

        public static void InitConfigs()
        {
            var types = TypeCollectionManager.GetTypeList<MGlobalConfig>();
            var instance = GlobalConfigAssets.Instance;

            if (instance == null)
            {
                return;
            }

            List<ConfigAssets.RenameOperation> renameOperations = new();
            void Rename(string oldName, string newName)
            {
                renameOperations.Add(new(oldName, newName));
            }

            foreach (var type in types)
            {
                if (type.GetCustomAttribute<MGlobalConfig>() is { } attribute)
                {
                    void SetupSO(ScriptableObject targetSO)
                    {
                        if (targetSO == null)
                            return;
                        //instance.SetConfigOrder(targetSO, attribute.order);
                    }

                    var configDatas = instance.GetConfigDatas(type);
                    if (configDatas.Count == 0)
                    {
                        instance.CreateConfig(attribute.configName, type);
                        var targetConfigData = instance.GetConfigData(type);
                        SetupSO(targetConfigData.SO);
                        continue;
                    }
                    
                    if (configDatas[0].Name != attribute.configName)
                    {
                        Rename(configDatas[0].Name, attribute.configName);
                    }
                    
                    // 存在多个同类型, 保留第一个
                    if (configDatas.Count > 1)
                    {
                        //..
                    }

                    SetupSO(configDatas[0].SO);
                }
            }

            // 清理没被引用的sub assets 
            //..

            //
            instance.LazyRefreshMap();
        }
    
        [MenuItem("Tools/Helper/Clear Global Configs")]
        public static void ClearMissingConfigs()
        {
            ClearMissingConfigs(null);
        }

        public static void ClearMissingConfigs(List<System.Type> types)
        {
            types ??= TypeCollectionManager.GetTypeList<MGlobalConfig>();
            
            var instance = GlobalConfigAssets.Instance;

            if (instance == null)
            {
                return;
            }

            instance.ClearMissingObjects();

            var assets = SubAssetUtils.GetSubAssets<ScriptableObject>(instance);
            List<ScriptableObject> objectsToDelete = new();
            for (int i = assets.Length - 1; i >= 0; i--)
            {
                var asset = assets[i];
                if (asset == null)
                {
                    continue;
                }

                var matchedTypes = types.Where(o => o == asset.GetType()).ToArray();
                if (matchedTypes.Length == 0)
                {
                    objectsToDelete.Add(asset);
                    continue;
                }

                var type = matchedTypes[0];

                if (type.GetCustomAttribute<MGlobalConfig>() is not { } attribute
                || attribute.configName != asset.name)
                {
                    objectsToDelete.Add(asset);
                    continue;
                }
            }

            foreach (var obj in objectsToDelete)
            {
                instance.RemoveConfig(obj);
            }

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
        }

    }
}