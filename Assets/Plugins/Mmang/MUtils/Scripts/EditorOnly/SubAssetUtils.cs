using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mmang.Util
{
    #if UNITY_EDITOR
    public static class SubAssetUtils
    {
        public static Object[] GetSubAssets(Object main, bool includeSelf = false)
        {
            var path = AssetDatabase.GetAssetPath(main);
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .Where(o => includeSelf || o != main)
                .ToArray();
        }

        public static T[] GetSubAssets<T>(Object main, bool includeSelf = false) where T : Object
        {
            var path = AssetDatabase.GetAssetPath(main);
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .Where(o => o != null && (includeSelf || o != main))
                .OfType<T>()
                .ToArray();
        }

        public static bool AddSubAsset(Object main, Object subAsset, bool hide = false)
        {
            var path = AssetDatabase.GetAssetPath(main);
            if (path == null)
            {
                Debug.LogError("添加子资产时: 无法找到路径");
                return false;
            }

            AssetDatabase.AddObjectToAsset(subAsset, main);
            subAsset.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;

            EditorUtility.SetDirty(main);
            EditorUtility.SetDirty(subAsset);
            //AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static ScriptableObject CreateSubAsset(Object main, System.Type type, string subAssetName = null)
        {
            var subAsset = ScriptableObject.CreateInstance(type);
            subAsset.name = string.IsNullOrEmpty(subAssetName) ? type.Name : subAssetName;

            if (!AddSubAsset(main, subAsset))
            {
                Object.DestroyImmediate(subAsset);
                return null;
            }

            return subAsset;
        }

        public static T CreateSubAsset<T>(Object main, string subAssetName = null) where T : ScriptableObject
        {
            return CreateSubAsset(main, typeof(T), subAssetName) as T;
        }

        public static void DeleteSubAsset(Object subAsset)
        {
            Object.DestroyImmediate(subAsset, true);
            AssetDatabase.SaveAssets();
        }


    }
    #endif
}