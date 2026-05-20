
using UnityEngine;

namespace Game
{
    public class SettingOption : MonoBehaviour, IChargable
    {
        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        public void SetChargeComplete(bool init)
        {
            
        }

        #endregion

        protected bool m_Active;

        protected virtual void Update()
        {
            SetActive(IsPowered);
        }

        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;
            m_Active = active;
            if (m_Active)
                OnTrigger();
        }

        protected virtual void OnTrigger() { }
    }
}