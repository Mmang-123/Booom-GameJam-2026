using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace Sloane
{
    public class SpriteBox : MonoBehaviour
    {
        protected static ObjectPool<SpriteBox> m_BoxPool = new ObjectPool<SpriteBox>(CreateSpriteBox, GetSpriteBox, ReleaseSpriteBox);
        public static ObjectPool<SpriteBox> BoxPool => m_BoxPool;
        protected static Dictionary<Collider2D, SpriteBox> m_ColliderSpriteBoxDic = new Dictionary<Collider2D, SpriteBox>();
        protected BoxCollider2D m_Collider;
        protected MeshRenderer m_MeshRenderer;
        protected MeshFilter m_MeshFilter;
        protected SpriteBoxMeshGroup m_Parent;
        protected Mesh m_Mesh;
        protected bool m_Initialize;
        protected Vector2 m_Size;
        protected Vector3[] m_Vertices = new Vector3[4];
        protected int[] m_Triangles = new int[6];
        Vector2[] m_UVs = new Vector2[4];
        protected Vector2 m_UVMin;
        protected Vector2 m_UVMax;

        public void Init(MeshRenderer meshRenderer, MeshFilter meshFilter, BoxCollider2D collider)
        {
            if (m_Initialize) return;

            m_MeshRenderer = meshRenderer;
            m_MeshFilter = meshFilter;
            m_Collider = collider;

            if (!m_ColliderSpriteBoxDic.ContainsKey(m_Collider) && enabled) m_ColliderSpriteBoxDic.Add(collider, this);
        }

        public void SetParent(SpriteBoxMeshGroup parent)
        {
            m_Parent = parent;
            m_Collider.isTrigger = m_Parent.IsTrigger;
            transform.SetParent(parent.transform);
        }

        public void UpdateData(Vector2 center, float width, float height, Vector2 uvCenter, float uvWidth, float uvHeight, Material material = null)
        {
            float halfUvWidth = uvWidth / 2;
            float halfUvHeight = uvHeight / 2;
            Vector2 uvMin = uvCenter - new Vector2(halfUvWidth, halfUvHeight);
            Vector2 uvMax = uvCenter + new Vector2(halfUvWidth, halfUvHeight);

            UpdateData(center, width, height, uvMin, uvMax, material);
        }

        public void UpdateData(Vector2 center, float width, float height, Vector2 uvMin, Vector2 uvMax, Material material = null)
        {
            SetUV(uvMin, uvMax);
            SetMaterial(material);
            UpdateData(center, width, height);
        }

        public void UpdateData(Vector2 center, float width, float height)
        {
            SetSize(width, height);
            transform.position = center;
        }

        public void SetSize(float width, float height)
        {
            m_Size = new Vector2(width, height);
            m_Collider.size = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

            UpdateMesh();
        }

        private void SetUV(Vector2 uvMin, Vector2 uvMax)
        {
            m_UVMin = uvMin;
            m_UVMax = uvMax;
        }

        private void SetMaterial(Material material)
        {
            if (material != null) m_MeshRenderer.material = material;
        }

        private void UpdateMesh()
        {
            if (m_Mesh == null) m_Mesh = new Mesh();

            m_Vertices[0] = new Vector3(-m_Size.x / 2, -m_Size.y / 2, 0); // Bottom left
            m_Vertices[1] = new Vector3(m_Size.x / 2, -m_Size.y / 2, 0);  // Bottom right
            m_Vertices[2] = new Vector3(-m_Size.x / 2, m_Size.y / 2, 0);  // Top left
            m_Vertices[3] = new Vector3(m_Size.x / 2, m_Size.y / 2, 0);   // Top right

            // 为每个顶点分配三角形（两个三角形组成一个矩形）
            m_Triangles[0] = 0;
            m_Triangles[1] = 2;
            m_Triangles[2] = 1;
            m_Triangles[3] = 2;
            m_Triangles[4] = 3;
            m_Triangles[5] = 1;

            // 设置UV坐标
            m_UVs[0] = new Vector2(m_UVMin.x, m_UVMin.y);
            m_UVs[1] = new Vector2(m_UVMax.x, m_UVMin.y);
            m_UVs[2] = new Vector2(m_UVMin.x, m_UVMax.y);
            m_UVs[3] = new Vector2(m_UVMax.x, m_UVMax.y);
            m_Mesh.vertices = m_Vertices;
            m_Mesh.triangles = m_Triangles;
            m_Mesh.uv = m_UVs;

            m_Mesh.RecalculateNormals();
            m_Mesh.RecalculateBounds();

            m_MeshFilter.mesh = m_Mesh;
        }


        void OnEnable()
        {
            if (m_Collider == null) return;

            if (!m_ColliderSpriteBoxDic.ContainsKey(m_Collider)) m_ColliderSpriteBoxDic.Add(m_Collider, this);
        }

        void OnDisable()
        {
            if (m_Collider == null) return;

            if (m_ColliderSpriteBoxDic.ContainsKey(m_Collider)) m_ColliderSpriteBoxDic.Remove(m_Collider);
        }

        public static SpriteBox GetSpriteBox(Collider2D collider)
        {
            if (m_ColliderSpriteBoxDic.ContainsKey(collider)) return m_ColliderSpriteBoxDic[collider];
            return null;
        }

        public Rect GetRect()
        {
            return new Rect(new Vector2(transform.position.x, transform.position.y) - m_Size / 2.0f, m_Size);
        }

        public Rect GetNormalRect()
        {
            var size = new Vector2(Mathf.Abs(m_Size.x), Mathf.Abs(m_Size.y));
            return new Rect(new Vector2(transform.position.x, transform.position.y) - size / 2.0f, size);
        }

        // 传入世界坐标下的Rect执行Slice
        public SpriteBox Slice(Rect cut)
        {
            var curRect = GetRect();
            var tempList = ListPool<Rect>.Get();
            tempList.Add(curRect);
            RectUtil.SliceRectWithRect(tempList, cut);
            SpriteBox outputBox = null;

            Vector2 uvMin = m_UVMin;
            Vector2 uvSize = m_UVMax - m_UVMin;

            foreach (var rect in tempList)
            {
                if(rect.width == 0 || rect.height == 0) continue;
                
                if (cut.Contains(rect))
                {
                    outputBox = this;
                    Vector2 currentUVMin = new Vector2((rect.xMin - curRect.xMin) / curRect.width, (rect.yMin - curRect.yMin) / curRect.height) * uvSize + uvMin;
                    Vector2 currentUVMax = new Vector2((rect.xMax - curRect.xMin) / curRect.width, (rect.yMax - curRect.yMin) / curRect.height) * uvSize + uvMin;

                    outputBox.UpdateData(rect.center, rect.width, rect.height, currentUVMin, currentUVMax);
                }
                else
                {
                    var newBox = BoxPool.Get();
#if UNITY_EDITOR
                    if (newBox == null) continue;
#endif
                    Vector2 currentUVMin = new Vector2((rect.xMin - curRect.xMin) / curRect.width, (rect.yMin - curRect.yMin) / curRect.height) * uvSize + uvMin;
                    Vector2 currentUVMax = new Vector2((rect.xMax - curRect.xMin) / curRect.width, (rect.yMax - curRect.yMin) / curRect.height) * uvSize + uvMin;

                    newBox.UpdateData(rect.center, rect.width, rect.height, currentUVMin, currentUVMax, m_MeshRenderer.material);
                    newBox.SetParent(m_Parent);

                    m_Parent.AddSpriteBoxed(newBox);
                }
            }

            return outputBox;
        }

        protected static SpriteBox CreateSpriteBox()
        {
            GameObject go = new GameObject("Sprite Box")
            {
                hideFlags = HideFlags.DontSave
            };

            go.SetActive(false);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            var meshFilter = go.AddComponent<MeshFilter>();
            var collider = go.AddComponent<BoxCollider2D>();

            var spriteBox = go.AddComponent<SpriteBox>();
            spriteBox.Init(meshRenderer, meshFilter, collider);

            return spriteBox;
        }

        protected static void GetSpriteBox(SpriteBox spriteBox)
        {
            if (spriteBox == null) return;
            spriteBox.gameObject.SetActive(true);
        }

        protected static void ReleaseSpriteBox(SpriteBox spriteBox)
        {
            if (spriteBox == null) return;
            spriteBox.transform.SetParent(null);
            spriteBox.transform.localScale = Vector3.one;
            spriteBox.gameObject.SetActive(false);
        }
    }
}
