using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    /// <summary>
    /// 单个残影实例：快照源对象下所有 SpriteRenderer，逐渐淡出后归还对象池。
    /// </summary>
    public class AfterimageInstance : MonoBehaviour
    {
        private readonly List<SpriteRenderer> m_ChildRenderers = new();

        private float m_Duration;
        private float m_Timer;
        private float m_StartAlpha;
        private IObjectPool<AfterimageInstance> m_Pool;
        private bool m_Active;

        /// <summary>
        /// 初始化残影：从 source 下所有 SpriteRenderer 拍快照。
        /// </summary>
        public void Init(Transform source, Color tintColor, float duration, IObjectPool<AfterimageInstance> pool, Material overrideMaterial = null)
        {
            m_Pool        = pool;
            m_Duration    = duration;
            m_Timer       = 0f;
            m_StartAlpha  = tintColor.a;
            m_Active      = true;

            // 将根节点重置到世界原点，方便子节点直接使用世界坐标
            transform.position   = Vector3.zero;
            transform.rotation   = Quaternion.identity;
            transform.localScale = Vector3.one;

            var sources = source.GetComponentsInChildren<SpriteRenderer>();

            // 复用已有的子渲染器，按需增补
            for (int i = m_ChildRenderers.Count; i < sources.Length; i++)
            {
                var child = new GameObject("AfterimageRenderer");
                child.transform.SetParent(transform, false);
                m_ChildRenderers.Add(child.AddComponent<SpriteRenderer>());
            }

            // 同步快照数据
            for (int i = 0; i < m_ChildRenderers.Count; i++)
            {
                var sr = m_ChildRenderers[i];
                if (i < sources.Length)
                {
                    var src = sources[i];
                    // 因为根节点是世界原点单位矩阵，local == world
                    sr.transform.position   = src.transform.position;
                    sr.transform.rotation   = src.transform.rotation;
                    sr.transform.localScale = src.transform.lossyScale;

                    sr.sprite         = src.sprite;
                    sr.color          = tintColor * src.color; // 叠加原始颜色
                    sr.flipX          = src.flipX;
                    sr.flipY          = src.flipY;
                    sr.sortingLayerID = src.sortingLayerID;
                    sr.sortingOrder   = src.sortingOrder - 1; // 渲染在原始精灵下方
                    sr.material       = overrideMaterial != null ? overrideMaterial : src.material;
                    sr.gameObject.SetActive(true);
                }
                else
                {
                    sr.gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (!m_Active) return;

            m_Timer += Time.deltaTime;
            float alpha = Mathf.Lerp(m_StartAlpha, 0f, m_Timer / m_Duration);

            foreach (var sr in m_ChildRenderers)
            {
                if (!sr.gameObject.activeSelf) break;
                var c = sr.color;
                c.a    = alpha;
                sr.color = c;
            }

            if (m_Timer >= m_Duration)
            {
                m_Active = false;
                m_Pool.Release(this);
            }
        }

        /// <summary>
        /// 归还对象池时由 AfterimagePool 调用，隐藏所有子渲染器。
        /// </summary>
        public void OnReturnToPool()
        {
            m_Active = false;
            foreach (var sr in m_ChildRenderers)
                sr.gameObject.SetActive(false);
        }
    }
}
