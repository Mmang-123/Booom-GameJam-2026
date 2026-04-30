using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace Sloane
{
    public class QuadTreeNode
    {
        public RectInt Bounds;
        public QuadTreeNode[] Children;

        private static ObjectPool<QuadTreeNode> m_NodePool = new ObjectPool<QuadTreeNode>(() => new QuadTreeNode());

        public QuadTreeNode()
        {
            this.Children = new QuadTreeNode[4];
        }

        public void Reset()
        {
            for (int i = 0; i < this.Children.Length; i++)
            {
                if (this.Children[i] != null)
                {
                    Release(this.Children[i]);
                    this.Children[i] = null;
                }
            }
        }

        public static QuadTreeNode Create(RectInt bounds)
        {
            var node = m_NodePool.Get();
            node.Bounds = bounds;
            node.Reset();
            return node;
        }

        public static void Release(QuadTreeNode node)
        {
            node.Reset();
            m_NodePool.Release(node);
        }

        public void DrawGizmos(bool drawChildren, Color color)
        {
            Gizmos.color = color;
            Vector3 center = new Vector3(Bounds.x + Bounds.width / 2.0f, Bounds.y + Bounds.height / 2.0f, 0);
            Vector3 size = new Vector3(Bounds.width, Bounds.height, 1);

            Gizmos.DrawWireCube(center, size);

            if (drawChildren)
            {
                foreach (var child in Children)
                {
                    child?.DrawGizmos(drawChildren, color);
                }
            }
        }
    }
}
