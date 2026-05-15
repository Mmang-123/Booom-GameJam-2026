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

            var ebutton = new Button() { text = "Set Infection Source Count To 5" };
            ebutton.clicked += () => gameManager.InfectionSourceCount = 5;

            root.Add(button);
            root.Add(ebutton);
            UIElementHelper.DrawDefaultInspector(root, serializedObject);
        
            return root;
        }
    }
}