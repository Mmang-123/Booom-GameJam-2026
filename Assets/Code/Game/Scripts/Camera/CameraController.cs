using System.Collections.Generic;
using System.Linq;
using Mmang.PixelartRender;
using Mmang.ProceduralAnimation;
using Mmang.Util;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class CameraTarget : IReference
    {
        public Transform Trans;
        public float Weight;

        public CameraTarget(Transform trans, float weight = 1f)
        {
            Trans = trans;
            Weight = weight;
        }
        public CameraTarget() { }

        public void Clear()
        {
            Trans = null;
            Weight = 0f;
        }
    }

    public class CameraController : SingletonMono<CameraController>
    {
        [Header("跟随设置")]
        [SerializeField] private SecondOrderDynamicsSetting m_FollowSetting;
        [SerializeField] private Vector2 m_CameraViewSize = new(30f, 16.875f);

        // Runtime
        private Transform m_MainTarget;
        private List<CameraTarget> m_Targets = new();
        private Dictionary<Transform, CameraTarget> m_TargetMap = new();

        public Vector2 TargetPoint { get; private set; }
        private Vector2SecondOrderDynamics m_FollowDamper;

        public Transform MainTarget => m_MainTarget;

        private bool m_FixedUpdateThisFrame = false;
        private Vector2 m_Offset;
        private float m_FDT;

        protected override void OnAwake()
        {
            TargetPoint = transform.position;
            m_FollowDamper = new(m_FollowSetting, TargetPoint);
        }

        private void Update()
        {
            if (m_FixedUpdateThisFrame)
            {
                ComputeTargetPoint();
                FollowTargetPoint(m_FDT);
            }
            //ComputeTargetPoint();
            //FollowTargetPoint(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            m_FixedUpdateThisFrame = true;
            m_FDT = Time.fixedDeltaTime;
            //ComputeTargetPoint();
            //FollowTargetPoint(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            /*
            if (m_FixedUpdateThisFrame)
            {
                if (m_Offset != Vector2.zero)
                {
                    m_FollowDamper.Offset(m_Offset);
                    m_Offset = Vector2.zero;   
                }
            }
            */
            m_Test = false;
            m_FixedUpdateThisFrame = false;
            ObstacleMaskManager.Instance.UpdatePosition(transform.position);
        }

        public float GetMixWeight(Vector2 position, Vector2 mixRange)
        {
            Transform trans = m_MainTarget == null ? transform : m_MainTarget;

            float xDis = Mathf.Abs(position.x - trans.position.x);
            float yDis = Mathf.Abs(position.y - trans.position.y);

            xDis = Mathf.Clamp01(xDis / m_CameraViewSize.x * 2f);
            yDis = Mathf.Clamp01(yDis / m_CameraViewSize.y * 2f);

            float dis = Mathf.Max(xDis, yDis);
            mixRange.x = Mathf.Min(mixRange.x, mixRange.y);

            if (mixRange.x == mixRange.y)
            {
                return dis <= mixRange.x ? 1f : 0f;
            }

            return 1.0f - (Mathf.Clamp(dis, mixRange.x, mixRange.y) - mixRange.x) / (mixRange.y - mixRange.x);
        }

        bool m_Test;
        public void TransferOffset(Vector2 offset)
        {
            m_Test = true;
            //m_Offset += offset;
            m_FollowDamper.Offset(offset);
            //transform.position += (Vector3)offset;
        }

        #region 追踪计算
        private void ComputeTargetPoint()
        {
            Vector2 targetPoint = Vector2.zero;
            float totalWeight = 0f;

            foreach (var target in m_Targets)
            {
                if (target.Trans == null)
                    continue;
                totalWeight += target.Weight;
            }

            foreach (var target in m_Targets)
            {
                if (target.Trans == null || target.Weight == 0f)
                    continue;
                
                float t = target.Weight / totalWeight;
                targetPoint += (Vector2)target.Trans.position * t;
            }

            TargetPoint = targetPoint;
        }

        private void FollowTargetPoint(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            m_FollowDamper.UpdateAttribute(m_FollowSetting);
            Vector2 finalPos = m_FollowDamper.Update(dt, TargetPoint);
            transform.position = new(finalPos.x, finalPos.y, transform.position.z);
        }

        #endregion

        #region 设置追踪点

        public void SetMainTarget(Transform transform)
        {
            m_MainTarget = transform;
            if (transform != null)
                AddFollowPoint(transform, 1f);
        }

        public bool ContainsFollowPoint(Transform trans)
        {
            return m_TargetMap.ContainsKey(trans);
        }

        public void AddFollowPoint(Transform trans, float weight = 1f)
        {
            if (ContainsFollowPoint(trans))
            {
                m_TargetMap[trans].Weight = weight;
            }
            else
            {
                var instance = ReferencePool.Acquire<CameraTarget>();
                instance.Trans = trans;
                instance.Weight = weight;
                m_Targets.Add(instance);
                m_TargetMap.Add(trans, instance);
            }
        }

        public void RemoveFollowPoint(Transform trans)
        {
            if (m_TargetMap.TryGetValue(trans, out var point))
            {
                m_TargetMap.Remove(trans);
                m_Targets.Remove(point);
                ReferencePool.Release(point);
            }

            if (trans == m_MainTarget)
            {
                m_MainTarget = null;
            }
        }

        public void SetWeight(Transform trans, float newWeight)
        {
            if (m_TargetMap.TryGetValue(trans, out var point))
            {
                point.Weight = newWeight;
            }
        }

        #endregion
        
    }
}