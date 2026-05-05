using Mmang.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        private const string k_PoolName = "AudioOneShotSource";

        [SerializeField] private int m_MaxSources = 32;

        private int m_ActiveCount;
        private readonly Dictionary<GameObject, Coroutine> m_Coroutines = new();

        /// <summary>在指定世界坐标播放一个音效（自动回收）。</summary>
        public static void PlayAtPosition(AudioClip clip, Vector2 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            if (!InstanceValid) return;
            if (Instance.m_ActiveCount >= Instance.m_MaxSources) return;

            var go = GlobalGameObjectPool.GetGameObject(k_PoolName, position, Quaternion.identity, Instance.GetSourcePrefab());
            var source = go.GetComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            Instance.ApplySpatial(source, position);
            source.volume *= volume;
            source.Play();

            Instance.m_ActiveCount++;
            Instance.TrackCoroutine(go, clip.length / Mathf.Abs(pitch));
        }

        /// <summary>播放可手动停止的音效，返回 AudioSource 引用。用 StopManaged 提前停止并回收。</summary>
        public static AudioSource PlayManaged(AudioClip clip, Vector2 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return null;
            if (!InstanceValid) return null;
            if (Instance.m_ActiveCount >= Instance.m_MaxSources) return null;

            var go = GlobalGameObjectPool.GetGameObject(k_PoolName, position, Quaternion.identity, Instance.GetSourcePrefab());
            var source = go.GetComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            Instance.ApplySpatial(source, position);
            source.volume *= volume;
            source.Play();

            Instance.m_ActiveCount++;
            Instance.TrackCoroutine(go, clip.length / Mathf.Abs(pitch));
            return source;
        }

        /// <summary>提前停止并回收由 PlayManaged 返回的 AudioSource。</summary>
        public static void StopManaged(ref AudioSource source)
        {
            if (source == null) return;
            var go = source.gameObject;
            source.Stop();
            source = null;
            if (!InstanceValid) return;

            // 取消对应协程，防止 go 被重用后被旧协程误回收
            if (Instance.m_Coroutines.TryGetValue(go, out var coroutine))
            {
                Instance.StopCoroutine(coroutine);
                Instance.m_Coroutines.Remove(go);
            }

            Instance.m_ActiveCount = Mathf.Max(0, Instance.m_ActiveCount - 1);
            GlobalGameObjectPool.Release(go, k_PoolName);
        }

        /// <summary>在相机中心（2D 无衰减）播放一个音效。</summary>
        public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (Camera.main != null)
                PlayAtPosition(clip, Camera.main.transform.position, volume, pitch);
        }

        private void TrackCoroutine(GameObject go, float delay)
        {
            // 若 go 有残留协程（被重用），先取消
            if (m_Coroutines.TryGetValue(go, out var old))
                StopCoroutine(old);
            m_Coroutines[go] = StartCoroutine(ReleaseAfterPlay(go, delay));
        }

        private IEnumerator ReleaseAfterPlay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay + 0.05f);
            m_Coroutines.Remove(go);
            m_ActiveCount--;
            GlobalGameObjectPool.Release(go, k_PoolName);
        }

        [Header("2D 空间化")]
        [SerializeField] private float m_PanRange = 10f;      // 摄像机左右各多少单位对应 ±1 pan
        [SerializeField] private float m_MaxHearRange = 30f;  // 超过此距离音量降为 0
        [SerializeField] private AnimationCurve m_VolumeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private void ApplySpatial(AudioSource source, Vector2 worldPos)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 camPos = cam.transform.position;
            float dx = worldPos.x - camPos.x;
            float dist = Vector2.Distance(worldPos, camPos);

            source.panStereo = Mathf.Clamp(dx / Mathf.Max(m_PanRange, 0.001f), -1f, 1f);
            source.volume = m_VolumeFalloff.Evaluate(Mathf.Clamp01(dist / Mathf.Max(m_MaxHearRange, 0.001f)));
        }
        private GameObject m_SourcePrefab;
        private GameObject GetSourcePrefab()
        {
            if (m_SourcePrefab != null) return m_SourcePrefab;

            m_SourcePrefab = new GameObject("OneShot_AudioSource");
            var source = m_SourcePrefab.AddComponent<AudioSource>();
            source.spatialBlend = 0f; // 2D 音效
            source.playOnAwake = false;
            // 不在此处 SetActive(false)：对象池 Release 时会自行禁用，
            // 首次 Instantiate 时若预制体为 disabled，实例也会是 disabled，导致无法播放。
            return m_SourcePrefab;
        }
    }
}
