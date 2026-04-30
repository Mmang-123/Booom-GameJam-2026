using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine.Pool;
using UnityEngine;

namespace Game
{
    public class PowerChain : MonoBehaviour, IChargable, IPowerSource
    {
        [System.Serializable]
        public struct ChainPoint
        {
            public SpriteRenderer Renderer1;
            public SpriteRenderer Renderer2;
        }

        public class PointData
        {
            public bool Active;
            public float ConductionTimer;
            public float OffTimer;
        }

        [SerializeField] private InterfaceObject<IPowerSource> m_PowerSource;
        [SerializeField] private InterfaceObject<IChargable> m_ChargeObject;
        [SerializeField] private float m_ConductionTime = 1f;
        [SerializeField] private float m_MaintainTime = 1f;

        [Header("Sprites")]
        [SerializeField] private Sprite m_BigPoint_Sprite_On;
        [SerializeField] private Sprite m_BigPoint_Sprite_Off;
        [SerializeField] private Sprite m_SmallPoint_Sprite_On;
        [SerializeField] private Sprite m_SmallPoint_Sprite_Off;

        [SerializeField] private List<ChainPoint> m_Points = new();

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered(1);

        #endregion

        #region IPowerSource
        public bool PowerOn { get; private set; }
        public event System.Action<bool> OnPowerChanged;

        #endregion


        // Runtime
        private List<PointData> m_PointDataList;
        private float m_PointConductionTime;
        private float m_PointMaintainTime;

        public int MaxPowerPointCount => m_Points.Count;


        private void Start()
        {
            m_PointDataList = new(m_Points.Count);
            for (int i = MaxPowerPointCount - 1; i >= 0; i--)
            {
                var newData = new PointData();
                m_PointDataList.Add(newData);
            }

            //
            m_PointConductionTime = m_ConductionTime / MaxPowerPointCount;
            m_PointMaintainTime = m_MaintainTime / MaxPowerPointCount;
        
            //
            if (m_PowerSource.Value != null)
                PowerSourceHandler.AddPowerSource(m_PowerSource.Value);
            if (m_ChargeObject.Value != null)
                m_ChargeObject.Value.PowerSourceHandler.AddPowerSource(this);
        }

        private void FixedUpdate()
        {
            HashSet<int> toTurnOff = HashSetPool<int>.Get();
            HashSet<int> toTurnOn = HashSetPool<int>.Get();
            HashSet<int> changed = HashSetPool<int>.Get();
            
            float dt = Time.fixedDeltaTime;

            if (IsPowered && MaxPowerPointCount > 0)
            {
                toTurnOn.Add(0);
                changed.Add(0);
            }

            for (int i = 0; i < MaxPowerPointCount; i++)
            {
                var data = m_PointDataList[i];
                if (!data.Active)
                    continue;

                data.OffTimer += dt;
                data.ConductionTimer += dt;

                if (data.ConductionTimer >= m_PointConductionTime && i < MaxPowerPointCount - 1)
                {
                    toTurnOn.Add(i + 1);
                    changed.Add(i + 1);
                    data.ConductionTimer = 0f;
                }
                if (data.OffTimer >= m_PointMaintainTime)
                {
                    toTurnOff.Add(i);
                    changed.Add(i);
                }
            }

            foreach (var index in changed)
            {
                if (toTurnOn.Contains(index))
                {
                    SetPointActive(index, true);
                    continue;
                }
                if (toTurnOff.Contains(index))
                {
                    SetPointActive(index, false);
                }
            }

            if (MaxPowerPointCount > 0)
            {
                SetPowerOn(m_PointDataList[MaxPowerPointCount - 1].Active);
            }
        }

        private void SetPointActive(int index, bool active)
        {
            var data = m_PointDataList[index];
            data.Active = active;
            if (active)
            {
                data.OffTimer = 0f;
            }
            else
            {
                data.ConductionTimer = 0f;
            }

            //
            SetPointSprite(index, active);
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

        #region 
        private void SetPowerOn(bool on)
        {
            if (PowerOn == on)
                return;
            
            PowerOn = on;
            OnPowerChanged?.Invoke(on);
        }
        #endregion
    }
}