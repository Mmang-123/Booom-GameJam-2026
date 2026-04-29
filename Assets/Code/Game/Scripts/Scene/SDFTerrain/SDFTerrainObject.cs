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
        [SerializeField, HideInInspector] private int m_UID = -1;

        private void OnValidate()
        {
            if (m_OriginalSprite == null)
            {
                m_OriginalSprite = GetComponent<SpriteRenderer>();
            }

            if (m_UID == -1)
            {
                Random.InitState((int)System.DateTime.Now.Ticks);
                m_UID = Random.Range(0, int.MaxValue);
            }
        }

        public void UpdateObject()
        {
            if (m_OriginalSprite == null || m_OriginalSprite.sprite == null)
            {
                return;
            }
            UpdateSpriteTexture();
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

        #endif
    }
}
