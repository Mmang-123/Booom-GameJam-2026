using Mmang.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{

    public struct LoadLevelParams
    {
        public string LevelName;
        public LoadLevelParams(string levelName)
        {
            LevelName = levelName;
        }
    }

    public class GameManager : SingletonMono<GameManager>
    {
        public enum EMode
        {
            Game, Edit
        }

        [SerializeField] private EMode m_Mode;
        [SerializeField] private LevelRoot m_LevelRoot;

        public LevelRoot CurrentLevelRoot => m_LevelRoot;
        public bool LevelValid => CurrentLevelRoot != null;

        //
        private bool m_Loading = false;

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void Update()
        {

        }

        public void LoadLevel(LoadLevelParams loadLevelParams)
        {
            var levelData = LevelConfig.GetLevel(loadLevelParams.LevelName);
            if (levelData == null)
            {
                Debug.LogWarning($"无法找到关卡 {loadLevelParams.LevelName}");
                return;
            }

            UnloadCurrentLevel();
            m_Loading = true;
            var operation = InstantiateAsync<LevelRoot>(levelData.Prefab);
            operation.completed += (op) => OnLoadLevelCompleted(operation.Result[0]);
        }

        private void UnloadCurrentLevel()
        {
            if (m_LevelRoot != null)
            {
                Debug.Log("Unloading: " + m_LevelRoot.gameObject);
                Destroy(m_LevelRoot.gameObject);
            }
            m_LevelRoot = null;
        }

        private void OnLoadLevelCompleted(LevelRoot levelRoot)
        {
            Debug.Log("Load Complete: " + levelRoot);
            m_Loading = false;
            m_LevelRoot = levelRoot;
        }
    }
}