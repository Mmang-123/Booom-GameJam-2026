
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ProgressDisplay : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> m_Renderers = new();
        [SerializeField] private Sprite m_ActiveSprite;
        [SerializeField] private Sprite m_InactiveSprite;

        private void Awake()
        {
            int progress = GameManager.Instance.GetCurrentProgress();
            if (progress <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                SetCount(progress);
            }
        }

        private void SetCount(int count)
        {
            for (int i = 0; i < m_Renderers.Count; i++)
            {
                m_Renderers[i].sprite = i < count ? m_ActiveSprite : m_InactiveSprite;
            }
        }
    }
}