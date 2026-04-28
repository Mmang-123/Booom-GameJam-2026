using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PowerChain : ChargableMono, IPowerSource
    {
        [System.Serializable]
        public struct ChainPoint
        {
            public SpriteRenderer Renderer1;
            public SpriteRenderer Renderer2;
        }

        [SerializeField] private ChargableMono m_ChargeObject;
        [SerializeField] private float m_ChargeInterval = 0.3f;
        [SerializeField] private float m_ReduceInterval = 0.3f;

        [Header("Sprites")]
        [SerializeField] private Sprite m_BigPoint_Sprite_On;
        [SerializeField] private Sprite m_BigPoint_Sprite_Off;
        [SerializeField] private Sprite m_SmallPoint_Sprite_On;
        [SerializeField] private Sprite m_SmallPoint_Sprite_Off;

        [SerializeField] private List<ChainPoint> m_Points = new();

        // Runtime
        private int m_PowerSourceCount = 0;
        private float m_ChargeTimer;
        private float m_ReduceTimer;

        private int m_CurrentPowerPoint;
        private bool m_Active;

        public int MaxPowerPointCount => m_Points.Count;
        public bool IsOn => m_PowerSourceCount > 0;

        public override void StartCharge(IPowerSource powerSource)
        {
            m_PowerSourceCount++;
        }

        public override void StopCharge(IPowerSource powerSource)
        {
            m_PowerSourceCount--;   
        }

        private void Update()
        {
            if (IsOn)
            {
                m_ChargeTimer += Time.deltaTime;
                m_ReduceTimer = 0f;

                if (m_ChargeTimer >= m_ChargeInterval)
                {
                    m_ChargeTimer -= m_ChargeInterval;
                    ChargePowerPoint();
                }       
            }
            else
            {
                m_ChargeTimer = 0f;
                m_ReduceTimer += Time.deltaTime;

                if (m_ReduceTimer >= m_ReduceInterval)
                {
                    m_ReduceTimer -= m_ReduceInterval;
                    ReducePowerPoint();
                }
            }
        }

        private void ChargePowerPoint()
        {
            if (m_CurrentPowerPoint == MaxPowerPointCount)
                return;

            SetPointSprite(m_CurrentPowerPoint, true);

            if (m_CurrentPowerPoint == MaxPowerPointCount - 1)
            {
                m_CurrentPowerPoint = MaxPowerPointCount;
                SetActive(true);
                return;
            }
            m_CurrentPowerPoint++;
        }

        private void ReducePowerPoint()
        {
            if (m_CurrentPowerPoint == 0)
                return;

            SetPointSprite(m_CurrentPowerPoint - 1, false);

            if (m_CurrentPowerPoint == MaxPowerPointCount)
            {
                SetActive(false);
            }

            if (m_CurrentPowerPoint == 1)
            {
                m_CurrentPowerPoint = 0;
                return;
            }
            m_CurrentPowerPoint--;   
        }

        private void SetPointSprite(int index, bool active)
        {
            var color = active ? Color.green : Color.white;
            var point = m_Points[index];
            if (point.Renderer1 != null)
            {
                point.Renderer1.color = color;
                point.Renderer1.sprite = active ? m_BigPoint_Sprite_On : m_BigPoint_Sprite_Off;
            }
            if (point.Renderer2 != null)
            {
                point.Renderer2.color = color;
                point.Renderer2.sprite = active ? m_SmallPoint_Sprite_On : m_SmallPoint_Sprite_Off;
            }
        }

        #region 充能其他物体..
        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;
            
            m_Active = active;
            OnActiveChanged();
        }

        private void OnActiveChanged()
        {
            if (m_ChargeObject != null)
            {
                if (m_Active)
                {
                    m_ChargeObject.StartCharge(this);
                }
                else
                {
                    m_ChargeObject.StopCharge(this);
                }   
            }
        }

        #endregion
    }
}