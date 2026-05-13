
using UnityEngine;

namespace Game
{
    public class FA_LookAtPoint : FishAIAbility
    {
        [SerializeField] private Transform m_TargetPoint;

        private FB_Swim m_SwimBehaviour;

        public override bool CanActivateAbility()
        {
            return true;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            m_SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            m_SwimBehaviour.RotateToTargetPoint = true;
            m_SwimBehaviour.Tracing = false;
            m_SwimBehaviour.TargetPoint = m_TargetPoint.position;
        }
    }
}