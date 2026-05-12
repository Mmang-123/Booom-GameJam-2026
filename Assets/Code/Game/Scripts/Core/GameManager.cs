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
        private bool m_Restarting = false;
        private bool m_CantRestart = false;
        private bool m_RestartLoading = false;

        public bool Restarting => m_Restarting;
        public bool CanLoad => !m_Loading && !m_Restarting;
        public bool CantSave { get; private set; }

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void Update()
        {
            ScreenFadeUpdate(Time.deltaTime);
            if (m_Restarting)
            {
                if (m_CurrentScreenFadeT >= 0.5f && !m_RestartLoading && !m_Loading)
                {
                    StartRestartLoad();
                }
            }
        }

        public void Restart()
        {
            if (m_Restarting || m_CantRestart)
                return;
            Debug.Log("Restart!");
            m_Restarting = true;
            CantSave = true;
            m_CurrentScreenFadeT = 0f;
            m_ScreenFadeState = EScreenFadeState.FadeIn;
        
            // Clear
            m_NameToSaveDataMap.Clear();
            m_CurrentSavables.Clear();
        }

        private void StartRestartLoad()
        {
            m_RestartLoading = true;
            var loadLevelParams = new LoadLevelParams(LevelConfig.GetInitLevelName());
            UnloadCurrentLevel(immediate: true);
            LoadLevel(loadLevelParams);
        }

        private void RestartLoadComplete()
        {
            CantSave = false;
            m_Restarting = false;
            m_RestartLoading = false;
            m_ScreenFadeState = EScreenFadeState.FadeOut;

            if (m_LevelRoot.InitFish != null)
            {
                CameraController.Instance.Teleport(m_LevelRoot.InitFish.transform.position);
                PlayerController.Instance.ControlFish(m_LevelRoot.InitFish);
                //m_LevelRoot.InitFish.SetController(PlayerController.Instance);
            }
            else
            {
                m_CantRestart = true;
            }
        }

        public void LoadLevel(LoadLevelParams loadLevelParams, System.Action completedCallback = null)
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
            operation.completed += (op) =>
            {
                OnLoadLevelCompleted(operation.Result[0]);
                completedCallback?.Invoke();
            };
        }

        private void UnloadCurrentLevel(bool immediate = false)
        {
            if (m_LevelRoot != null)
            {
                Debug.Log("Unloading: " + m_LevelRoot.gameObject);

                if (!CantSave)
                {
                    var savedData = GetSavedData(m_LevelRoot.LevelName);
                    foreach (var savable in m_CurrentSavables)
                    {
                        savedData.Save(savable);
                    }   
                }

                //
                if (!immediate)
                    Destroy(m_LevelRoot.gameObject);
                else
                    DestroyImmediate(m_LevelRoot.gameObject);
            }
            m_LevelRoot = null;
        }

        private void OnLoadLevelCompleted(LevelRoot levelRoot)
        {
            Debug.Log("Load Complete: " + levelRoot);
            m_Loading = false;
            m_LevelRoot = levelRoot;
            m_CurrentSavables.Clear();

            LightingTextureManager.Instance.Clear();

            if (m_RestartLoading)
            {
                RestartLoadComplete();
            }
        }

        public bool TryLoadSavedData(ILevelSavable savable)
        {
            if (m_LevelRoot == null)
            {
                return false;
            }

            if (m_CurrentSavables.Contains(savable))
            {
                Debug.Log("重复加载: " + savable);
                return false;
            }

            m_CurrentSavables.Add(savable);
            var savedData = GetSavedData(m_LevelRoot.LevelName);
            return savedData.Load(savable);
        }

        #region 转换过场和结算

        public enum EScreenFadeState { None, FadeIn, FadeOut }
        private EScreenFadeState m_ScreenFadeState;
        [SerializeField, Range(0, 1)] private float m_CurrentScreenFadeT;
        public float ScreenFadeT => m_CurrentScreenFadeT;

        private void ScreenFadeUpdate(float dt)
        {
            if (m_ScreenFadeState == EScreenFadeState.FadeIn && m_CurrentScreenFadeT < 0.5f)
            {
                m_CurrentScreenFadeT = Mathf.Clamp(m_CurrentScreenFadeT + dt * 0.5f, 0f, 0.5f);
            }
            else if (m_ScreenFadeState == EScreenFadeState.FadeOut && m_CurrentScreenFadeT < 1.0f)
            {
                m_CurrentScreenFadeT = Mathf.Clamp(m_CurrentScreenFadeT + dt * 0.5f, 0.5f, 1f);
            }
            //Shader.SetGlobalFloat("_SceneTransition", m_CurrentScreenFadeT);
        }

        public void Settle()
        {
            m_ScreenFadeState = EScreenFadeState.FadeIn;
        }

        private void SettleUpdate()
        {
            
        }

        #endregion



        #region 保存和加载

        private LevelSaveData GetSavedData(string levelName)
        {
            if (m_NameToSaveDataMap.TryGetValue(levelName, out var result))
                return result;
            var newData = new LevelSaveData();
            m_NameToSaveDataMap.Add(levelName, newData);
            return newData;
        }

        public void Save(ILevelSavable savable)
        {
            if (!LevelValid || CantSave)
                return;

            var savedData = GetSavedData(m_LevelRoot.LevelName);
            savedData.Save(savable);
        }

        #endregion
    }
}