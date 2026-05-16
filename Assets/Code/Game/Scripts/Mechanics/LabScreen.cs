
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class LabScreen : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Map;
        [SerializeField] private SpriteRenderer m_Text;
        [SerializeField] private List<SpriteRenderer> m_Points = new();
    
        public void SwitchToText()
        {
            DOTween.To(() => m_Map.color.a, val =>
            {
                var mapColor = m_Map.color;
                mapColor.a = val;
                m_Map.color = mapColor;
                foreach (var point in m_Points)
                {
                    var pointColor = point.color;
                    pointColor.a = val;
                    point.color = pointColor;
                }
            }, 0f, 1f)
            .OnComplete(() => DOTween.To(() => m_Text.color.a, val => m_Text.color = new Color(1, 1, 1, val), 1f, 0.5f));
        }
    }
}