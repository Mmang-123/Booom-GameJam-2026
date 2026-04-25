using UnityEngine;
using UnityEditor;

namespace Sloane.Editor
{
    public class SDFGeneratorWindow : EditorWindow
    {
        private Texture2D sourceTexture;
        private float alphaThreshold = 0.5f;
        private bool invertSelection = false;
        private Texture2D resultInitialized;
        private Texture2D resultNearestPoint;
        private Texture2D resultSDF;
        private string lastSavePath = "Assets"; [MenuItem("Tools/Sloane/SDF Generator")]
        public static void ShowWindow()
        {
            GetWindow<SDFGeneratorWindow>("SDF Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Generate SDF from Texture", EditorStyles.boldLabel);

            sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
            alphaThreshold = EditorGUILayout.Slider("Alpha Threshold", alphaThreshold, 0f, 1f);
            invertSelection = EditorGUILayout.Toggle("Invert Selection (Inner SDF)", invertSelection);

            EditorGUI.BeginDisabledGroup(sourceTexture == null);
            if (GUILayout.Button("Generate SDF"))
            {
                GenerateSDF();
            }
            EditorGUI.EndDisabledGroup();

            if (resultInitialized != null)
            {
                GUILayout.Label("Initialized Seed Buffer (Debug)");
                EditorGUILayout.ObjectField("Initialized Texture", resultInitialized, typeof(Texture2D), false);

                if (GUILayout.Button("Save Initialized Texture"))
                {
                    SaveTexture(resultInitialized, "Initialized");
                }
            }

            if (resultNearestPoint != null)
            {
                GUILayout.Label("Nearest Point Result (RG: Nearest Point, B: Is Seed)");
                EditorGUILayout.ObjectField("Nearest Point Texture", resultNearestPoint, typeof(Texture2D), false);

                if (GUILayout.Button("Save Nearest Point Texture"))
                {
                    SaveTexture(resultNearestPoint, "NearestPoint");
                }
            }

            if (resultSDF != null)
            {
                GUILayout.Label("SDF Distance Field Result");
                EditorGUILayout.ObjectField("Distance Field", resultSDF, typeof(Texture2D), false);

                if (GUILayout.Button("Save Distance Field"))
                {
                    SaveTexture(resultSDF, "SDF");
                }
            }
        }

        private void GenerateSDF()
        {
            if (sourceTexture == null) return;

            // 获取初始化后的种子缓冲区
            RenderTexture initializedRT = SDFTools.GetInitializedSeedBuffer(sourceTexture, alphaThreshold, invertSelection);
            resultInitialized = SDFTools.ConvertToTexture2D(initializedRT);
            RenderTexture.ReleaseTemporary(initializedRT);

            // 计算最近点
            RenderTexture nearestPointRT = SDFTools.ComputeNearestPoint(sourceTexture, alphaThreshold, invertSelection, 32);
            resultNearestPoint = SDFTools.ConvertToTexture2D(nearestPointRT);

            // 生成归一化的距离场
            RenderTexture normalizedSDF = SDFTools.CalculateDistanceField(nearestPointRT, normalize: true);
            resultSDF = SDFTools.ConvertToTexture2D(normalizedSDF);

            // 从归一化的距离场获取最大距离绝对值
            float maxDist = SDFTools.GetMaxDistance(normalizedSDF, isNormalized: true);
            Debug.Log($"[SDF] Max absolute distance: {maxDist:F2} pixels");

            RenderTexture.ReleaseTemporary(nearestPointRT);
            RenderTexture.ReleaseTemporary(normalizedSDF);

            Debug.Log($"SDF generated successfully! Size: {resultSDF.width}x{resultSDF.height}");
        }

        private void SaveTexture(Texture2D texture, string prefix)
        {
            string path = EditorUtility.SaveFilePanel(
                $"Save {prefix} Texture",
                lastSavePath,
                $"{sourceTexture.name}_{prefix}.png",
                "png"
            );

            if (string.IsNullOrEmpty(path)) return;

            // 更新缓存路径
            lastSavePath = System.IO.Path.GetDirectoryName(path);

            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);

            // 刷新资源数据库
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            AssetDatabase.ImportAsset(relativePath);

            Debug.Log($"Texture saved to: {path}");
        }
    }
}
