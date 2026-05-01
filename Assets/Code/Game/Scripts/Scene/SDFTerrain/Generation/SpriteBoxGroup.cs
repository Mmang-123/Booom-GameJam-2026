using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Sloane
{
    public class SpriteBoxGroup : MonoBehaviour
    {
        [SerializeField]
        protected Texture2D m_SourceTexture;
        [SerializeField, Min(1)]
        protected int m_MinSize = 1;
        protected SpriteQuadTree m_SpriteQuadTree;
        void Start()
        {
            Rebuild();
        }

        public virtual void Rebuild()
        {
            CleanUp();
            if (m_SourceTexture == null) return;
            m_SpriteQuadTree = new SpriteQuadTree(m_SourceTexture, m_MinSize);
        }

        public virtual void CleanUp()
        {
            if (m_SpriteQuadTree == null) return;
            m_SpriteQuadTree.Dispose();
            m_SpriteQuadTree = null;
        }

        void OnDestroy()
        {
            CleanUp();
        }

#if UNITY_EDITOR

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(DelayedRebuild());
        }

        IEnumerator DelayedRebuild()
        {
            yield return new WaitForEndOfFrameUnit();
            if(!Application.isPlaying) Rebuild();
        }

#endif
    }
}
