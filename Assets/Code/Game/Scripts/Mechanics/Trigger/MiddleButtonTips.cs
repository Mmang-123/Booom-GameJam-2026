using Mmang.Game;
using UnityEngine;

namespace Game
{
    public class MiddleButtonTips : PlayerTrigger
    {
        [SerializeField] private SpriteRenderer m_Renderer;

        public static bool Triggered = false;
        private bool m_Show;

        private void Update()
        {
            if (m_Show)
            {
                var color = m_Renderer.color;
                if (color.a < 1f)
                {
                    color.a = Mathf.Min(1f, color.a + Time.deltaTime);
                    m_Renderer.color = color;
                }
            }
        }

        protected override void Trigger(Fish fish)
        {
            if (!Triggered)
            {
                m_Show = true;
            }
            m_StopCheck = true;
        }
    }
}