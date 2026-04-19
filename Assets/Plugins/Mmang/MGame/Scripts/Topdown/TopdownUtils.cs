using Mmang.Util;
using UnityEngine;

namespace Mmang.Topdown
{
    public static class TopdownUtils
    {
        public static Vector3 GetTopDownMoveDirection(Vector2 inputDirection, Quaternion faceRotation)
        {
            return TransformUtil.DirectionLocalToWorldOnPlane(inputDirection, faceRotation);
        }

    }
}