using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class PlayerTrigger : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D m_CircleRange;
        [SerializeField] private float m_DelayCheckTime = 1f;        
        
        // Runtime
        private float m_Timer;
        protected bool m_StopCheck = false;

        protected virtual void FixedUpdate()
        {
            if (m_StopCheck)
                return;
            if (m_Timer < m_DelayCheckTime)
            {
                m_Timer += Time.fixedDeltaTime;
                return;
            }

            Vector2 position = (Vector2)transform.position + m_CircleRange.offset;
            float radius = m_CircleRange.radius;

            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(position, radius, fishList);

            foreach (var fish in fishList)
            {
                if (fish.IsPlayer)
                {
                    Trigger(fish);
                }
            }

            ListPool<Fish>.Release(fishList);
        }

        protected virtual void Trigger(Fish fish) { }
    }
}