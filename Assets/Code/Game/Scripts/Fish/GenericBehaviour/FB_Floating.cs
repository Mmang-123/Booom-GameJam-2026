using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FB_Floating : FishBehaviour
    {
        [SerializeField] private float m_FallTime = 0.8f;
        [SerializeField] private float m_RiseTime = 0.2f;
        [SerializeField] private float m_FallDistance = 1.5f;

        // Runtime
        private FB_Swim m_SwimBehaviour;
        private float m_Timer;

        public bool CanFloating => m_SwimBehaviour == null || (m_SwimBehaviour.Tracing == false && m_SwimBehaviour.CurrentSpeed <= 0.1f);

        private void Start()
        {
            m_SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
        }

        public override void BeforeFishFixedUpdate()
        {
            float GetLength(float t)
            {
                //Debug.Log("t: " + t);
                return Mathf.SmoothStep(0f, m_FallDistance, t);
            }

            if (CanFloating)
            {
                float totalTime = m_FallTime + m_RiseTime;
                float newTime = m_Timer + Time.fixedDeltaTime;
                //Debug.Log("newTime: " + newTime);

                if (m_Timer < m_FallTime)
                {
                    float toTime = Mathf.Min(newTime, m_FallTime);
                    float moveDistance = GetLength(toTime / m_FallTime) - GetLength(m_Timer / m_FallTime);
                    Fish.Move(Vector2.down * moveDistance);
                    m_Timer = toTime;
                }

                if (m_Timer >= m_FallTime)
                {
                    float toTime = Mathf.Min(newTime, totalTime);
                    //Debug.Log("to " + toTime);
                    float moveDistance = GetLength((toTime - m_FallTime) / m_RiseTime) - GetLength((m_Timer - m_FallTime) / m_RiseTime);
                    //Debug.Log(moveDistance);
                    Fish.Move(Vector2.up * moveDistance);
                    m_Timer = toTime;
                }

                if (m_Timer >= totalTime)
                {
                    m_Timer = 0f;
                }
            }
            else
            {
                m_Timer = 0f;
            }
        }
    }
}