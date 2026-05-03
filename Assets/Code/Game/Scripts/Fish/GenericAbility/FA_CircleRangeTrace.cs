using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FA_CircleRangeTrace : FA_Trace
    {
        [SerializeField] private float m_StartRadius = 3f;
        [SerializeField] private float m_StopRadius = 7f;

        protected override bool FindTarget(out Fish outTarget)
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(Fish.Position, m_StartRadius, fishList, ignoreFish: Fish, clearResultList: true);

            bool flag = false;
            outTarget = null;
            if (fishList.Count > 0)
            {
                flag = true;
                outTarget = fishList[0];
            }
            ListPool<Fish>.Release(fishList);
            return flag;
        }

        public bool CanTrace()
        {
            if (TargetFish == null)
                return false;
            float distance = Vector2.Distance(Fish.Position, TargetFish.Position);
            return distance <= m_StopRadius;
        }

        public override void OnUpdate(float dt)
        {
            if (TargetFish != null)
                SwimBehaviour.TargetPoint = TargetFish.Position;

            if (!CanTrace())
            {
                FishAI.PendingEndAbility(this, EEndAbilityType.End);
            }
        }
    }

}