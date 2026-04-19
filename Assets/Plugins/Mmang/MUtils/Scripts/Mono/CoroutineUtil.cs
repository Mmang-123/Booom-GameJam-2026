using System.Collections;
using UnityEngine;

namespace Mmang.Util
{
    public static class CoroutineUtil
    {
        public static IEnumerator WaitSecondsUnscaled(float seconds)
        {
            float timer = 0f;
            while (timer < seconds)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public static IEnumerator WaitSeconds(float seconds)
        {
            float timer = 0f;
            while (timer < seconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}