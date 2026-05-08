using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PassageTrigger : MonoBehaviour
    {
        [SerializeField] private Passage m_Passage;

        private HashSet<Collider2D> m_ColliderSet = new();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (m_ColliderSet.Count > 0)
                return;

            if (other.TryGetComponent<FishCollider>(out var fishCollider)
            && fishCollider.Fish is { } fish && fish.IsPlayer)
            {
                m_ColliderSet.Add(other);
                m_Passage.OnTrigger(fish);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (m_ColliderSet.Contains(other))
            {
                m_ColliderSet.Remove(other);
            }
        }
    }
}