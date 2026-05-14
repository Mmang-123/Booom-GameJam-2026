using Mmang;
using UnityEngine;

namespace Game
{
    public class CameraTargetPoint : MonoBehaviour
    {
        public enum ERangeType { Camera, World }
        [SerializeField] private ERangeType m_MixRangeType = ERangeType.Camera;
        [Header("相对相机大小的混合范围, x~y表示混合的范围")]
        [SerializeField] private Vector2 m_MixRange = new(0.8f, 0.9f);
        [Header("世界空间大小的混合")]
        [SerializeField] private Vector2 m_WorldMixRangeX = new(0f, 0f);
        [SerializeField] private Vector2 m_WorldMixRangeY = new(0f, 0f);
        [SerializeField] private float m_Weight = 1f;

        private void FixedUpdate()
        {
            var manager = CameraController.Instance;

            if (manager.MainTarget == transform)
                return;

            float weight;
            if (m_MixRangeType == ERangeType.Camera)
            {
                weight = manager.GetMixWeight(transform.position, m_MixRange);
            }
            else
            {
                var trans = manager.GetMixCenter();
                float xDis = Mathf.Abs(transform.position.x - trans.position.x);
                float yDis = Mathf.Abs(transform.position.y - trans.position.y);
                
                
                float xLerpT, yLerpT;
                if (m_WorldMixRangeX.x == m_WorldMixRangeX.y)
                    xLerpT = xDis <= m_WorldMixRangeX.x ? 0f : 1f;
                else
                    xLerpT = Mathf.Clamp01(Mathf.Max(0f, xDis - m_WorldMixRangeX.x * 0.5f) / ((m_WorldMixRangeX.y - m_WorldMixRangeX.x) * 0.5f));
                
                if (m_WorldMixRangeY.x == m_WorldMixRangeY.y)
                    yLerpT = yDis <= m_WorldMixRangeY.x ? 0f : 1f;
                else
                    yLerpT = Mathf.Clamp01(Mathf.Max(0f, yDis - m_WorldMixRangeY.x * 0.5f) / ((m_WorldMixRangeY.y - m_WorldMixRangeY.x) * 0.5f));
                float t = Mathf.Max(xLerpT, yLerpT);

                //Debug.Log($"Dis: {yDis}  Letp: {yLerpT}  T: {t}");
                weight = 1.0f - t;
            }

            weight = weight * weight * m_Weight;
            
            if (weight > 0f)
            {
                manager.AddFollowPoint(transform, weight);
            }
            else
            {
                manager.RemoveFollowPoint(transform);
            }
        }
        
        private void OnDisable()
        {
            if (CameraController.InstanceValid)
            {
                var manager = CameraController.Instance;
                manager.RemoveFollowPoint(transform);   
            }
        }
    }
}