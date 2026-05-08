using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

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

        //
        public LevelRoot CurrentLevelRoot => m_LevelRoot;
        public bool LevelValid => CurrentLevelRoot != null;

        //
        private Dictionary<string, LevelSaveData> m_NameToSaveDataMap = new();
        private HashSet<ILevelSavable> m_CurrentSavables = new();

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
            var operation = InstantiateAsync<LevelRoot>(levelData);
            operation.completed += (op) => OnLoadLevelCompleted(operation.Result[0]);
        }

        private void UnloadCurrentLevel()
        {
            if (m_LevelRoot != null)
            {
                Debug.Log("Unloading: " + m_LevelRoot.gameObject);

                var savedData = GetSavedData(m_LevelRoot.LevelName);
                foreach (var savable in m_CurrentSavables)
                {
                    savedData.Save(savable);
                }

                //
                Destroy(m_LevelRoot.gameObject);
            }
            m_LevelRoot = null;
        }

        private void OnLoadLevelCompleted(LevelRoot levelRoot)
        {
            Debug.Log("Load Complete: " + levelRoot);
            m_Loading = false;
            m_LevelRoot = levelRoot;
            m_CurrentSavables.Clear();
        }

        public bool TryLoadSavedData(ILevelSavable savable)
        {
            if (m_CurrentSavables.Contains(savable))
            {
                Debug.Log("重复加载: " + savable);
                return false;
            }

            m_CurrentSavables.Add(savable);
            var savedData = GetSavedData(m_LevelRoot.LevelName);
            return savedData.Load(savable);
        }

        #region 保存和加载

        private LevelSaveData GetSavedData(string levelName)
        {
            if (m_NameToSaveDataMap.TryGetValue(levelName, out var result))
                return result;
            var newData = new LevelSaveData();
            m_NameToSaveDataMap.Add(levelName, newData);
            return newData;
        }

        #endregion
    }
}