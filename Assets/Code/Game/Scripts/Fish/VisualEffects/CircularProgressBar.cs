using UnityEngine;

namespace Game
{
    public class CircularProgressBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private Color m_Color;
        [SerializeField] private float m_T;

        public void SetT(float t)
        {
            m_T = t;
            UpdateRenderer();
        }

        public void SetColor(Color color)
        {
            m_Color = color;
        }

        public void SetFish(Fish fish)
        {
            SetColor(fish.BodyColor);
            if (fish.FishTypeTag.Equals(FishUtils.GolemFishTag))
            {
                transform.localScale = new Vector3(7, 7, 7);
            }
            else
            {
                transform.localScale = new Vector3(4, 4, 4);
            }
        }

        private void UpdateRenderer()
        {
            m_Renderer.color = new(m_Color.r, m_Color.g, m_Color.b, Mathf.Clamp01(m_T));
        }
    }
}