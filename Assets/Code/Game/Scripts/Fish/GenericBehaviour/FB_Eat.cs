using UnityEngine;

namespace Game
{
    public class FB_Eat : FishBehaviour
    {
        public enum EState
        {
            Shut, Open, Wait
        }
        
        [SerializeField] private float m_OpenDistance = 3f;
        [SerializeField] private float m_ShutDistance = 4f;

        // Runtime
        public Fish Target { get; set; }
        public EState State { get; private set; }

        private void Update()
        {
            switch (State)
            {
                case EState.Shut:
                    ShutUpdate();
                    break;
                case EState.Open:
                    OpenUpdate();
                    break;
                case EState.Wait:
                    WaitUpdate();
                    break;
            }
        }

        private void ShutUpdate()
        {
            if (Target != null)
            {
                float distance = Vector2.Distance(Fish.Position, Target.Position);
                if (distance <= m_OpenDistance)
                {
                    State = EState.Open;
                    return;
                }
            }
        }

        private void OpenUpdate()
        {
            if (Target != null)
            {
                float distance = Vector2.Distance(Fish.Position, Target.Position);
                if (distance > m_ShutDistance)
                {
                    State = EState.Shut;
                    return;
                }
            }
            else
            {
                State = EState.Shut;
            }
        }

        private void WaitUpdate()
        {
            
        }
        
    }
}