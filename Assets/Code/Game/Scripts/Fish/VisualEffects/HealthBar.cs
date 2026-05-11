using UnityEngine;

namespace Game
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private SpriteRenderer m_FrameRenderer;
        [SerializeField] private int m_SegmentCount = 3;
        private Color m_Color;
        private float m_T;

        private int SegmentPixelCount => 48;

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

        public void SetSegmentCount(int segmentCount)
        {
            m_SegmentCount = segmentCount;
            ApplySegmentCount();
        }

        private void UpdateRenderer()
        {
            m_Renderer.color = new(m_Color.r, m_Color.g, m_Color.b, m_T);
        }

        [ContextMenu("Apply")]
        private void ApplySegmentCount()
        {
            int segmentTotalPixelCount = SegmentPixelCount * m_SegmentCount + m_SegmentCount - 1;
            int framePixelCount = segmentTotalPixelCount + 4;

            m_FrameRenderer.size = new(framePixelCount / 16.0f, m_FrameRenderer.size.y);
            m_Renderer.transform.localScale = new(segmentTotalPixelCount / 16.0f / 3.0f, 1f, 1f);

            var material = m_Renderer.sharedMaterial;
            if (material != null)
            {
                material.SetInt("_SegmentCount", m_SegmentCount);
            }
        }

    }
}