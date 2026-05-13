using UnityEngine;

namespace Game
{
    public class ExitGameMachine : GameOption
    {
        private bool m_Active;
        private float m_Timer;

        protected override void Update()
        {
            base.Update();
            if (IsPowered)
                m_Active = true;
            
            if (m_Active)
            {
                m_Timer += Time.deltaTime;
                if (m_Timer >= 0.5f)
                {
                    ExitGame();
                }
            }
        }

        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}