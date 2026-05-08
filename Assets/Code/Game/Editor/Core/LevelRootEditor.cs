using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Game.Editors
{
    [CustomEditor(typeof(LevelRoot))]
    public class LevelRootEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var levelRoot = (LevelRoot)target;
            var root = new VisualElement();

            var button = new Button() { text = "Regenerate GUIDs" };
            button.clicked += () => levelRoot.Editor_RegenerateGUIDs();

            root.Add(button);
            UIElementHelper.DrawDefaultInspector(root, serializedObject);
        
            return root;
        }
    }
}