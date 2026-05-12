using UnityEngine;

namespace Game
{
    public class ExitGameMachine : MonoBehaviour, IChargable
    {
        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        #endregion

        private void Update()
        {
            if (IsPowered)
                ExitGame();
        }

        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetChargeComplete(bool init)
        {
            
        }
    }
}