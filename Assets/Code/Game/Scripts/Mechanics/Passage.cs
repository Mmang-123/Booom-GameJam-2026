
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    using EState = LoopZone.EState;

    public class Passage : MonoBehaviour
    {
        public enum ELoadFlag { LeftLevel, RightLevel }

        [SerializeField] private string m_PassageName;
        [SerializeField] private string m_LeftLevelName;
        [SerializeField] private string m_RightLevelName;
        [SerializeField] private LoopZone m_LoopZone;
        [SerializeField] private int m_MinLoopCount = 1;


        public string PassageName => m_PassageName;
        public string LeftLevelName => m_LeftLevelName;
        public string RightLevelName => m_RightLevelName;

        // Runtime
        private bool m_InZone;
        private bool m_Loading;
        private ELoadFlag m_LoadFlag;
        private int m_LoopCount;

        private void Start()
        {
            if (m_LoopZone != null)
            {
                m_LoopZone.CheckFunc = CheckLoop;
            }
            SetLoading(false);
        }

        private void FixedUpdate()
        {
            var playerFish = PlayerController.Instance.Fish;
            if (playerFish != null)
            {
                var states = m_LoopZone.CheckPosition(playerFish.Position);
                if (states.horizontalState == EState.Include
                && states.verticalState == EState.Include)
                {
                    if (!m_InZone)
                    {
                        m_InZone = true;
                        OnTrigger(playerFish);
                    }
                }
                else
                {
                    m_InZone = false;
                }
            }
            else
            {
                m_InZone = false;
            }
        }

        public void OnTrigger(Fish fish)
        {
            if (m_Loading || m_LoopZone.Active || !GameManager.Instance.CanLoad || GameManager.Instance.InOrLoadingTitle)
                return;
            
            var currentLevelName = GameManager.Instance.CurrentLevelRoot.LevelName;
            if (currentLevelName != LeftLevelName && currentLevelName != RightLevelName)
            {
                Debug.LogWarning("当前场景名称跟通道不匹配");
            }

            Load(currentLevelName == m_LeftLevelName ? ELoadFlag.RightLevel : ELoadFlag.LeftLevel);
        }

        private bool CheckLoop((EState horizontalState, EState verticalState) states)
        {
            m_LoopCount++;

            if (m_Loading)
                return true;

            bool flag = true;

            if (m_LoadFlag == ELoadFlag.LeftLevel && states.horizontalState == EState.Negative)
            {
                flag = false;
            }
            if (m_LoadFlag == ELoadFlag.RightLevel && states.horizontalState == EState.Positive)
            {
                flag = false;
            }

            if (flag)
            {
                if (GameManager.Instance.Loading)
                    return true;

                // 重新加载反方向房间
                Load(m_LoadFlag == ELoadFlag.LeftLevel ? ELoadFlag.RightLevel : ELoadFlag.LeftLevel);   
                return true;
            }
            else
            {
                if (m_LoopCount < m_MinLoopCount + 1)
                    return true;
                
                m_LoopZone.SetActive(false);
                return false;
            }
        }

        private void Load(ELoadFlag loadFlag)
        {
            m_LoopCount = 0;
            m_LoadFlag = loadFlag;
            SetLoading(true);
            m_LoopZone.SetActive(true);

            string loadLevelName = loadFlag == ELoadFlag.LeftLevel ? m_LeftLevelName : m_RightLevelName;
            var loadParams = new LoadLevelParams(loadLevelName);
            GameManager.Instance.LoadLevel(loadParams, () =>
            {
                SetLoading(false);
            });
        }

        private void SetLoading(bool active)
        {
            m_Loading = active;
        }
    }
}