using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Game.Editors
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var gameManager = (GameManager)target;
            var root = new VisualElement();

            var button = new Button() { text = "Reset Progress" };
            button.clicked += () => gameManager.ResetProgress();

            root.Add(button);
            UIElementHelper.DrawDefaultInspector(root, serializedObject);
        
            return root;
        }
    }
}