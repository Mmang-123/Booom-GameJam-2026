using UnityEngine;

namespace Game
{
    public class SetWindowOption : SettingOption
    {
        [SerializeField] private bool m_FullScreen;

        private void Start()
        {
            if (PlayerPrefs.HasKey("FullScreen"))
            {
                SetFullScreen(PlayerPrefs.GetInt("FullScreen") == 1);
            }
        }

        protected override void OnTrigger()
        {
            base.OnTrigger();
            SetFullScreen(m_FullScreen);
        }

        private void SetFullScreen(bool full)
        {
            Screen.fullScreen = full;
            PlayerPrefs.SetInt("FullScreen", full ? 1 : 0);
        }
    }
}