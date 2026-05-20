
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LocalizationUI : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;
        [SerializeField] private Sprite m_ChineseSprite;
        [SerializeField] private Sprite m_EnglishSprite;

        private void Start()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            OnLanguageChanged(LocalizationManager.CurrentLanguage);
        }
        
        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            if (LocalizationManager.InstanceValid)
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        public void OnLanguageChanged(LocalizationManager.ELanguage language)
        {
            switch (language)
            {
                case LocalizationManager.ELanguage.Chinese:
                    m_Renderer.sprite = m_ChineseSprite;
                    break;
                case LocalizationManager.ELanguage.English:
                    m_Renderer.sprite = m_EnglishSprite;
                    break;
            }
        }
    }
}