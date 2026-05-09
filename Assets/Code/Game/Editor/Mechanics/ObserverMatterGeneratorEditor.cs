using Mmang.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Editors
{
    [CustomEditor(typeof(ObserverMatterGenerator))]
    public class ObserverMatterGeneratorEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var generator = (ObserverMatterGenerator)target;
            var root = new VisualElement();

            var generateButton = new Button() { text = "Generate Units" };
            generateButton.clicked += () => generator.Editor_Generate();

            root.Add(generateButton);
            UIElementHelper.DrawDefaultInspector(root, serializedObject);

            return root;
        }
    }
}
