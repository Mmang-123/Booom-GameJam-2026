using UnityEngine;

namespace Mmang.Util
{
    [DisallowMultipleComponent]
    public class DontDestroyOnLoadComponent : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}