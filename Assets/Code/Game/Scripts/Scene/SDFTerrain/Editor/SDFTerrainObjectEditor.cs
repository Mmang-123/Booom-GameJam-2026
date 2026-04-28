using UnityEditor;
using UnityEngine;

namespace Sloane
{
    [CustomEditor(typeof(SDFTerrainObject))]
    public class SDFTerrainObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SDFTerrainObject obj = (SDFTerrainObject)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Update Object"))
            {
                obj.UpdateObject();
            }
        }
    }
}
