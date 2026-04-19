using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
namespace Mmang.Tool
{
    public static class MissingReferenceClearer
    {
        [MenuItem("Tools/Helper/Clear ScriptableObject References with Missing Types")]
        public static void ClearSelection()
        {
            var obj = Selection.activeObject;
            if (obj is not ScriptableObject so
            || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(so)))
                return;

            if (SerializationUtility.ClearAllManagedReferencesWithMissingTypes(obj))
            {
                EditorUtility.SetDirty(obj);
            }
        }
    }

}
#endif