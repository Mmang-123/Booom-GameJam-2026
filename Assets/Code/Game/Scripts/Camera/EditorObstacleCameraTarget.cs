
using Mmang.PixelartRender;
using UnityEngine;

namespace Game
{
    [ExecuteAlways]
    public class EditorObstacleCameraTarget : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && ObstacleMaskManager.InstanceValid)
            {
                ObstacleMaskManager.Instance.UpdatePosition(transform.position);
            }
        }
#endif
    }
}