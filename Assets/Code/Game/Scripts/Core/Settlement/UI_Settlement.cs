
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class UI_Settlement : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> m_Renderers = new();

        [SerializeField] private Sprite m_InactiveSprite;
        [SerializeField] private Sprite m_ActiveSprite;

        // Runtime
        private float m_A;
        private float m_CurrentPointIndex;
        private int m_TargetPointCount;
        public bool ShowCompleted => m_CurrentPointIndex >= m_TargetPointCount;

        public bool m_FadeOut;

        public float UpdateRate => 8f;
        public float AlphaUpdateRate => 3f;

        public void Show(int count)
        {
            foreach (var renderer in m_Renderers)
            {
                renderer.sprite = m_InactiveSprite;
                renderer.color = Color.clear;
            }

            gameObject.SetActive(true);
            m_TargetPointCount = count;
            m_CurrentPointIndex = 0;
            m_A = 0f;
        }

        public void Hide()
        {
            m_FadeOut = true;
        }
        
        private void SetA(float a)
        {
            foreach (var renderer in m_Renderers)
            {
                renderer.color = new(1, 1, 1, a);
            }
        }

        private void Update()
        {
            if (m_FadeOut)
            {
                if (m_A > 0f)
                {
                    m_A = Mathf.Clamp01(m_A - Time.deltaTime * AlphaUpdateRate);
                    SetA(m_A);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                return;
            }

            if (m_A < 1f)
            {
                m_A = Mathf.Clamp01(m_A + Time.deltaTime * AlphaUpdateRate);
                SetA(m_A);
                return;
            }

            if (m_CurrentPointIndex < m_TargetPointCount)
            {
                m_CurrentPointIndex = Mathf.Clamp(m_CurrentPointIndex + Time.deltaTime * UpdateRate, 0, m_TargetPointCount);
                
                int count = Mathf.FloorToInt(m_CurrentPointIndex);
                for (int i = 0; i < count; i++)
                {
                    m_Renderers[i].sprite = m_ActiveSprite;
                }
            }
        }
    }
}