using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Util
{
    public static class MonoUtil
    {
        static WaitForEndOfFrame m_WaitForEndOfFrame = new();
        public static void WaitEndOfFrame(this MonoBehaviour mono, System.Action action)
        {
            static IEnumerator _(System.Action action)
            {
                yield return m_WaitForEndOfFrame;
                action?.Invoke();
            }
            mono.StartCoroutine(_(action));
        }

        public static void DelayFrame(this MonoBehaviour mono, int frameCount, System.Action action)
        {
            static IEnumerator _(int frameCount, System.Action action)
            {
                while (frameCount > 0)
                {
                    frameCount--;
                    yield return null;
                }
                action?.Invoke();
            }
            mono.StartCoroutine(_(frameCount, action));
        }

        public static void WaitSecondsUnscaled(this MonoBehaviour mono, float seconds, System.Action action)
        {
            static IEnumerator _(float seconds, System.Action action)
            {
                float timer = 0f;
                while (timer < seconds)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }
                action?.Invoke();
            }
            mono.StartCoroutine(_(seconds, action));
        }

        public static void WaitSeconds(this MonoBehaviour mono, float seconds, System.Action action)
        {
            static IEnumerator _(float seconds, System.Action action)
            {
                float timer = 0f;
                while (timer < seconds)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                action?.Invoke();
            }
            mono.StartCoroutine(_(seconds, action));
        }
    }


}