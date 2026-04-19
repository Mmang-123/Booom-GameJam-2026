
using UnityEngine;

namespace Mmang.Util
{
    public static class LerpUtil
    {
        public static float MoveValueToTarget(float value, float target, float change)
        {
            if (value < target)
            {
                value += change;
                if (value >= target)
                    return target;
            }
            else if (value > target)
            {
                value -= change;
                if (value <= target)
                    return target;
            }
            return value;
        }

        public static Vector3 MoveValueToTarget(Vector3 value, Vector3 target, float change)
        {
            Vector3 dir = (target - value).normalized;
            float distance = Vector3.Distance(value, target);
            distance = MoveValueToTarget(0f, distance, change);
            return value + distance * dir;
        }

        public static Vector2 MoveValueToTarget(Vector2 value, Vector2 target, float change)
        {
            Vector2 dir = (target - value).normalized;
            float distance = Vector3.Distance(value, target);
            distance = MoveValueToTarget(0f, distance, change);
            return value + distance * dir;
        }

        public static bool GTimer(ref float timer, float target, float delta)
        {
            timer += delta;
            return timer >= target;
        }

        public static bool LTimer(ref float timer, float target, float delta)
        {
            timer -= delta;
            return timer <= target;
        }
    }

}