using UnityEngine;

namespace Game
{
    public class VelocityObject : MonoBehaviour
    {
        public Vector2 Velocity { get; private set; }
        public Vector2 LastFramePosition { get; private set; }

        private static readonly int s_ShaderID_Velocity = Shader.PropertyToID("_Velocity");

        private Renderer m_Renderer;
        private MaterialPropertyBlock m_MPB;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            if (m_Renderer != null)
                m_MPB = new MaterialPropertyBlock();
        }

        private void Start()
        {
            LastFramePosition = transform.position;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector2 pos = transform.position;
            Vector2 offset = pos - LastFramePosition;
            Velocity = offset / dt / 8.0f;
            LastFramePosition = pos;

            if (m_Renderer != null && m_MPB != null)
            {
                m_Renderer.GetPropertyBlock(m_MPB);
                m_MPB.SetVector(s_ShaderID_Velocity, Velocity);
                m_Renderer.SetPropertyBlock(m_MPB);
            }
        }
    }
}
