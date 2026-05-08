
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class LevelRoot : MonoBehaviour
    {
        [SerializeField] private string m_LevelName;
        public string LevelName => m_LevelName;

#if UNITY_EDITOR

        public void Editor_RegenerateGUIDs()
        {
            var allComponent = gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in allComponent)
            {
                if (component is ILevelSavable savable)
                {
                    string newGUID = System.Guid.NewGuid().ToString();
                    savable.Editor_SetGUID(newGUID);   
                }
            }
        }

#endif
    }
}