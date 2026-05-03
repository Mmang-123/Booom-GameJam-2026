using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class CircleTraceSetting : TraceSetting
    {
        [SerializeField] private float m_StartRadius = 3f;
        [SerializeField] private float m_StopRadius = 7f;

        public override bool FindTarget(FishAIComponent fishAI, out Fish outTarget)
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(fishAI.Fish.Position, m_StartRadius, fishList, ignoreFish: fishAI.Fish, clearResultList: true);

            bool flag = false;
            outTarget = null;
            foreach (var fish in fishList)
            {
                flag = true;
                outTarget = fish;
                break;
            }

            ListPool<Fish>.Release(fishList);
            return flag;
        }

        public override bool CanTrace(FishAIComponent fishAI, Fish target)
        {
            if (target == null)
                return false;
            float distance = Vector2.Distance(fishAI.Fish.Position, target.Position);
            return distance <= m_StopRadius;
        }

        /*
        public override bool StopTrace(FishAIComponent fishAI)
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(fishAI.Fish.Position, m_StopRadius, fishList);

            bool flag = false;
            foreach (var fish in fishList)
            {
                if (fish == fishAI.Fish)
                    continue;
                
                flag = true;
                break;
            }

            ListPool<Fish>.Release(fishList);
            return !flag;
        }
        */
    }
}