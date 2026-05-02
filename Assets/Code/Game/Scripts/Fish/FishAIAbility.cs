using UnityEngine;
using Mmang.Game;

namespace Game
{

    public enum EEndAbilityType
    {
        End, Cancelled
    }

    public abstract class FishAIAbility : MonoBehaviour
    {
        [SerializeField] private int m_Priority = 0;
        public int Priority => m_Priority;

        // Runtime
        public Fish Fish { get; private set; }
        public bool Active { get; protected set; }

        public void Init(Fish fish)
        {
            Fish = fish;
        }

        public virtual bool CanActivateAbility()
        {
            return true;
        }

        public void ActivateAbility()
        {
            Active = true;
            OnActivate();
        }

        public void EndAbility(EEndAbilityType endType)
        {
            Active = false;
            OnEnd(endType);
        }

        public virtual void OnActivate() { }
        public virtual void OnEnd(EEndAbilityType endType) { }
        public virtual void OnUpdate(float dt) { }

    }

}