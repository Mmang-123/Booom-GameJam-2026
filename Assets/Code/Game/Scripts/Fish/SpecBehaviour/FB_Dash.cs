using UnityEngine;

namespace Game
{
    public class FB_Dash : FB_Skill
    {
        [SerializeField] private float m_CD = 2.5f;
        [SerializeField] private float m_DashSpeed = 16f;
        [SerializeField] private float m_DashEndDamping = 32f;

        [Header("残影")]
        [SerializeField] private Color m_AfterimageTint = new Color(0.55f, 0.85f, 1f, 0.75f);
        [SerializeField] private float m_AfterimageFadeDuration = 0.25f;
        [SerializeField] private float m_AfterimageInterval = 0.05f;
        [SerializeField] private float m_AfterimageSpawnDuration = 0.4f; // 残影生成持续时长，可长于冲刺本身
        [SerializeField] private Material m_AfterimageMaterial;

        // Runtime
        public bool Active { get; private set; }
        public float CD { get; private set; }
        public float Timer { get; private set; }
        public int DashState { get; private set; }

        private float m_AfterimageTimer;
        private float m_AfterimageSpawnTimer; // > 0 时持续生成残影

        private void Update()
        {
            if (!Active && CD > 0f)
            {
                CD -= Time.deltaTime;
            }
            else if (Active)
            {
                DashUpdate();
            }

            // 残影生成独立于冲刺逻辑，持续到 m_AfterimageSpawnTimer 归零
            if (m_AfterimageSpawnTimer > 0f)
            {
                m_AfterimageSpawnTimer -= Time.deltaTime;
                m_AfterimageTimer -= Time.deltaTime;
                if (m_AfterimageTimer <= 0f)
                {
                    SpawnAfterimage();
                    m_AfterimageTimer = m_AfterimageInterval;
                }
            }
        }

        public override bool CanUse()
        {
            return !Active && CD <= 0f;
        }

        public override void Use()
        {
            DashStart();
        }

        #region 冲刺逻辑

        private void DashStart()
        {
            Active = true;
            Timer = 0f;
            m_AfterimageTimer = 0f;
            m_AfterimageSpawnTimer = m_AfterimageSpawnDuration;
            var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
            var animatorBehaviour = Fish.GetBehaviour<FB_GenericAnimator>();
            swimBehaviour.IsDisable = true;
            animatorBehaviour.TriggerDashAnimation();
            DashState = 0;
        }

        private void SpawnAfterimage()
        {
            var root = transform;
            AfterimagePool.Spawn(root, m_AfterimageTint, m_AfterimageFadeDuration, m_AfterimageMaterial);
        }

        private void DashUpdate()
        {
            var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
            Timer += Time.deltaTime;

            if (Timer <= 0.2f)
            {
                float t = Mathf.Clamp01(Timer / 0.2f);
                if (t < 0.3f)
                {
                    t /= 0.3f;
                    t = Mathf.Sqrt(t);
                }
                else
                {
                    t = 1f;
                }

                float speed = t * m_DashSpeed;
                swimBehaviour.CurrentSpeed = speed;
            }
            else
            {
                swimBehaviour.CurrentSpeed = m_DashSpeed;
                //DashState = 1;
                var additionalVelocity = AdditionalVelocity.Create(Mathf.Max(0f, swimBehaviour.CurrentSpeed - swimBehaviour.MaxSpeed) * Fish.ForwardDirection, m_DashEndDamping);
                swimBehaviour.AddAdditionalVelocity(additionalVelocity);
                swimBehaviour.CurrentSpeed = Mathf.Min(swimBehaviour.MaxSpeed, swimBehaviour.CurrentSpeed);
                swimBehaviour.IsDisable = false;
                DashEnd();
            }
        }

        private void DashEnd()
        {
            Active = false;
            CD = m_CD;

            var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
            swimBehaviour.IsDisable = false;
        }

        #endregion
    }
}