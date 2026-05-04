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
        [SerializeField] private GameplayTagContainer m_Tags;
        [SerializeField] private GameplayTag m_SingletonTag;
        [SerializeField] private GameplayTagContainer m_CancelTags;
        [SerializeField] private int m_Priority = 0;
        public IReadOnlyGameplayTagContainer Tags => m_Tags.AsReadOnly();
        public GameplayTag SingletonTag => m_SingletonTag;
        public IReadOnlyGameplayTagContainer CancelTags => m_CancelTags.AsReadOnly();
        public int Priority => m_Priority;

        // Runtime
        public FishAIComponent FishAI { get; private set; }
        public Fish Fish => FishAI.Fish;
        public bool Active { get; protected set; }

        public void Init(FishAIComponent fishAI)
        {
            FishAI = fishAI;
            OnInit();
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

        protected virtual void OnInit() { }
        public virtual void OnActivate() { }
        public virtual void OnEnd(EEndAbilityType endType) { }
        public virtual void OnUpdate(float dt) { }

    }

}