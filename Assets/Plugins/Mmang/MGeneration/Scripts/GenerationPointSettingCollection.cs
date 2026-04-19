using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Generations
{
    [CreateAssetMenu(fileName = nameof(GenerationPointSettingCollection), menuName = "Configs/Generation/SettingCollection")]
    public class GenerationPointSettingCollection : GenerationPointBehaviour
    {
        public enum ExecuteType
        {
            All, Random
        }

        public ExecuteType Type;
        public int RandomCount;

        [SerializeField] private List<GenerationPointBehaviour> m_Behaviours = new();
        public List<GenerationPointBehaviour> Behaviours => m_Behaviours;
    
        #region Property Name
        public static string PN_ExecuteType => nameof(Type);
        public static string PN_Behaviours => nameof(m_Behaviours);
        #endregion
    }

}