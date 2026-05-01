using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Game.Editors
{
    [CustomEditor(typeof(PowerChain))]
    public class PowerChainEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var pc = (PowerChain)target;
            var root = new VisualElement();

            var generateButton = new Button() { text = "Generate Points" };
            generateButton.clicked += () => pc.Editor_GeneratePoints();

            var clearButton = new Button() { text = "Clear Points" };
            clearButton.clicked += () => pc.Editor_ClearPoints();

            root.Add(generateButton);
            root.Add(clearButton);

            UIElementHelper.DrawDefaultInspector(root, serializedObject);
            
            return root;
        }

        private Vector2 P(Vector2 position)
        {
            var pc = (PowerChain)target;
            return position + (Vector2)pc.transform.position;
        }

        private Vector2 LP(Vector2 position)
        {
            var pc = (PowerChain)target;
            return position - (Vector2)pc.transform.position;
        }

        private void OnSceneGUI()
        {
            var pc = (PowerChain)target;
            if (pc == null)
                return;

            for (int i = 0; i < pc.ControlPoints.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector2 newPos1 = Handles.PositionHandle(P(pc.ControlPoints[i].Position1), Quaternion.identity);
                Vector2 newPos2 = Handles.PositionHandle(P(pc.ControlPoints[i].Position2), Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(pc, "Move Node");
                    pc.ControlPoints[i] = new(LP(newPos1), LP(newPos2));
                    EditorUtility.SetDirty(pc);
                }

                Debug.DrawLine(P(pc.ControlPoints[i].Position1), P(pc.ControlPoints[i].Position2), Color.green, 0f);
            }

            for (int i = 0; i < pc.ControlPoints.Count - 1; i++)
            {
                Debug.DrawLine(P(pc.ControlPoints[i].Position1), P(pc.ControlPoints[i + 1].Position1), Color.red, 0f);
            }
        }
    }
}