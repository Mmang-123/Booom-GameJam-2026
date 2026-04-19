using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mmang.Game;
using UnityEditor;
using UnityEngine;

namespace Mmang.Generations
{
    public enum EGenerationType
    {
        PerControlPoint, PerTriangles, PerGenerationPoint
    }

    [CreateAssetMenu(fileName = nameof(GenerationPointComputeConfig), menuName = "Configs/Generation/ComputeConfig")]
    public class GenerationPointComputeConfig : ScriptableObject
    {
        [SerializeField] private ComputeShader m_ComputeShader;
        [SerializeField] private EGenerationType m_GenerationType;
        [SerializeField] private uint m_GenerationCountPerControlPoint;
        [SerializeField] private bool m_ReceiveInteractionBuffer;

        [SerializeField, PropertyContainer(typeof(float), typeof(int))]
        private PropertyContainer m_Properties = new();

        public ComputeShader ComputeShader => m_ComputeShader;
        public EGenerationType GenerationType => m_GenerationType;
        public int GenerationCountPerControlPoint => (int)m_GenerationCountPerControlPoint;

        public bool ReceiveInteractionBuffer => m_ReceiveInteractionBuffer;

        public PropertyContainer Properties => m_Properties;
    }
}