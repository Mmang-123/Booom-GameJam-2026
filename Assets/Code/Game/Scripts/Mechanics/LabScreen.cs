
using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class LabScreen : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Map;
        [SerializeField] private SpriteRenderer m_Text;
    
        public void SwitchToText()
        {
            DOTween.To(() => m_Map.color.a, val => m_Map.color = new Color(1, 1, 1, val), 0f, 1f)
            .OnComplete(() => DOTween.To(() => m_Text.color.a, val => m_Text.color = new Color(1, 1, 1, val), 0f, 1f));
        }
    }
}