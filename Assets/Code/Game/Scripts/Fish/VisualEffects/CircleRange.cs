using UnityEngine;

namespace Game
{
    public class CircleRange : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Renderer;

        //
        private bool m_Active;
        private float m_A;

        private void Update()
        {
            if (m_Active && m_A < 1f)
            {
                m_A = Mathf.Clamp01(m_A + Time.deltaTime * 4f);
                UpdateA();
            }
            else if (!m_Active && m_A > 0f)
            {
                m_A = Mathf.Clamp01(m_A - Time.deltaTime * 3f);
                UpdateA();
            }
        }

        private void UpdateA()
        {
            m_Renderer.color = new Color(m_Renderer.color.r, m_Renderer.color.g, m_Renderer.color.b, m_A);
        }

        public void SetRadius(float radius)
        {
            transform.localScale = new(radius, radius, 1);
        }

        public void FadeIn(Fish fish, ControlFishConfig config)
        {
            if (fish == null || config == null)
                return;
            if (fish.InfectedLevel < EInfectedLevel.High
            || fish.FishTypeTag.Equals(FishUtils.JellyGleamTag))
                return;

            SetRadius(config.InfectRadius);

            m_Active = true;
        }

        public void FadeOut()
        {
            m_Active = false;
        }
    }
}