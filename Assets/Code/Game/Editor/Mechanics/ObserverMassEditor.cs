using Mmang.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Editors
{
    [CustomEditor(typeof(ObserverMass))]
    public class ObserverMassEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var om = (ObserverMass)target;
            var root = new VisualElement();

            var generateButton = new Button() { text = "Generate Points" };
            generateButton.clicked += () => om.Editor_GeneratePoints();

            root.Add(generateButton);
            UIElementHelper.DrawDefaultInspector(root, serializedObject);
            
            return root;
        }

        private Vector2 P(Vector2 position)
        {
            var om = (ObserverMass)target;
            return position + (Vector2)om.transform.position;
        }

        private Vector2 LP(Vector2 position)
        {
            var om = (ObserverMass)target;
            return position - (Vector2)om.transform.position;
        }

        private void OnSceneGUI()
        {
            var om = (ObserverMass)target;
            if (om == null)
                return;
            
            for (int i = 0; i < om.ControlPoints.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector2 newPos1 = Handles.PositionHandle(P(om.ControlPoints[i]), Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(om, "Move Node");
                    om.ControlPoints[i] = LP(newPos1);
                    EditorUtility.SetDirty(om);
                }
            }

            for (int i = 0; i < om.ControlPoints.Count - 1; i++)
            {
                Debug.DrawLine(P(om.ControlPoints[i]), P(om.ControlPoints[i + 1]), Color.green, 0f);
            }
        }
    }
}