using UnityEngine;

namespace Game
{
    public class StartGameMachine : GameOption
    {
        private int m_StartState = 0;

        protected override void Update()
        {
            base.Update();
            if (m_StartState == 0 && IsPowered)
                StartGame();
            else if (m_StartState == 1 && GameManager.Instance.ScreenFadeT >= 0.5f)
                LoadInitLevel();
        }

        private void StartGame()
        {
            m_StartState = 1;
            Debug.Log("!Start!");
            GameManager.Instance.ScreenFadeState = GameManager.EScreenFadeState.FadeIn;
        }

        private void LoadInitLevel()
        {
            m_StartState = 2;
            var loadParams = new LoadLevelParams(LevelConfig.GetInitLevelName(), true);
            GameManager.Instance.LoadLevel(loadParams);
        }
    }
}