using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class AudioClipRef
    {
        public enum ESource { Single, Set }

        [SerializeField] private ESource m_Source = ESource.Single;
        [SerializeField] private AudioClip m_Clip;
        [SerializeField] private AudioClipSet m_Set;

        public AudioClip GetNextClip() =>
            m_Source == ESource.Single ? m_Clip : m_Set?.GetNextClip();

        public bool IsNull =>
            m_Source == ESource.Single ? m_Clip == null : m_Set == null;
    }
}
