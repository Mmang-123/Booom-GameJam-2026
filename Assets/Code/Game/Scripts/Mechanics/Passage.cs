
using UnityEngine;

namespace Game
{
    public class Passage : MonoBehaviour
    {
        [SerializeField] private string m_PassageName;
        public string PassageName => m_PassageName;
    }
}