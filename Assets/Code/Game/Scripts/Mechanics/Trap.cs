
using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine;

namespace Game
{
    public class Trap : MonoBehaviour, IChargable
    {
        [SerializeField] private InterfaceObject<IPowerSource> m_PowerSource;
        [SerializeField] private Transform m_TrapPoint;
        [SerializeField] private float m_WaitTime = 0.8f;
        [SerializeField] private float m_Width;
        [SerializeField] private float m_ActiveTime = 0.2f;
        [SerializeField] private Transform m_LeftCage;
        [SerializeField] private Transform m_RightCage;
        [SerializeField] private List<ParticleSystem> m_HitParticles = new();
        [SerializeField] private List<ParticleSystem> m_MoveParticles = new();

        // Runtime
        private bool m_Active;
        private float m_WaitTimer;
        private bool m_Completed;
        private float m_T;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        public void SetChargeComplete(bool init)
        {
            
        }

        #endregion

        private void Start()
        {
            m_Active = false;
            m_Completed = false;
            SetCage(0);

            if (m_PowerSource.Value != null)
                PowerSourceHandler.AddPowerSource(m_PowerSource.Value);
        }

        private void Update()
        {
            if (!m_Active)
            {
                SetActive(IsPowered);
            }
            else if (m_Active && !m_Completed && m_T < 1f)
            {
                var fish = PlayerController.Instance.Fish;
                float distance = Vector2.Distance(fish.Position, m_TrapPoint.position);
                var swim = fish.GetBehaviour<FB_Swim>();
                if (distance <= 0.2f)
                {
                    swim.Tracing = false;
                }
                else
                {
                    swim.Tracing = true;
                    swim.TargetPoint = m_TrapPoint.position;
                }

                if (m_WaitTimer < m_WaitTime)
                {
                    m_WaitTimer += Time.deltaTime;
                    return;
                }

                m_T = Mathf.Clamp01(m_T + Time.deltaTime / m_ActiveTime);
                SetCage(m_T * m_T);
                if (m_T >= 1f)
                {
                    OnCompleted();
                }
            }
        }

        private void SetCage(float t)
        {
            Vector2 offset = m_Width * (1f - t) / 2f * Vector2.left;
            Vector2 leftPos = (Vector2)m_TrapPoint.position + offset;
            Vector2 rightPos = (Vector2)m_TrapPoint.position - offset;
            
            m_LeftCage.position = leftPos;
            m_RightCage.position = rightPos;
        }

        private void SetActive(bool active)
        {
            if (active == m_Active)
                return;
            m_Active = active;

            PlayerController.Instance.DisableControl(m_ActiveTime + m_WaitTime + 0.5f);
            foreach (var particle in m_MoveParticles)
            {
                particle.Play();
            }

            var fish = PlayerController.Instance.Fish;
            if (fish != null)
            {
                var swimBehaviour = fish.GetBehaviour<FB_Swim>();
                swimBehaviour.Tracing = true;
                swimBehaviour.TargetPoint = m_TrapPoint.position;
            }
        }

        private void OnCompleted()
        {
            m_Completed = true;
            foreach (var particle in m_HitParticles)
            {
                particle.Play();
            }
            foreach (var particle in m_MoveParticles)
            {
                particle.Stop();
            }
        }
    }
}