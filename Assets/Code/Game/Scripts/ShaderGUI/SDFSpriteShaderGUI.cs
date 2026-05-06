using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Sloane
{
    public class SDFSpriteShaderGUI : ShaderGUI
    {
        private const string BOIL_KEYWORD = "_BOIL_EFFECT_ENABLED";

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;

            // --- Boil Effect Toggle ---
            bool boilEnabled = material.IsKeywordEnabled(BOIL_KEYWORD);
            EditorGUI.BeginChangeCheck();
            boilEnabled = EditorGUILayout.Toggle("Boil Effect", boilEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material m in materialEditor.targets)
                {
                    if (boilEnabled)
                        m.EnableKeyword(BOIL_KEYWORD);
                    else
                        m.DisableKeyword(BOIL_KEYWORD);
                }
            }

            if (boilEnabled)
            {
                EditorGUI.indentLevel++;
                MaterialProperty boilDuration = FindProperty("_BoilDuration", properties);
                materialEditor.ShaderProperty(boilDuration, boilDuration.displayName);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // 隐藏已手动处理的属性，其余走默认绘制
            base.OnGUI(materialEditor, properties);
        }
    }
}
#endif
