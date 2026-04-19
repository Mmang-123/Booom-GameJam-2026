

using UnityEngine;

namespace Mmang.Topdown
{

    [RequireComponent(typeof(TopdownCameraController))]
    public abstract class TopdownCameraComponent : MonoBehaviour
    {
        protected TopdownCameraController Controller { get; private set; }

        public void Init(TopdownCameraController controller)
        {
            Controller = controller;
            OnInit();
        }

        protected virtual void OnInit() { }
    }

    [DisallowMultipleComponent]
    public class TCameraComponent_Follow : TopdownCameraComponent
    {
        [SerializeField] private Transform m_Target;
        [SerializeField] private Vector3 m_Offset;

        // temp
        [SerializeField] private float m_LerpFactor = 30;
        private Vector3 m_LastPosition;

        protected override void OnInit()
        {
            base.OnInit();
            m_LastPosition = Controller.CenterPosition;
        }

        private void Update()
        {
            if (m_Target != null)
            {
                Vector3 finalPoint = m_Target.position + m_Offset;
                m_LastPosition = Vector3.Lerp(m_LastPosition, finalPoint, m_LerpFactor * Time.deltaTime);
                Controller.CenterPosition = m_LastPosition;
            }
        }
    }
}