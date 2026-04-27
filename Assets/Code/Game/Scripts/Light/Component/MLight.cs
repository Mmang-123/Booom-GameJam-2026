using UnityEngine;

namespace Mmang.PixelartRender
{
    public enum ELightType
    {
        Point, Spot, Area
    }

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