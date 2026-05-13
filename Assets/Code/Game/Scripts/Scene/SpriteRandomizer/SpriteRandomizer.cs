using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sloane
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRandomizer : MonoBehaviour
    {
        [SerializeField] private SpriteRandomizerPool m_Pool;
        [SerializeField] private bool m_LockDirection = false;
        [SerializeField] private Vector2 m_LockedDirection = Vector2.right;
        [SerializeField, HideInInspector] private bool m_WasLocked = false;
        [SerializeField] private bool m_LockSeed = false;
        [SerializeField] private int m_LockedSeed = 0;
        [SerializeField, HideInInspector] private bool m_WasSeedLocked = false;

#if UNITY_EDITOR
        private Vector3 m_LastPosition;

        private void OnValidate()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
            // 仅在从未锁定 -> 锁定的瞬间捕获方向，之后不再覆盖
            if (m_LockDirection && !m_WasLocked) 
                m_LockedDirection = GetDirectionToNearestTerrain();
            m_WasLocked = m_LockDirection;
            if (m_LockSeed && !m_WasSeedLocked)
                m_LockedSeed = GetPositionSeed();
            m_WasSeedLocked = m_LockSeed;
            Apply();
        }

        private void Update()
        {
            if (Application.isPlaying) return;
            if (transform.position != m_LastPosition)
            {
                m_LastPosition = transform.position;
                Apply();
            }
        }

        [ContextMenu("Apply Sprite")]
        public void Apply()
        {
            if (m_Pool == null) return;

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            Vector2 dir = m_LockDirection ? m_LockedDirection : GetDirectionToNearestTerrain();
            int seed = m_LockSeed ? m_LockedSeed : GetPositionSeed();
            Sprite sprite = m_Pool.GetSprite(dir, seed);
            if (sprite != null && sprite != sr.sprite)
            {
                sr.sprite = sprite;
                EditorUtility.SetDirty(this);
            }
        }

        private Vector2 GetDirectionToNearestTerrain()
        {
#pragma warning disable CS0618
            SDFTerrainObject[] all = FindObjectsByType<SDFTerrainObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#pragma warning restore CS0618
            if (all == null || all.Length == 0)
                return Vector2.right;

            Vector3 selfPos = transform.position;
            SDFTerrainObject nearest = null;
            float minDist = float.MaxValue;

            foreach (var t in all)
            {
                float d = Vector3.SqrMagnitude(t.transform.position - selfPos);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = t;
                }
            }

            if (nearest == null) return Vector2.right;

            Vector2 diff = selfPos - nearest.transform.position;
            return diff == Vector2.zero ? Vector2.right : diff.normalized;
        }

        private int GetPositionSeed()
        {
            Vector3 p = transform.position;
            // 用世界坐标生成确定性种子
            int x = Mathf.RoundToInt(p.x * 16f);
            int y = Mathf.RoundToInt(p.y * 16f);
            int z = Mathf.RoundToInt(p.z * 16f);
            return x * 73856093 ^ y * 19349663 ^ z * 83492791;
        }
#endif
    }
}
