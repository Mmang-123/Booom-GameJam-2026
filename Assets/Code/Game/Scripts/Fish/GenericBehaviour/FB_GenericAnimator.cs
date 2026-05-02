using UnityEngine;

namespace Game
{
    public class FB_GenericAnimator : FishBehaviour
    {
        [SerializeField] private Animator m_Animator;

        // Runtime
        private FB_Swim m_SwimBehaviour;

        private void Start()
        {
            m_SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
        }

        private void Update()
        {
            m_Animator.SetBool("IsMoving", m_SwimBehaviour.CurrentSpeed > 0.1f);
        }

        public void TriggerDashAnimation()
        {
            m_Animator.SetTrigger("Dash");
        }
    }
}