using System.Collections.Generic;
using Mmang.Game;
using UnityEngine;

namespace Game
{
    public class FA_Trace : FishAIAbility
    {
        [SerializeField] private TraceSetting m_TraceSetting;

        // Runtime
        private FB_Swim SwimBehaviour { get; set; }
        private FB_Eat EatBehaviour { get; set; }
        private Fish TargetFish { get; set; }

        public override bool CanActivateAbility()
        {
            bool flag = m_TraceSetting.FindTarget(FishAI, out var result);
            if (flag)
                TargetFish = result;
            return flag;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            Debug.Log("Trace_Start");

            SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
            SwimBehaviour.Tracing = true;

            EatBehaviour = Fish.GetBehaviour<FB_Eat>();
            if (EatBehaviour != null)
                EatBehaviour.Target = TargetFish;
        }

        public override void OnEnd(EEndAbilityType endType)
        {
            base.OnEnd(endType);
            Debug.Log("Trace_End");

            SwimBehaviour.Tracing = false;
        }

        public override void OnUpdate(float dt)
        {
            if (TargetFish != null)
                SwimBehaviour.TargetPoint = TargetFish.Position;

            if (!m_TraceSetting.CanTrace(FishAI, TargetFish))
            {
                FishAI.PendingEndAbility(this, EEndAbilityType.End);
            }
        }

    }
}