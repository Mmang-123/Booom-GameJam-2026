using UnityEngine;

namespace Game
{
    public class LanguageSwitcher : PlayerTrigger
    {
        [SerializeField] private LocalizationManager.ELanguage m_Language;

        private bool m_Triggered = false;
        private bool m_LastFrameTriggered;

        protected override void FixedUpdate()
        {
            m_Triggered = false;
            base.FixedUpdate();

            if (!m_LastFrameTriggered && m_Triggered)
            {
                SwitchLanguage();
            }
            m_LastFrameTriggered = m_Triggered;
        }

        protected override void Trigger(Fish fish)
        {
            m_Triggered = true;
        }

        private void SwitchLanguage()
        {
            LocalizationManager.Instance.SetLanguage(m_Language);   
        }
    }
}