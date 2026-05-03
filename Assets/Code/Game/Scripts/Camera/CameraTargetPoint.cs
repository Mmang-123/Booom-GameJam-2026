using Mmang;
using UnityEngine;

namespace Game
{
    public class CameraTargetPoint : MonoBehaviour
    {
        [SerializeField] private Vector2 m_MixRange = new(0.8f, 0.9f);
        [SerializeField] private float m_Weight = 1f;

        private void FixedUpdate()
        {
            var manager = CameraController.Instance;

            if (manager.MainTarget == transform)
                return;

            float weight = manager.GetMixWeight(transform.position, m_MixRange);
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