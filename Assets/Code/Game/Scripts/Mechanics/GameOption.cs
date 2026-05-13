
using Mmang.Generic;
using UnityEngine;

namespace Game
{
    public class GameOption : MonoBehaviour, IChargable
    {
        [SerializeField] private InterfaceObject<IMLight> m_Light;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        public void SetChargeComplete(bool init)
        {
            
        }

        #endregion

        //
        private bool m_LightActive;

        protected virtual void Update()
        {
            if (IsPowered)
                m_LightActive = true;
            if (m_LightActive)
            {
                var light = m_Light.Value;
                if (light != null)
                {
                    light.LightIntensity = Mathf.Clamp01(light.LightIntensity + Time.deltaTime * 3f);
                }
            }
        }
    }
}