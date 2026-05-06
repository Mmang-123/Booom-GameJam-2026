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
        [SerializeField] private Color m_ActiveColor = Color.green;

        [SerializeField] private InterfaceObject<IPowerSource> m_PowerSource;
        [SerializeField] private InterfaceObject<IChargable> m_ChargeObject;
        [SerializeField] private float m_ConductionTime = 1f;
        [SerializeField] private float m_MaintainTime = 1f;

        [Header("Sprites")]
        [SerializeField] private SpriteRenderer m_PointPrefab_Big;
        [SerializeField] private SpriteRenderer m_PointPrefab_Small;
        [SerializeField] private Sprite m_BigPoint_Sprite_On;
        [SerializeField] private Sprite m_BigPoint_Sprite_Off;
        [SerializeField] private Sprite m_SmallPoint_Sprite_On;
        [SerializeField] private Sprite m_SmallPoint_Sprite_Off;

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
        public event System.Action<bool> OnPowerChanged;

        #endregion

        // Runtime
        private bool m_Inited = false;
        private List<PointData> m_PointDataList;
        private float m_PointConductionTime;
        private float m_PointMaintainTime;

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

            if (m_PowerSource.Value != null)
            {
                m_PowerSource.Value.InitPowerSource();
                if (m_PowerSource.Value.PowerOn)
                {
                    ChargeAllPoint();
                    m_ChargeObject.Value?.SetChargeComplete(true);   
                }
            }
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

            HashSetPool<int>.Release(toTurnOff);
            HashSetPool<int>.Release(toTurnOn);
            HashSetPool<int>.Release(changed);
        }

        public void SetChargeComplete(bool init) => ChargeAllPoint();
        public void ChargeAllPoint()
        {
            for (int i = 0; i < MaxPowerPointCount; i++)
            {
                SetPointActive(i, true);
            }
            SetPowerOn(true);
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
            OnPowerChanged?.Invoke(on);
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