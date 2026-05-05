using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Game/Audio/AudioClipSet", fileName = "NewAudioClipSet")]
    public class AudioClipSet : ScriptableObject
    {
        public enum EPlayMode
        {
            Random,
            Sequence,
        }

        [SerializeField] private EPlayMode m_PlayMode = EPlayMode.Random;
        [SerializeField] private List<AudioClip> m_Clips = new();
        [SerializeField] private List<AudioClip> m_TheHarmony = new(16);

        // 序列索引按 SO 实例全局计数
        private int m_SequenceIndex;

        public AudioClip GetNextClip()
        {
            if(HarmonyManager.Instance.Active)
            {
                var harmonyClip = GetHarmonyClip(HarmonyManager.Instance.CurrentNoteIndex);
                if (harmonyClip != null) return harmonyClip;
            }
            
            if (m_Clips == null || m_Clips.Count == 0) return null;

            if (m_PlayMode == EPlayMode.Random)
                return m_Clips[Random.Range(0, m_Clips.Count)];

            var clip = m_Clips[m_SequenceIndex % m_Clips.Count];
            m_SequenceIndex++;
            return clip;
        }

        public AudioClip GetHarmonyClip(int noteIndex)
        {
            if (m_TheHarmony == null || m_TheHarmony.Count == 0) return null;

            int currentIndex = HarmonyManager.Instance.CurrentNoteIndex;
            
            if (currentIndex < 0 || currentIndex >= m_TheHarmony.Count) return null;

            return m_TheHarmony[currentIndex];
        }

        public void ResetSequence() => m_SequenceIndex = 0;
    }
}
