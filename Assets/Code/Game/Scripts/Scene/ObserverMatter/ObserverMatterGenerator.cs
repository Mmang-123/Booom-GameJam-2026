using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ObserverMatterGenerator : MonoBehaviour
    {
        [Header("生成设置")]
        [SerializeField] private GameObject m_UnitPrefab;
        [SerializeField] private Transform m_GenerationRoot;
        [SerializeField] private int m_GridPixelSize = 4;
        [SerializeField, Range(0f, 1f)] private float m_AlphaThreshold = 0.1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            var sr = GetComponent<SpriteRenderer>();
        }

        public void Editor_Generate()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null)
            {
                Debug.LogWarning("[ObserverMatterGenerator] No sprite found on SpriteRenderer.");
                return;
            }

            if (m_UnitPrefab == null)
            {
                Debug.LogWarning("[ObserverMatterGenerator] Unit prefab is not assigned.");
                return;
            }

            if (m_GenerationRoot == null)
            {
                Debug.LogWarning("[ObserverMatterGenerator] Generation root is not assigned.");
                return;
            }

            // Clear previously generated children
            var existing = new List<GameObject>();
            foreach (Transform child in m_GenerationRoot)
                existing.Add(child.gameObject);
            foreach (var go in existing)
                UnityEditor.Undo.DestroyObjectImmediate(go);

            var sprite = sr.sprite;
            bool useSingleChannel = IsSingleChannelFormat(sprite.texture.format);
            var texture = GetReadableTexture(sprite.texture);
            float ppu = sprite.pixelsPerUnit;

            // Sprite rect in texture pixel space
            Rect rect = sprite.rect;

            // Pivot offset from bottom-left of sprite rect, in world units
            float pivotX = sprite.pivot.x / ppu;
            float pivotY = sprite.pivot.y / ppu;

            int pixelStep = Mathf.Max(1, m_GridPixelSize);

            // Number of grid cells (in pixel space)
            int cols = Mathf.Max(1, Mathf.FloorToInt(rect.width  / pixelStep));
            int rows = Mathf.Max(1, Mathf.FloorToInt(rect.height / pixelStep));

            // Cell size in world units
            float cellW = pixelStep / ppu;
            float cellH = pixelStep / ppu;

            UnityEditor.Undo.SetCurrentGroupName("Generate Observer Matter");
            int group = UnityEditor.Undo.GetCurrentGroup();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Cell center in local sprite space (pivot at origin)
                    float localX = -pivotX + (col + 0.5f) * cellW;
                    float localY = -pivotY + (row + 0.5f) * cellH;

                    // Corresponding pixel position within the texture
                    int texX = Mathf.RoundToInt(rect.x + (localX + pivotX) * ppu);
                    int texY = Mathf.RoundToInt(rect.y + (localY + pivotY) * ppu);

                    // Clamp to texture bounds
                    texX = Mathf.Clamp(texX, (int)rect.x, (int)(rect.x + rect.width  - 1));
                    texY = Mathf.Clamp(texY, (int)rect.y, (int)(rect.y + rect.height - 1));

                    Color pixel = texture.GetPixel(texX, texY);
                    float pixelValue = useSingleChannel ? pixel.r : pixel.a;
                    if (pixelValue < m_AlphaThreshold)
                        continue;

                    Vector3 worldPos = transform.TransformPoint(new Vector3(localX, localY, 0f));

                    var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                    var scene = stage != null ? stage.scene : m_GenerationRoot.gameObject.scene;
                    var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_UnitPrefab, scene);
                    instance.transform.SetParent(m_GenerationRoot, true);
                    instance.transform.position = worldPos;
                    UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Generate Observer Matter");
                }
            }

            UnityEditor.Undo.CollapseUndoOperations(group);

            sr.enabled = false;
            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.EditorUtility.SetDirty(m_GenerationRoot.gameObject);
        }

        private static bool IsSingleChannelFormat(TextureFormat format)
        {
            return format == TextureFormat.R8
                || format == TextureFormat.R16
                || format == TextureFormat.RFloat
                || format == TextureFormat.RHalf;
        }

        private static Texture2D GetReadableTexture(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readable.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return readable;
        }
#endif
    }
}
