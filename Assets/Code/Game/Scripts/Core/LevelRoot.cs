
using UnityEditor;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class LevelRoot : MonoBehaviour
    {
        [SerializeField] private string m_LevelName;
        [SerializeField] private Fish m_InitFish;
        public string LevelName => m_LevelName;
        public Fish InitFish => m_InitFish;

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
                    EditorUtility.SetDirty(component);
                }
            }
        }

#endif
    }
}