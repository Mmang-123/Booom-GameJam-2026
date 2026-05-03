using UnityEngine;

namespace Game
{
    public class FB_GenericAnimator : FishBehaviour
    {
        [SerializeField] private Animator m_Animator;
        [SerializeField] private bool m_CanEat = true;

        // Runtime
        private FB_Swim m_SwimBehaviour;
        private FB_Eat m_EatBehaviour;

        private void Start()
        {
            m_SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
            m_EatBehaviour = Fish.GetBehaviour<FB_Eat>();
        }

        private void Update()
        {
            m_Animator.SetBool("IsMoving", m_SwimBehaviour.CurrentSpeed > 0.1f);

            if (m_CanEat)
            {
                m_Animator.SetBool("IsOpen", m_EatBehaviour.State == FB_Eat.EState.Open);
            }
        }

        public void TriggerDashAnimation()
        {
            m_Animator.SetTrigger("Dash");
        }

        public void TriggerCatchAnimation()
        {
            m_Animator.SetTrigger("Catch");
        }

        public void TriggerSwallowAnimation(bool infected)
        {
            m_Animator.SetTrigger(infected ? "InfectedSwallow" : "RegularSwallow");
        }

        public void TriggerDieAnimation()
        {
            m_Animator.SetTrigger("Die");
        }
    }
}