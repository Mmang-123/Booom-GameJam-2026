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

            serializedObject.Update();

            // Sorting Layer dropdown
            SerializedProperty layerIDProp = serializedObject.FindProperty("m_SortingLayerID");
            SortingLayer[] layers = SortingLayer.layers;
            string[] layerNames = new string[layers.Length];
            int[] layerIDs = new int[layers.Length];
            int currentIndex = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                layerNames[i] = layers[i].name;
                layerIDs[i] = layers[i].id;
                if (layers[i].id == layerIDProp.intValue)
                    currentIndex = i;
            }
            int selectedIndex = EditorGUILayout.Popup("Sorting Layer", currentIndex, layerNames);
            layerIDProp.intValue = layerIDs[selectedIndex];

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Update Object"))
            {
                SDFTerrainObject obj = (SDFTerrainObject)target;
                obj.UpdateObject();
            }
        }
    }
}
