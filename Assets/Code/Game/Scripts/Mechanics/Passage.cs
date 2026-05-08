
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Passage : MonoBehaviour
    {
        public enum ELoadFlag { Level1, Level2 }

        [SerializeField] private string m_PassageName;
        [SerializeField] private string m_LevelName1;
        [SerializeField] private string m_LevelName2;
        [SerializeField] private List<Collider2D> m_BlockColliders = new();

        public string PassageName => m_PassageName;
        public string LevelName1 => m_LevelName1;
        public string LevelName2 => m_LevelName2;

        // Runtime
        private bool m_Loading;
        private ELoadFlag m_LoadFlag;

        private void Start()
        {
            SetLoading(false);
        }

        public void OnTrigger(Fish fish)
        {
            if (m_Loading)
                return;
            
            var currentLevelName = GameManager.Instance.CurrentLevelRoot.LevelName;
            if (currentLevelName != LevelName1 && currentLevelName != LevelName2)
            {
                Debug.LogWarning("当前场景名称跟通道不匹配");
            }

            Load(currentLevelName == m_LevelName1 ? ELoadFlag.Level2 : ELoadFlag.Level1);
        }

        private void Load(ELoadFlag loadFlag)
        {
            m_LoadFlag = loadFlag;
            SetLoading(true);

            string loadLevelName = loadFlag == ELoadFlag.Level1 ? m_LevelName1 : m_LevelName2;
            var loadParams = new LoadLevelParams(loadLevelName);
            GameManager.Instance.LoadLevel(loadParams, () =>
            {
                Debug.Log("回调");
                SetLoading(false);
            });
        }

        private void SetLoading(bool active)
        {
            m_Loading = active;
            
            foreach (var collider in m_BlockColliders)
            {
                collider.gameObject.SetActive(active);
            }
        }
    }
}