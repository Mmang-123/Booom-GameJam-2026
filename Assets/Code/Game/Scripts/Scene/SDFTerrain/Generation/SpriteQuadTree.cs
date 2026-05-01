using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Sloane
{
    public class SpriteQuadTree : IDisposable
    {
        const float ALPHA_THRESHOLD = 0.5f;
        private QuadTreeNode m_Root;
        private List<QuadTreeNode> m_NodesWithContent;
        private Texture2D m_Texture;
        private Texture2D m_ReadableTexture;
        private int m_MinSize;
        private bool SingleChannelSource => m_Texture != null &&  m_Texture.format == TextureFormat.R8;

        public Texture2D Texture
        {
            get
            {
                if (m_Texture == null)
                {
                    return null;
                }

                if (!m_Texture.isReadable)
                {
                    if(m_ReadableTexture == null) ValidateTexture();
                    return m_ReadableTexture;
                }

                return m_Texture;
            }
            set
            {
                m_Texture = value;
                ValidateTexture();
            }
        }

        public int MinSize
        {
            get => m_MinSize;
            set
            {
                m_MinSize = value;
            }
        }

        public List<QuadTreeNode> NodesWithContent => m_NodesWithContent;

        private bool m_Disposed;

        public SpriteQuadTree(Texture2D texture, int minSize)
        {
            m_Texture = texture;
            ValidateTexture();
            m_MinSize = minSize;
            m_Root = QuadTreeNode.Create(new RectInt(0, 0, Texture.width, Texture.height));
            m_NodesWithContent = ListPool<QuadTreeNode>.Get();

            Rebuild();
        }

        public void Rebuild()
        {
            m_Root.Reset();
            m_Root.Bounds = new RectInt(0, 0, Texture.width, Texture.height);
            m_NodesWithContent.Clear();
            Subdivide(m_Root);
        }

        private void Subdivide(QuadTreeNode node)
        {
            bool isHomo = IsHomogeneous(node);

            if (node.Bounds.width <= m_MinSize || node.Bounds.height <= m_MinSize)
            {
                return;
            }

            if (isHomo)
            {
                return;
            }

            int halfWidth = node.Bounds.width / 2;
            int halfHeight = node.Bounds.height / 2;

            for (int i = 0; i < 4; i++)
            {
                int x = node.Bounds.x + i % 2 * halfWidth;
                int y = node.Bounds.y + (i / 2) * halfHeight;
                RectInt childBounds = new RectInt(x, y, halfWidth, halfHeight);
                node.Children[i] = QuadTreeNode.Create(childBounds);
                Subdivide(node.Children[i]);
            }
        }

        private bool IsHomogeneous(QuadTreeNode node)
        {
            int flag = 0;

            for (int y = node.Bounds.y; y < node.Bounds.y + node.Bounds.height; y++)
            {
                for (int x = node.Bounds.x; x < node.Bounds.x + node.Bounds.width; x++)
                {
                    Color pixel = Texture.GetPixel(x, y);
                    flag += SingleChannelSource ? (pixel.r >= ALPHA_THRESHOLD ? 1 : 0) : (pixel.a >= ALPHA_THRESHOLD ? 1 : 0);
                }
            }

            if (flag == node.Bounds.height * node.Bounds.width || (node.Bounds.height <= m_MinSize && node.Bounds.width <= m_MinSize && flag >= node.Bounds.height * node.Bounds.width * 0.75))
            {
                m_NodesWithContent.Add(node);
                return true;
            }

            if (flag == 0) return true;

            return false;
        }

        public void DrawGizmos(Color color)
        {
            foreach (var node in m_NodesWithContent)
            {
                node.DrawGizmos(false, color);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    if (m_Root != null) QuadTreeNode.Release(m_Root);
                    if (m_NodesWithContent != null) ListPool<QuadTreeNode>.Release(m_NodesWithContent);
                }

                m_Disposed = true;
            }
        }

        private void ValidateTexture()
        {
            if (m_Texture == null)
            {
                Debug.LogError("Texture is null.");
                return;
            }

            if (!m_Texture.isReadable)
            {
                if (m_ReadableTexture != null)
                {
                    UnityEngine.Object.Destroy(m_ReadableTexture);
                    m_ReadableTexture = null;
                }

                var rt = RenderTexture.GetTemporary(m_Texture.width, m_Texture.height, 0, SingleChannelSource ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(m_Texture, rt);

                var prev = RenderTexture.active;
                RenderTexture.active = rt;

                m_ReadableTexture = new Texture2D(m_Texture.width, m_Texture.height, SingleChannelSource ? TextureFormat.R8 : TextureFormat.RGBA32, false);
                m_ReadableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                m_ReadableTexture.Apply();

                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            if (m_ReadableTexture != null)
            {
                GameObject.DestroyImmediate(m_ReadableTexture);
                m_ReadableTexture = null;
            }
            GC.SuppressFinalize(this);
        }

        ~SpriteQuadTree()
        {
            Dispose(false);
        }
    }
}