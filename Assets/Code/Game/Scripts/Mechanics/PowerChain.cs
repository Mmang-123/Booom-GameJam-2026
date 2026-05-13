using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEditor;
using Mmang.Util;

namespace Game
{
    [System.Serializable]
    public struct BezierControlPoint
    {
        public Vector2 Position1;
        public Vector2 Position2;
        public BezierControlPoint(Vector2 p1, Vector2 p2)
        {
            Position1 = p1;
            Position2 = p2;
        }

        public Vector2 GetNegativePoint2()
        {
            Vector2 offset = Position2 - Position1;
            return Position1 - offset;
        }

        public void SetPosition1(Vector2 pos) => Position1 = pos;
        public void SetPosition2(Vector2 pos) => Position2 = pos;
    }

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

        [System.Serializable]
        public class EnergyPulse : IReference
        {
            public float HeadIndex; // 脉冲头部的位置 (0 ~ MaxPowerPointCount)
            public float TailIndex; // 脉冲尾部的位置
            public bool IsReceivingPower; // 是否正在持续接收能量
        
            public void Clear()
            {
                HeadIndex = 0;
                TailIndex = 0;
                IsReceivingPower = false;
            }
        }

        [SerializeField] private Color m_ActiveColor = Color.green;

        [SerializeField] private InterfaceObject<IPowerSource> m_PowerSource;
        [SerializeField] private InterfaceObject<IChargable> m_ChargeObject;
        [SerializeField] private int m_ChargeSlot = 0;
        [SerializeField] private float m_ConductionTime = 1f;
        [SerializeField] private float m_MaintainTime = 1f;
        [SerializeField] private bool m_FreezeWhenInvalid = false;
        [SerializeField] private List<EnergyPulse> m_InitPulse = new();

        [Header("Sprites")]
        [SerializeField] private SpriteRenderer m_PointPrefab_Big;
        [SerializeField] private SpriteRenderer m_PointPrefab_Small;
        [SerializeField] private Sprite m_BigPoint_Sprite_On;
        [SerializeField] private Sprite m_BigPoint_Sprite_Off;
        [SerializeField] private Sprite m_SmallPoint_Sprite_On;
        [SerializeField] private Sprite m_SmallPoint_Sprite_Off;

        [Header("传导设置")]
        [SerializeField] private bool m_DynamicSpeed = false;
        private const float PointsPerUnit = 16f;

        [Header("生成设置")]
        [SerializeField] private float m_PointDistance = 0.8f;
        [SerializeField] private List<BezierControlPoint> m_ControlPoints = new()
        {
            new(new(-1, 0), new(-1, 1)), new(new(1, 0), new(1, 1))
        };

        [Header("节点引用")]
        [SerializeField] private List<ChainPoint> m_Points = new();

        public List<BezierControlPoint> ControlPoints => m_ControlPoints;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered(1);

        #endregion

        #region IPowerSource
        public bool PowerOn { get; private set; }
        public event System.Action<IPowerSource, bool> OnPowerChanged;
        public bool PowerValid => !m_FreezeWhenInvalid || PowerSourceHandler.IsValid();
        #endregion

        // Runtime
        private bool m_Inited = false;
        private List<EnergyPulse> m_Pulses; // 当前链条上移动的所有脉冲段
        private bool[] m_PointStates;       // 记录每个点的当前激活状态，避免重复赋值
        public int MaxPowerPointCount => m_Points.Count;


        private void Start()
        {
            Init();
        }

        public void InitPowerSource()
            => Init();

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            m_Pulses = new List<EnergyPulse>();
            m_PointStates = new bool[MaxPowerPointCount];

            //
            if (m_PowerSource.Value != null)
                PowerSourceHandler.AddPowerSource(m_PowerSource.Value);
            if (m_ChargeObject.Value != null)
                m_ChargeObject.Value.PowerSourceHandler.AddPowerSource(this, m_ChargeSlot);

            bool chargedAll = false;
            if (m_PowerSource.Value != null)
            {
                m_PowerSource.Value.InitPowerSource();
                if (m_PowerSource.Value.PowerOn)
                {
                    ChargeAllPoint();
                    chargedAll = true;
                    m_ChargeObject.Value?.SetChargeComplete(true);   
                }
            }

            if (!chargedAll)
            {
                foreach (var initPulse in m_InitPulse)
                {
                    var newPulse = ReferencePool.Acquire<EnergyPulse>();
                    newPulse.HeadIndex = initPulse.HeadIndex;
                    newPulse.TailIndex = initPulse.TailIndex;
                    newPulse.IsReceivingPower = false;
                    m_Pulses.Add(newPulse);
                }
            }
        }

        private void FixedUpdate()
        {
            if (MaxPowerPointCount == 0) return;
            if (!PowerSourceHandler.IsValid())
                return;

            float dt = Time.fixedDeltaTime;
            
            // 计算脉冲头部和尾部的移动速度 (单位：个节点/秒)
            // m_DynamicSpeed=true 时速度固定（按实际距离），链越长耗时越长；否则时间固定
            float baseCount = m_DynamicSpeed ? PointsPerUnit : MaxPowerPointCount;
            float speed = m_ConductionTime > 0f ? baseCount / m_ConductionTime : 9999f;
            float tailSpeed = m_MaintainTime > 0f ? baseCount / m_MaintainTime : speed;

            bool isCurrentlyPowered = IsPowered;

            // 1. 处理能量源状态，生成或断开脉冲
            if (isCurrentlyPowered)
            {
                // 如果当前没有脉冲，或者最后一个脉冲已经断开了连接，则生成一个新的脉冲段
                if (m_Pulses.Count == 0 || !m_Pulses[^1].IsReceivingPower)
                {
                    var newPulse = ReferencePool.Acquire<EnergyPulse>();
                    newPulse.HeadIndex = 0; newPulse.TailIndex = 0; newPulse.IsReceivingPower = true;
                    m_Pulses.Add(newPulse);
                }
            }
            else
            {
                // 如果断电了，让最后一个接收能量的脉冲段断开连接（尾部开始收缩移动）
                if (m_Pulses.Count > 0 && m_Pulses[^1].IsReceivingPower)
                {
                    var pulse = m_Pulses[^1];
                    pulse.IsReceivingPower = false;
                    float length = Mathf.Floor(pulse.HeadIndex - pulse.TailIndex);
                    pulse.TailIndex = pulse.HeadIndex - length;
                }
            }

            // 2. 更新所有脉冲段的位置
            for (int i = m_Pulses.Count - 1; i >= 0; i--)
            {
                var pulse = m_Pulses[i];
                
                // 头部始终向前推进
                pulse.HeadIndex += speed * dt;

                // 只有断开能量源的脉冲，尾部才会向前收缩
                if (!pulse.IsReceivingPower)
                {
                    pulse.TailIndex += tailSpeed * dt;
                }

                // 限制头部不超过最大节点数
                if (pulse.HeadIndex > MaxPowerPointCount)
                {
                    pulse.HeadIndex = MaxPowerPointCount;
                }

                // 如果脉冲完全离开了链条，或者首尾闭合（能量耗尽），则移除该脉冲
                if (pulse.TailIndex >= MaxPowerPointCount || pulse.TailIndex >= pulse.HeadIndex)
                {
                    ReferencePool.Release(m_Pulses[i]);
                    m_Pulses.RemoveAt(i);
                }
            }

            // 3. 将脉冲映射到具体的渲染节点上
            int currentActiveCount = 0;
            for (int i = 0; i < MaxPowerPointCount; i++)
            {
                bool shouldBeActive = false;
                
                // 判断当前节点是否位于任意一个脉冲段内部
                foreach (var pulse in m_Pulses)
                {
                    if (pulse.HeadIndex >= i && pulse.TailIndex < i + 1)
                    {
                        shouldBeActive = true;
                        break;
                    }
                }

                // 状态发生改变时更新表现
                if (m_PointStates[i] != shouldBeActive)
                {
                    m_PointStates[i] = shouldBeActive;
                    SetPointSprite(i, shouldBeActive);
                }

                if (shouldBeActive) 
                    currentActiveCount++;
            }

            // 4. 更新对下游物体的供电状态 (如果链条最后一个点是激活的，就传导能量)
            SetPowerOn(m_PointStates[MaxPowerPointCount - 1]);
        }

        public void SetChargeComplete(bool init) => ChargeAllPoint();
        public void ChargeAllPoint()
        {
            if (MaxPowerPointCount == 0) return;

            m_Pulses.Clear();
            
            // 直接生成一段覆盖全链条的脉冲
            var newPulse = ReferencePool.Acquire<EnergyPulse>();
            newPulse.HeadIndex = MaxPowerPointCount; newPulse.TailIndex = 0; newPulse.IsReceivingPower = true;
            m_Pulses.Add(newPulse);

            for (int i = 0; i < MaxPowerPointCount; i++)
            {
                m_PointStates[i] = true;
                SetPointSprite(i, true);
            }
            SetPowerOn(true);
        }

        private void SetPointSprite(int index, bool active)
        {
            var color = active ? m_ActiveColor : Color.white;
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
            OnPowerChanged?.Invoke(this, on);
        }
        #endregion



        #region 工具
#if UNITY_EDITOR

        private SpriteRenderer Editor_CreatePointInstance(bool isBig)
        {
            var prefab = isBig ? m_PointPrefab_Big : m_PointPrefab_Small;
            var instance = Object.Instantiate<SpriteRenderer>(prefab);
            instance.transform.SetParent(transform, false);
            return instance;
        }

        public void Editor_ReverseControlPoints()
        {
            int count = m_ControlPoints.Count;
            for (int i = 0; i < count / 2; i++)
            {
                int j = count - i - 1;
                (m_ControlPoints[j], m_ControlPoints[i]) = (m_ControlPoints[i], m_ControlPoints[j]);
            }
            Editor_GeneratePoints();
        }

        public void Editor_ClearPoints()
        {
            foreach (var point in m_Points)
            {
                if (point.Renderer1 != null)
                    Object.DestroyImmediate(point.Renderer1.gameObject);
                if (point.Renderer2 != null)
                    Object.DestroyImmediate(point.Renderer2.gameObject);
            }
            m_Points.Clear();
        }

        public void Editor_GeneratePoints()
        {
            Vector2 P(Vector2 localPosition)
            {
                return (Vector2)transform.position + localPosition;
            }

            Editor_ClearPoints();
            var path = new BezierPath();

            float step = m_PointDistance;

            for (int i = 0; i < ControlPoints.Count - 1; i++)
            {
                var point1 = ControlPoints[i];
                var point2 = ControlPoints[i + 1];

                if (i > 0)
                {
                    point1.Position2 = point1.GetNegativePoint2();
                }

                path.Initialize(P(point1.Position1), P(point1.Position2), P(point2.Position2), P(point2.Position1));

                int pointCount = Mathf.FloorToInt(path.TotalLength / step) + 1;
                if (pointCount % 2 == 0)
                    pointCount++;
                
                float newStep = path.TotalLength / (pointCount - 1);

                SpriteRenderer preSmall = null;
                bool isBig = true;
                for (int j = 0; j < pointCount; j++)
                {
                    float currentLength = j * newStep;
                    Vector2 position = path.GetPointAtDistance(currentLength);
                
                    var instance = Editor_CreatePointInstance(isBig);
                    instance.transform.position = position;

                    if (isBig)
                    {
                        ChainPoint chainPoint = new()
                        {
                            Renderer1 = instance,
                            Renderer2 = preSmall
                        };
                        m_Points.Add(chainPoint);
                    }

                    if (!isBig)
                    {
                        preSmall = instance;
                    }

                    isBig = !isBig;
                }

            }
            EditorUtility.SetDirty(this);
        }

        private void OnDrawGizmos()
        {
            if (ControlPoints.Count > 0)
            {
                var rawColor= Gizmos.color;
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube((Vector3)ControlPoints[0].Position1 + transform.position, Vector3.one * 0.08f);
                Gizmos.color = rawColor;
            }
        }

#endif
        #endregion
    }
}