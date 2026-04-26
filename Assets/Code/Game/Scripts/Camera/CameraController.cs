using System.Collections.Generic;
using System.Linq;
using Mmang.ProceduralAnimation;
using Mmang.Util;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class CameraTarget
    {
        public Transform Trans;
        public float Weight;

        public CameraTarget(Transform trans, float weight = 1f)
        {
            Trans = trans;
            Weight = weight;
        }
    }

    public class CameraController : SingletonMono<CameraController>
    {
        [SerializeField] private List<CameraTarget> m_Targets = new();

        [Header("跟随设置")]
        [SerializeField] private SecondOrderDynamicsSetting m_FollowSetting;

        // Runtime
        public Vector2 TargetPoint { get; private set; }
        private Vector2SecondOrderDynamics m_FollowDamper;

        protected override void OnAwake()
        {
            TargetPoint = transform.position;
            m_FollowDamper = new(m_FollowSetting, TargetPoint);
        }

        private void Update()
        {
            ComputeTargetPoint();
            FollowTargetPoint();
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

        private void FollowTargetPoint()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }
            m_FollowDamper.UpdateAttribute(m_FollowSetting);
            Vector2 finalPos = m_FollowDamper.Update(Time.deltaTime, TargetPoint);
            transform.position = new(finalPos.x, finalPos.y, transform.position.z);
        }

        #endregion

        #region 设置追踪点

        public bool ContainsFollowPoint(Transform trans)
        {
            return m_Targets.Any(i => i.Trans == trans);
        }

        public void AddFollowPoint(Transform trans, float weight = 1f)
        {
            m_Targets.Add(new(trans, weight));
        }

        public void RemoveFollowPoint(Transform trans)
        {
            m_Targets.RemoveAll(t => t.Trans == trans);
        }

        public void SetWeight(Transform trans, float newWeight)
        {
            // todo: 后面改成map
            var pair = m_Targets.Find(i => i.Trans == trans);
            pair.Weight = newWeight;
        }

        #endregion
        
    }
}