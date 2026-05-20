
using Mmang.Util;

namespace Game
{
    public class LocalizationManager : SingletonMono<LocalizationManager>
    {
        public enum ELanguage { Chinese, English }

        public static ELanguage CurrentLanguage { get; private set; }
        public static System.Action<ELanguage> OnLanguageChanged;

        public void SetLanguage(ELanguage language)
        {
            if (CurrentLanguage == language)
                return;
            CurrentLanguage = language;
            OnLanguageChanged?.Invoke(language);
        }
    }
}