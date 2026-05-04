using UnityEngine;
using Sloane;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
#endif

namespace Sloane
{
    public class SDFTerrainObject : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private SpriteRenderer m_OriginalSprite;
        [SerializeField, HideInInspector] private SpriteRenderer m_OutlineSpriteRenderer;
        [SerializeField, HideInInspector] private SpriteRenderer m_SolidSpriteRenderer;
        [SerializeField, HideInInspector] private Sprite m_SDFSprite;
        [SerializeField] private bool m_ShowOutline = true;
        [SerializeField] private bool m_GenerateCollider = true;
        [SerializeField] private Material m_SDFOutlineMaterial;
        [SerializeField] private Material m_SDFSolidMaterial;
        [SerializeField] private string m_UID = string.Empty;
        [SerializeField, HideInInspector] private GameObject m_ColliderRoot;
        [SerializeField, HideInInspector] private int m_SortingLayerID = 0;
        [SerializeField] private int m_SortingOrder = 0;

        SpriteQuadTree spriteQuadTree;

        private void OnValidate()
        {
            if (m_OriginalSprite == null)
            {
                m_OriginalSprite = GetComponent<SpriteRenderer>();
            }

            Sprite sprite = m_OriginalSprite.sprite;
            
            string previousUID = m_UID;

            if (sprite == null)
            {
                m_UID = string.Empty;
            }
            else
            {
                m_UID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite));
            }

            if (m_UID != previousUID)
            {
                m_SDFSprite = null;
            }
        }

        public void UpdateObject()
        {
            OnValidate();
            
            if (m_OriginalSprite == null || m_OriginalSprite.sprite == null)
            {
                return;
            }

            UpdateSpriteTexture();
            GenerateSpriteRenderers();

            if (m_GenerateCollider)
            {
                GeneratePhysicsCollider();
            }
            else
            {
                if (m_ColliderRoot != null)
                {
                    DestroyImmediate(m_ColliderRoot);
                    m_ColliderRoot = null;
                }
            }

            PreviewResult();
        }

        public void PreviewResult()
        {
            m_OriginalSprite.enabled = false;
            if (m_OutlineSpriteRenderer != null)
                m_OutlineSpriteRenderer.enabled = true;
            m_SolidSpriteRenderer.enabled = true;
        }

        public void PreviewOriginal()
        {
            m_OriginalSprite.enabled = true;
            m_OutlineSpriteRenderer.enabled = false;
            m_SolidSpriteRenderer.enabled = false;
        }

        private void UpdateSpriteTexture()
        {
            if (m_SDFSprite != null)
            {
                string path = AssetDatabase.GetAssetPath(m_SDFSprite);
                AssetDatabase.DeleteAsset(path);
            }

            Texture2D sdfTexture = SDFTools.GenerateSDF(m_OriginalSprite.sprite.texture, 0.5f, 128, 256);
            File.WriteAllBytes("Assets/Sprites/Terrain/SDFs" + $"/SDF_{m_UID}.png", sdfTexture.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath("Assets/Sprites/Terrain/SDFs" + $"/SDF_{m_UID}.png") as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = false;
            importer.SaveAndReimport();

            m_SDFSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Terrain/SDFs" + $"/SDF_{m_UID}.png");
        }

        private void GenerateSpriteRenderers()
        {
            // 描边 renderer：根据配置决定是否创建/销毁
            if (m_ShowOutline)
            {
                if (m_OutlineSpriteRenderer == null)
                {
                    GameObject outlineObj = new GameObject("Outline");
                    outlineObj.transform.SetParent(transform);
                    outlineObj.transform.localPosition = Vector3.zero;
                    outlineObj.transform.localRotation = Quaternion.identity;
                    outlineObj.transform.localScale = Vector3.one;
                    m_OutlineSpriteRenderer = outlineObj.AddComponent<SpriteRenderer>();
                }

                int sortingLayerID = m_SortingLayerID;
                m_OutlineSpriteRenderer.sprite = m_SDFSprite;
                m_OutlineSpriteRenderer.material = m_SDFOutlineMaterial;
                m_OutlineSpriteRenderer.sortingLayerID = sortingLayerID;
                m_OutlineSpriteRenderer.sortingOrder = m_SortingOrder;
            }
            else
            {
                if (m_OutlineSpriteRenderer != null)
                {
                    DestroyImmediate(m_OutlineSpriteRenderer.gameObject);
                    m_OutlineSpriteRenderer = null;
                }
            }

            if (m_SolidSpriteRenderer == null)
            {
                GameObject solidObj = new GameObject("Solid");
                solidObj.transform.SetParent(transform);
                solidObj.transform.localPosition = Vector3.zero;
                solidObj.transform.localRotation = Quaternion.identity;
                solidObj.transform.localScale = Vector3.one;

                m_SolidSpriteRenderer = solidObj.AddComponent<SpriteRenderer>();
            }

            m_SolidSpriteRenderer.sprite = m_SDFSprite;
            m_SolidSpriteRenderer.material = m_SDFSolidMaterial;
            m_SolidSpriteRenderer.sortingLayerID = m_SortingLayerID;
            m_SolidSpriteRenderer.sortingOrder = m_SortingOrder + 1;
        }

        private void GeneratePhysicsCollider(int minSize = 4)
        {
            var sprite = m_OriginalSprite.sprite;
            var texture = sprite.texture;
            float ppu = sprite.pixelsPerUnit;

            if (m_ColliderRoot == null)
            {
                m_ColliderRoot = new GameObject("Colliders");
                m_ColliderRoot.transform.SetParent(transform);
                m_ColliderRoot.transform.localPosition = Vector3.zero;
                m_ColliderRoot.transform.localRotation = Quaternion.identity;
                m_ColliderRoot.transform.localScale = Vector3.one;
            }
            else
            {
                for (int i = m_ColliderRoot.transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(m_ColliderRoot.transform.GetChild(i).gameObject);
            }

            spriteQuadTree?.Dispose();
            spriteQuadTree = new SpriteQuadTree(texture, minSize);

            var rects = new List<RectInt>();
            foreach (var node in spriteQuadTree.NodesWithContent)
                rects.Add(node.Bounds);

            var colliderBounds = MergeRects(rects);

            foreach (var bounds in colliderBounds)
            {
                float width = bounds.width / ppu;
                float height = bounds.height / ppu;
                Vector2 localCenter = (bounds.center - new Vector2(texture.width, texture.height) * 0.5f) / ppu;

                GameObject colliderObj = new GameObject($"Collider_{bounds.x}_{bounds.y}");
                colliderObj.transform.SetParent(m_ColliderRoot.transform);
                colliderObj.transform.localPosition = localCenter;
                colliderObj.transform.localRotation = Quaternion.identity;
                colliderObj.transform.localScale = Vector3.one;

                var boxCollider = colliderObj.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(width, height);
            }
        }

        // 对任意一组非重叠 RectInt 做贪心水平+垂直合并，直到无法继续合并
        // 可跨父节点合并相邻等高/等宽的矩形
        private static List<RectInt> MergeRects(List<RectInt> input)
        {
            var rects = new List<RectInt>(input);
            bool changed = true;
            while (changed)
            {
                changed = false;

                // 水平合并：同一 y、同一 height、x 相邻
                for (int i = 0; i < rects.Count && !changed; i++)
                {
                    for (int j = i + 1; j < rects.Count; j++)
                    {
                        var a = rects[i];
                        var b = rects[j];
                        if (a.y == b.y && a.height == b.height)
                        {
                            if (a.x + a.width == b.x)
                            {
                                rects[i] = new RectInt(a.x, a.y, a.width + b.width, a.height);
                                rects.RemoveAt(j);
                                changed = true;
                                break;
                            }
                            if (b.x + b.width == a.x)
                            {
                                rects[i] = new RectInt(b.x, b.y, a.width + b.width, a.height);
                                rects.RemoveAt(j);
                                changed = true;
                                break;
                            }
                        }
                    }
                }

                // 垂直合并：同一 x、同一 width、y 相邻
                for (int i = 0; i < rects.Count && !changed; i++)
                {
                    for (int j = i + 1; j < rects.Count; j++)
                    {
                        var a = rects[i];
                        var b = rects[j];
                        if (a.x == b.x && a.width == b.width)
                        {
                            if (a.y + a.height == b.y)
                            {
                                rects[i] = new RectInt(a.x, a.y, a.width, a.height + b.height);
                                rects.RemoveAt(j);
                                changed = true;
                                break;
                            }
                            if (b.y + b.height == a.y)
                            {
                                rects[i] = new RectInt(b.x, b.y, a.width, a.height + b.height);
                                rects.RemoveAt(j);
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            }
            
            return rects;
        }
    }
    #endif
}
