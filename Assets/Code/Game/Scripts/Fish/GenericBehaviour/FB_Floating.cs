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

        [SerializeField] private bool m_PlayAnim = true;
        [SerializeField] private string m_AnimName = "IdleJump";

        // Runtime
        private FB_Swim m_SwimBehaviour;
        private float m_Timer;
        private bool m_AnimPlayed = false;

        public bool CanFloating => m_SwimBehaviour == null || (m_SwimBehaviour.Tracing == false && m_SwimBehaviour.CurrentSpeed <= 0.1f);

        private void Start()
        {
            m_SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
        }

        public override void BeforeFishFixedUpdate()
        {
            float GetLength(float t)
            {
                return Mathf.SmoothStep(0f, m_FallDistance, t);
            }

            if (CanFloating)
            {
                float totalTime = m_FallTime + m_RiseTime;
                float newTime = m_Timer + Time.fixedDeltaTime;

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
                    float moveDistance = GetLength((toTime - m_FallTime) / m_RiseTime) - GetLength((m_Timer - m_FallTime) / m_RiseTime);
                    Fish.Move(Vector2.up * moveDistance);
                    m_Timer = toTime;

                    if (!m_AnimPlayed && m_PlayAnim)
                    {
                        m_AnimPlayed = true;
                        Fish.GetBehaviour<FB_GenericAnimator>().TriggerCustomAnimation(m_AnimName);
                    }
                }

                if (m_Timer >= totalTime)
                {
                    m_Timer = 0f;
                    m_AnimPlayed = false;
                }
            }
            else
            {
                m_AnimPlayed = false;
                m_Timer = 0f;
            }
        }
    }
}