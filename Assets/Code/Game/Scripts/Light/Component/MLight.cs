using UnityEngine;

namespace Mmang.PixelartRender
{
    [ExecuteAlways]
    public abstract class MLight : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            LightingManager.Instance.RegisterLight(this);
        }

        protected virtual void OnDisable()
        {
            if (LightingManager.InstanceValid)
                LightingManager.Instance.UnregisterLight(this);
        }
    }
}