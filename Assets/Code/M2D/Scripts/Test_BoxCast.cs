using UnityEngine;

namespace Mmang.M2D
{
    public class Test_BoxCast : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D m_Collider;
        [SerializeField] private LayerMask m_LayerMask;

        [SerializeField] private Vector2 m_CastDirection;
        [SerializeField] private float m_CastDistance;

        private RaycastHit2D[] m_HitBuffer = new RaycastHit2D[16];

        [ContextMenu("Test1")]
        public void Test1()
        {
            /*
            ContactFilter2D contactFilter = new()
            {
                useLayerMask = true,
                layerMask = m_LayerMask,
                useTriggers = false,
            };

            var bounds = m_Collider.bounds;

            int hitCount = Physics2D.BoxCast(
                bounds.center,
                bounds.size,
                0f,
                m_CastDirection.normalized,
                contactFilter,
                m_HitBuffer,
                m_CastDistance
            );
            */
        }
    }
}