using Mmang.Util;
using UnityEngine;

namespace Mmang.Generations
{
    public static class GenerationEditorUtil
    {
        public static void GeneratePositions(GameObject selection, float density, int maxCount, System.Action<Vector3, Vector3> addPointFunc = null)
        {
            if (addPointFunc == null)
                return;

            // mesh
            if (selection.TryGetComponent(out MeshFilter meshFilter))
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                    return;

                //
                Matrix4x4 localToWorldMat = meshFilter.transform.localToWorldMatrix;
                Bounds bounds = mesh.bounds;

                // 网格数据
                var oTriangles = mesh.triangles;
                var oVertices = mesh.vertices;
                var oColors = mesh.colors;
                var oNormals = mesh.normals;

                // 三角形面积
                int triangleCount = mesh.triangles.Length / 3;
                float[] triangleAreas = GetTriangleAreas(oTriangles, oVertices, out float totalArea);

                // 网格体积
                Vector3 meshSize = new
                (
                    bounds.size.x * meshFilter.transform.lossyScale.x,
                    bounds.size.y * meshFilter.transform.lossyScale.y,
                    bounds.size.z * meshFilter.transform.lossyScale.z
                );
                meshSize += Vector3.one;
                float meshVolume = meshSize.x * meshSize.y * meshSize.z;

                // 生成数量
                int generationCount = Mathf.Min(maxCount, Mathf.RoundToInt(meshVolume * density));
            
                for (int i = 0; i < generationCount; i++)
                {
                    //
                    float rand = Random.value * totalArea;
                    int triangleIndex = -1;
                    float areaMatch = 0f;
                    for (int tI = 0; tI < triangleCount; tI++)
                    {
                        areaMatch += triangleAreas[tI];
                        if (rand <= areaMatch)
                        {
                            triangleIndex = tI;
                            break;
                        }
                    }

                    //
                    if (triangleIndex == -1)
                    {
                        Debug.Log("无法定位三角形");
                        continue;
                    }

                    //
                    Vector3 v1 = oVertices[oTriangles[triangleIndex * 3 + 0]];
                    Vector3 v2 = oVertices[oTriangles[triangleIndex * 3 + 1]];
                    Vector3 v3 = oVertices[oTriangles[triangleIndex * 3 + 2]];
                    
                    Vector3 resultPositionOS = RandomUtil.GetRandomPointInTriangleFast(v1, v2, v3);
                    Vector3 resultPositionWS = localToWorldMat.MultiplyPoint3x4(resultPositionOS);
                    Vector3 resultNormal = oNormals[oTriangles[triangleIndex * 3 + 1]];

                    addPointFunc(resultPositionWS, resultNormal);
                }
            }
        }

        public static float[] GetTriangleAreas(int[] triangleIncludes, Vector3[] vertexs, out float totalArea)
        {
            totalArea = 0f;
            int triangleCount = triangleIncludes.Length / 3;
            float[] areas = new float[triangleCount];
            for (int i = 0; i < triangleCount; i++)
            {
                Vector3 v1 = vertexs[triangleIncludes[i * 3 + 0]];
                Vector3 v2 = vertexs[triangleIncludes[i * 3 + 1]];
                Vector3 v3 = vertexs[triangleIncludes[i * 3 + 2]];                
                float area = MathUtil.GetTriangleArea(v1, v2, v3);
                areas[i] = area;
                totalArea += area;
            }
            return areas;
        }
    }
}