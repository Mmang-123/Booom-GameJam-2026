using Game;
using UnityEngine;

namespace Mmang.PixelartRender
{
    public enum ELightType
    {
        Point, Spot, Area
    }

    [ExecuteAlways]
    public abstract class MLight : MonoBehaviour, IMLight
    {
        #region IMLight
        public abstract float LightRadius { get; set; }
        public abstract float LightIntensity { get; set; }
        #endregion

        protected virtual void OnEnable()
        {
            LightingManager.Instance.RegisterLight(this);
        }

        protected virtual void OnDisable()
        {
            if (LightingManager.InstanceValid)
                LightingManager.Instance.UnregisterLight(this);
        }

        public abstract Bounds GetBounds();
    }
}