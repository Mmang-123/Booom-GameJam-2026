
using Mmang.Util;
using UnityEngine;

namespace Game
{
    public class LocalizationManager : SingletonMono<LocalizationManager>
    {
        public enum ELanguage { Chinese = 0, English = 1 }

        public static ELanguage CurrentLanguage { get; private set; }
        public static System.Action<ELanguage> OnLanguageChanged;

        protected override void OnAwake()
        {
            base.OnAwake();
            InitLanguage();
        }

        public void SetLanguage(ELanguage language)
        {
            if (CurrentLanguage == language)
                return;
            CurrentLanguage = language;
            OnLanguageChanged?.Invoke(language);
            PlayerPrefs.SetInt("Language", (int)CurrentLanguage);
        }

        private void InitLanguage()
        {
            if (PlayerPrefs.HasKey("Language"))
            {
                int val = PlayerPrefs.GetInt("Language");
                CurrentLanguage = (ELanguage)val;
                return;
            }

            var systemLang = Application.systemLanguage;
            if (systemLang == SystemLanguage.Chinese || systemLang == SystemLanguage.ChineseSimplified || systemLang == SystemLanguage.ChineseTraditional)
                CurrentLanguage = ELanguage.Chinese;
            else
                CurrentLanguage = ELanguage.English;
            PlayerPrefs.SetInt("Language", (int)CurrentLanguage);
        }
    }
}