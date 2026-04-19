using UnityEngine;

namespace Mmang.Util
{
    [DisallowMultipleComponent]
    public class SingleDontDestroyOnLoadComponent : SingletonMono<SingleDontDestroyOnLoadComponent>
    {
        protected override void OnAwake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                UnityEditor.SceneVisibilityManager.instance.Show(gameObject, false);
#endif
            DontDestroyOnLoad(gameObject);
        }
    }
}