using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class Repeller : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D m_CircleRange;
        [SerializeField] private bool m_SetDirection = false;
        [SerializeField] private Vector2 m_Direction;

        private void FixedUpdate()
        {
            Repel();
        }

        private void Repel()
        {
            Vector2 position = (Vector2)transform.position + m_CircleRange.offset;
            float radius = m_CircleRange.radius;

            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(position, radius, fishList);

            foreach (var fish in fishList)
            {
                if (fish.InfectedLevel >= EInfectedLevel.Mid)
                    continue;
                
                if (fish.FishController is FishAIComponent fishAI)
                {
                    var fleeAbility = fishAI.GetAbility<FA_Flee>();
                    if (fleeAbility != null)
                    {
                        fleeAbility.FleeFromPoint(transform.position);
                    }
                }
            }

            ListPool<Fish>.Release(fishList);
        }
    }
}