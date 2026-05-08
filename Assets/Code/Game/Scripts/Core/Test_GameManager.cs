using UnityEngine;

namespace Game
{
    public class Test_GameManager : MonoBehaviour
    {
        [SerializeField] private string m_LoadLevelName;        
        
        [ContextMenu("Load Level")]
        private void LoadLevel()
        {
            var loadLevelParams = new LoadLevelParams(m_LoadLevelName);
            GameManager.Instance.LoadLevel(loadLevelParams);
        }
    }
}