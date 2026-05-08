using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine;

namespace Game
{

    [System.Serializable]
    public struct LightDetectorSaveData
    {
        public bool Active;
    }

    public class LightDetector : MonoBehaviour, IPowerSource, ILevelSavable
    {
        [SerializeField] private SpriteRenderer m_EmissionLight;
        [SerializeField] private bool m_InitTurnOn = false;

        [Header("音效")]
        [SerializeField] private AudioClipRef m_ActivateClip;
        [SerializeField] private AudioClipRef m_DeactivateClip;

        private float ActiveTime => 0.1f;
        private float MaxActiveTime => ActiveTime * 2f;

        public bool Active => m_Active;
        public bool PowerOn => Active;
        public event System.Action<IPowerSource, bool> OnPowerChanged;

        public Color ActiveColor = Color.green;
        public Color InactiveColor = Color.red;

        // Runtime
        private bool m_Inited = false;
        private float m_ActiveTimer;
        private bool m_Active;

        private void Start()
        {
            InitPowerSource();
        }

        public void InitPowerSource()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            // 加载
            if (!GameManager.Instance.TryLoadSavedData(this))
            {
                // 加载失败按原值
                SetActive(m_InitTurnOn);
            }

            m_EmissionLight.color = m_Active ? ActiveColor : InactiveColor;
            m_ActiveTimer = m_Active ? MaxActiveTime : 0f;
        }

        private void FixedUpdate()
        {
            if (LightingTextureManager.Instance.InValidChunk(transform.position))
            {
                bool lightExist = CheckLightStrength();
                m_ActiveTimer = Mathf.Clamp(m_ActiveTimer + Time.fixedDeltaTime * (lightExist ? 1 : -1), 0f, MaxActiveTime);
                SetActive(m_ActiveTimer >= ActiveTime);
            }
        }

        private bool CheckLightStrength()
        {
            float strength = LightingTextureManager.Instance.GetLightStrength(transform.position);
            return strength >= 0.0625f;
        }

        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;

            m_Active = active;
            OnActiveChanged();
        }

        private void OnActiveChanged()
        {
            m_EmissionLight.color = m_Active ? ActiveColor : InactiveColor;
            OnPowerChanged?.Invoke(this, m_Active);

            var clip = m_Active ? m_ActivateClip : m_DeactivateClip;
            AudioManager.PlayAtPosition(clip, transform.position);
        }


        #region 保存和加载

        [SerializeField, HideInInspector] private string m_GUID = System.Guid.NewGuid().ToString();
        public string GUID => m_GUID;

        public virtual string SaveJson()
        {
            var saveData = new LightDetectorSaveData()
            {
                Active = m_Active
            };

            string json = JsonUtility.ToJson(saveData);
            return json;
        }

        public virtual void LoadJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            var saveData = JsonUtility.FromJson<LightDetectorSaveData>(json);
            SetActive(saveData.Active);
        }

        #endregion
    }
}