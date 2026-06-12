using UnityEngine;
using UnityEditor;
using System.IO;

namespace Game.Editor
{
    /// <summary>
    /// 生成单位正方形网格（1×1），沿 Y 轴细分。
    /// 顶点色 R 通道从下到上 0→1。
    /// </summary>
    public class SeagrassMeshGenerator : EditorWindow
    {
        private int m_SegmentsY = 8;
        private string m_SavePath = "Assets/Meshes/Seagrass.asset";

        [MenuItem("Tools/Mesh/Generate Seagrass Mesh")]
        private static void Open()
        {
            var window = GetWindow<SeagrassMeshGenerator>("Seagrass Mesh");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("单位正方形（1×1），Y 轴细分 + 顶点色 R 梯度");
            EditorGUILayout.Space();

            m_SegmentsY = EditorGUILayout.IntSlider("Y 轴段数", m_SegmentsY, 1, 32);
            m_SavePath = EditorGUILayout.TextField("保存路径", m_SavePath);

            EditorGUILayout.Space();

            if (GUILayout.Button("生成并保存", GUILayout.Height(30)))
            {
                GenerateAndSave();
            }
        }

        private void GenerateAndSave()
        {
            var mesh = GenerateMesh(m_SegmentsY);

            // 确保目录存在
            string dir = Path.GetDirectoryName(m_SavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 保存为 .asset
            AssetDatabase.CreateAsset(mesh, m_SavePath);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", $"网格已保存到:\n{m_SavePath}\n顶点数: {mesh.vertexCount}", "OK");
            EditorGUIUtility.PingObject(mesh);
        }

        /// <summary>纯函数：生成网格，不依赖 Editor API</summary>
        public static Mesh GenerateMesh(int segmentsY)
        {
            // segments 段 → segments+1 行顶点
            int rows = segmentsY + 1;
            int cols = 2; // 左右两列

            var vertices = new Vector3[rows * cols];
            var colors = new Color[rows * cols];

            for (int r = 0; r < rows; r++)
            {
                float t = (float)r / segmentsY; // 0 → 1，下到上
                float y = t;                     // 单位高度

                // 左列 x = -0.5，右列 x = 0.5
                vertices[r * cols + 0] = new Vector3(-0.5f, y, 0f);
                vertices[r * cols + 1] = new Vector3( 0.5f, y, 0f);

                float rChannel = t; // 下=0，上=1
                var color = new Color(rChannel, 0f, 0f, 1f);
                colors[r * cols + 0] = color;
                colors[r * cols + 1] = color;
            }

            // 三角形：每段 2 个三角形
            int triCount = segmentsY * 2;
            var triangles = new int[triCount * 3];
            int ti = 0;
            for (int r = 0; r < segmentsY; r++)
            {
                int bl = r * cols + 0;      // bottom-left
                int br = r * cols + 1;      // bottom-right
                int tl = (r + 1) * cols + 0; // top-left
                int tr = (r + 1) * cols + 1; // top-right

                // tri 1: bl → tl → br
                triangles[ti++] = bl;
                triangles[ti++] = tl;
                triangles[ti++] = br;

                // tri 2: br → tl → tr
                triangles[ti++] = br;
                triangles[ti++] = tl;
                triangles[ti++] = tr;
            }

            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uv[i] = new Vector2(vertices[i].x + 0.5f, vertices[i].y);
            }

            var mesh = new Mesh
            {
                name = $"Seagrass_{segmentsY}",
                vertices = vertices,
                colors = colors,
                triangles = triangles,
                uv = uv
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
