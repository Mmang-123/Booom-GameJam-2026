using UnityEngine;

namespace Mmang.M2D
{
    [DisallowMultipleComponent]
    public class KinematicBody2D : MonoBehaviour
    {
        [Header("碰撞体")]
        private BoxCollider2D m_BoxCollider;

        [Header("设置")]
        public LayerMask m_CollisionMask;
        public float m_SkinWidth = 0.015f;

        // Runtime
        private ContactInfo m_UpContact;
        private ContactInfo m_DownContact;
        private ContactInfo m_LeftContact;
        private ContactInfo m_RightContact;

        private ContactFilter2D m_ContactFilter;

        private const float MIN_MOVE_DISTANCE = 0.001f;


        private void Awake()
        {
            if (m_BoxCollider == null)
            {
                m_BoxCollider = GetComponent<BoxCollider2D>();
            }

            m_ContactFilter = new()
            {
                useLayerMask = true,
                layerMask = m_CollisionMask,
                useTriggers = false
            };
        }

        public void UpdateState()
        {

        }

        private void ResetState()
        {
            m_UpContact = m_DownContact = m_LeftContact = m_RightContact = ContactInfo.None;
        }

        public void Move(Vector2 motion)
        {
            // 这次gj用的是可能不是AABB碰撞盒的地形，还是直接用原生碰撞系统好了
            // temp
            transform.Translate(motion);

            /*
            Bounds bounds = m_BoxCollider.bounds;
            //bounds.Expand(m_SkinWidth * -2);

            CheckCollisions(ref motion, bounds);

            transform.Translate(motion);
            */
        }

        private RaycastHit2D[] m_HitBuffer = new RaycastHit2D[16];
        private void CheckCollisions(ref Vector2 moveAmount, Bounds bounds)
        {
            
        }
    }
}