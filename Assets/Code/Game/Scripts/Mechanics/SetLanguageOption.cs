
using UnityEngine;

namespace Game
{
    public class SetLanguageOption : SettingOption
    {
        [SerializeField] private LocalizationManager.ELanguage m_Language;

        protected override void OnTrigger()
        {
            base.OnTrigger();
            LocalizationManager.Instance.SetLanguage(m_Language);
        }
    }
}