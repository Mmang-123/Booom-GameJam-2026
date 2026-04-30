using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace Sloane
{
    public class SpriteBoxMeshGroup : SpriteBoxGroup
    {
        [SerializeField]
        protected int m_PixelPerUnit = 8;
        [SerializeField]
        protected Material m_Material;
        [SerializeField]
        protected bool m_IsTrigger;

        protected Material m_SharedMaterial;
        protected List<SpriteBox> m_SpriteBoxes = new List<SpriteBox>();
        public bool IsTrigger => m_IsTrigger;
        public Material SharedMaterial => m_SharedMaterial;
        public override void Rebuild()
        {
            base.Rebuild();
            if(m_SharedMaterial == null)
            {
                m_SharedMaterial = new Material(m_Material)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            
            m_SharedMaterial.SetTexture("_MainTex", m_SourceTexture);
            RebuildSpriteBoxes();
        }

        public override void CleanUp()
        {
            base.CleanUp();

            foreach (var box in m_SpriteBoxes)
            {
                if(box != null) SpriteBox.BoxPool.Release(box);
            }

            m_SpriteBoxes.Clear();
        }

        public void AddSpriteBoxed(SpriteBox spriteBox)
        {
            m_SpriteBoxes.Add(spriteBox);
        }

        protected void RebuildSpriteBoxes()
        {
            if (m_SpriteQuadTree == null) return;

            foreach (var node in m_SpriteQuadTree.NodesWithContent)
            {
                float width = (float)node.Bounds.width / m_PixelPerUnit;
                float height = (float)node.Bounds.height / m_PixelPerUnit;
                Vector2 center = node.Bounds.center - new Vector2(m_SourceTexture.width, m_SourceTexture.height) / 2.0f;
                center /= m_PixelPerUnit;
                Vector3 pos = transform.TransformPoint(center);

                float uvWidth = (float)node.Bounds.width / m_SourceTexture.width;
                float uvHeight = (float)node.Bounds.height / m_SourceTexture.height;
                Vector2 uvCenter = new Vector2(node.Bounds.center.x / m_SourceTexture.width, node.Bounds.center.y / m_SourceTexture.height);

                var spriteBox = SpriteBox.BoxPool.Get();

#if UNITY_EDITOR
                if (spriteBox == null) continue;
#endif

                spriteBox.SetParent(this);
                spriteBox.UpdateData(pos, width, height, uvCenter, uvWidth, uvHeight, m_SharedMaterial);

                AddSpriteBoxed(spriteBox);
            }
        }
    }
}
