using UnityEngine;
using UnityEngine.Playables;

namespace Mmang.Test
{
    public class HitboxBehaviour : PlayableBehaviour
    {
        public string hitboxName;
        public float damageMultiplier;
        public bool isActive = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            isActive = true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            isActive = false;
        }
    }

    [System.Serializable]
    public class HitboxClip : PlayableAsset
    {
        public string hitboxName = "RightHand";
        public float damageMultiplier = 1.0f;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<HitboxBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            // 将数据传递给运行时
            behaviour.hitboxName = hitboxName;
            behaviour.damageMultiplier = damageMultiplier;

            return playable;
        }
    }
}