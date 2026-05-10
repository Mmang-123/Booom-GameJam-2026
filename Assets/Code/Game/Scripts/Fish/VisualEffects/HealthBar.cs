using UnityEngine;

namespace Game
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private SpriteRenderer m_FrameRenderer;
        private Color m_Color;
        private float m_T;

        public void SetT(float t)
        {
            m_T = t;
            UpdateRenderer();
        }

        public void SetColor(Color color)
        {
            m_Color = color;
            m_FrameRenderer.color = color;
            UpdateRenderer();
        }

        private void UpdateRenderer()
        {
            m_Renderer.color = new(m_Color.r, m_Color.g, m_Color.b, m_T);
        }

    }
}