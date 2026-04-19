using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;

namespace Mmang.Generations
{
    [CustomEditor(typeof(GenerationPointComputeBase), true)]
    public class GenerationPointComputeEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            GenerationPointComputeBase computeTarget = target as GenerationPointComputeBase;
            computeTarget.Editor_CheckConfigProperties();

            VisualElement root = new();
            UIElementHelper.DrawDefaultInspector(root, serializedObject);
            
            Button bt_LoadConfigProperties = new(() => { computeTarget.Editor_LoadConfigProperties(); });
            bt_LoadConfigProperties.Add(new Label("Load Config Properties"));
            root.Add(bt_LoadConfigProperties);

            Button bt_Refresh = new(() => { computeTarget.Editor_Refresh(); });
            bt_Refresh.Add(new Label("Refresh"));
            root.Add(bt_Refresh);

            return root;
        }
    }
}