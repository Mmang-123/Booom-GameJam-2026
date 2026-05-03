using System.Collections.Generic;
using Mmang.Game;
using UnityEngine;

namespace Game
{
    public abstract class FA_Trace : FishAIAbility
    {
        [System.Serializable]
        public struct TargetPriority
        {
            public GameplayTag Tag;
            public int Priority;
        }

        [SerializeField] private List<TargetPriority> m_TargetPriorityList = new();
        public List<TargetPriority> TargetPriorityList => m_TargetPriorityList;


        // Runtime
        protected FB_Swim SwimBehaviour { get; set; }
        protected FB_Eat EatBehaviour { get; set; }
        protected Fish TargetFish { get; set; }
        

        protected abstract bool FindTarget(out Fish outTarget);

        public override bool CanActivateAbility()
        {
            bool flag = FindTarget(out var result);
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

    }
}