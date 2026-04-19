using UnityEngine;

namespace Mmang.Game
{
    [RequireComponent(typeof(Animator))]
    public class RootMotionTransmitter : MonoBehaviour
    {
        private Animator m_Animator;

        public event System.Action<Animator> OnMove;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        private void OnDisable()
        {
            OnMove = null;    
        }

        private void OnAnimatorMove()
        {
            OnMove?.Invoke(m_Animator);
        }
    }
}