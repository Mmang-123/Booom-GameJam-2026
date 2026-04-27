using UnityEngine;

namespace Game
{
    public abstract class FishBehaviour : MonoBehaviour
    {
        public Fish Fish { get; private set; }

        public void Init(Fish fish)
        {
            Fish = fish;
        }

        public virtual void BeforeFishUpdate() { }
        public virtual void BeforeFishFixedUpdate() { }

    }
}