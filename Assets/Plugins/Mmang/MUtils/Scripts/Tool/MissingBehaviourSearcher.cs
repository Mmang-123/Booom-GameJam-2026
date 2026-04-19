using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Mmang.Tool
{
    public class MissingBehaviourSearcher : MonoBehaviour
    {
        [SerializeField] private bool m_Kill = false;

        [ContextMenu("Execute")]
        public void Execute()
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject obj;

            foreach (GameObject go in allObjects)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(go) && PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    continue; // 跳过了预制体实例
                }

                Component[] components = go.GetComponents<Component>();

                foreach (var component in components)
                {
                    if (component == null)
                    {
                        Debug.Log("GameObject with missing script found: " + go.name, go);
                        break;
                    }
                }
            }

            if (!m_Kill)
                return;

            while (GameObject.Find("SceneIDMap") != null)
            {
                obj = GameObject.Find("SceneIDMap");
                if (obj != null)
                {
                    DestroyImmediate(obj);
                    Debug.Log("Cleared a SceneIDMap instance");
                }
                else
                {
                    Debug.Log("Clear Completed!");
                    break;
                }
            }
        }
    }

}
#endif