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
            public string Tag;
            public int Priority;
        }

        [SerializeField] private List<TargetPriority> m_TargetPriorityList = new();
        public Dictionary<GameplayTag, int> TargetPriorityMap { get; } = new();

        // Runtime
        protected FB_Swim SwimBehaviour { get; set; }
        protected FB_Eat EatBehaviour { get; set; }
        protected Fish TargetFish { get; set; }

        private bool m_Inited = false;

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            TargetPriorityMap.Clear();
            foreach (var pair in m_TargetPriorityList)
            {
                var gameplayTag = GameplayTag.CreateByName(pair.Tag);
                if (!TargetPriorityMap.ContainsKey(gameplayTag))
                    TargetPriorityMap.Add(gameplayTag, pair.Priority);
            }
        }

        protected abstract bool FindTarget(out Fish outTarget);

        public override bool CanActivateAbility()
        {
            Init();
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

        /// <summary>
        /// 保证传入的都在Map中
        /// </summary>
        /// <param name="fishes"></param>
        /// <returns></returns>
        public void SortPriorityTarget(List<Fish> fishes)
        {
            fishes.Sort((a, b) =>
            {
                int p1 = TargetPriorityMap[a.FishTypeTag];
                int p2 = TargetPriorityMap[b.FishTypeTag];

                if (p1 == p2)
                {
                    if (a.IsPlayer)
                        return -1;
                    else if (b.IsPlayer)
                        return 1;
                    return 0;
                }

                return p2.CompareTo(p1); // 反过来比较，优先返回大的
            });


        }

    }
}