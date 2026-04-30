using UnityEngine;
using Sloane;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
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
        [SerializeField, HideInInspector] private Material m_SDFOutlineMaterial;
        [SerializeField, HideInInspector] private Material m_SDFSolidMaterial;
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
            if (sprite == null)
            {
                m_UID = string.Empty;
            }
            else
            {
                m_UID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite));
            }
        }

        public void UpdateObject()
        {
            if (m_OriginalSprite == null || m_OriginalSprite.sprite == null)
            {
                return;
            }

            UpdateSpriteTexture();
            GenerateSpriteRenderers();
            GeneratePhysicsCollider();
            PreviewResult();
        }

        public void PreviewResult()
        {
            m_OriginalSprite.enabled = false;
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

            Texture2D sdfTexture = SDFTools.GenerateSDF(m_OriginalSprite.sprite.texture, 0.5f, 64, 128);
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
            m_SolidSpriteRenderer.sortingLayerID = sortingLayerID;
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

            foreach (var node in spriteQuadTree.NodesWithContent)
            {
                float width = node.Bounds.width / ppu;
                float height = node.Bounds.height / ppu;
                Vector2 localCenter = (node.Bounds.center - new Vector2(texture.width, texture.height) * 0.5f) / ppu;

                GameObject colliderObj = new GameObject($"Collider_{node.Bounds.x}_{node.Bounds.y}");
                colliderObj.transform.SetParent(m_ColliderRoot.transform);
                colliderObj.transform.localPosition = localCenter;
                colliderObj.transform.localRotation = Quaternion.identity;
                colliderObj.transform.localScale = Vector3.one;

                var boxCollider = colliderObj.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(width, height);
            }
        }

#endif
    }
}
