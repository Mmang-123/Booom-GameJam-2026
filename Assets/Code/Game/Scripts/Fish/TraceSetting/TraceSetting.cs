using UnityEngine;

namespace Game
{
    public abstract class TraceSetting : MonoBehaviour
    {
        public abstract bool FindTarget(FishAIComponent fishAI, out Fish outTarget);
        public abstract bool CanTrace(FishAIComponent fishAI, Fish target);
        //public abstract bool StopTrace(FishAIComponent fishAI);
    }
}