
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class LevelRoot : MonoBehaviour
    {
        [SerializeField] private string m_LevelName;
        public string LevelName => m_LevelName;
    }
}