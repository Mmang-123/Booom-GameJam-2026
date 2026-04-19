using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Mmang.ProceduralAnimation
{
    public static class AnimationRiggingUtil
    {
        public static void SetTarget(this TwoBoneIKConstraint twoBoneIK, Transform target)
        {
            twoBoneIK.data.target = target;
        }

        public static void SetTarget(this ChainIKConstraint chainIK, Transform target)
        {
            chainIK.data.target = target;
        }

        public static void SetTarget(this OverrideTransform overrideTransform, Transform target)
        {
            overrideTransform.data.sourceObject = target;
        }

        public static void SetTarget(this IRigConstraint rigConstraint, Transform target)
        {
            switch (rigConstraint)
            {
                case TwoBoneIKConstraint twoBoneIK:
                    twoBoneIK.SetTarget(target);
                    break;
                case ChainIKConstraint chainIK:
                    chainIK.SetTarget(target);
                    break;
                case OverrideTransform overrideTransform:
                    overrideTransform.SetTarget(target);
                    break;
            }
        }
    }

}