using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class FishCollider : MonoBehaviour
    {
        public Fish Fish { get; private set; }
        public void Init(Fish fish)
        {
            Fish = fish;
        }
    }
}