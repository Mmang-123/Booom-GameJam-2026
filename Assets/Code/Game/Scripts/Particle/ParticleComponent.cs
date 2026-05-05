using UnityEngine;

namespace Game
{
    public class ParticleComponent : MonoBehaviour
    {
        [Header("粒子名(不要重复)")]
        [SerializeField] private string m_ParticleName;
    }
}