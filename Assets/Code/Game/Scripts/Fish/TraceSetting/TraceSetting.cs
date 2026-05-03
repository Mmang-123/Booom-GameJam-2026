using System.Collections.Generic;
using Mmang.Game;
using UnityEngine;

namespace Game
{
    public abstract class TraceSetting : MonoBehaviour
    {
        [System.Serializable]
        public struct TargetPriority
        {
            public GameplayTag Tag;
            public int Priority;
        }

        [SerializeField] private List<TargetPriority> m_TargetPriorityList = new();
        public List<TargetPriority> TargetPriorityList => m_TargetPriorityList;

        public abstract bool FindTarget(FishAIComponent fishAI, out Fish outTarget);
        public abstract bool CanTrace(FishAIComponent fishAI, Fish target);
    }
}