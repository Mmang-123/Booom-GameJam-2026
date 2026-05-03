using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    /// <summary>
    /// 残影对象池，全局单例。首次使用时若场景中不存在则自动创建。
    /// </summary>
    public class AfterimagePool : MonoBehaviour
    {
        [SerializeField] private int m_DefaultCapacity = 10;
        [SerializeField] private int m_MaxSize         = 50;

        private static AfterimagePool s_Instance;

        public static AfterimagePool Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("[AfterimagePool]");
                    DontDestroyOnLoad(go);
                    s_Instance = go.AddComponent<AfterimagePool>();
                    s_Instance.InitPool();
                }
                return s_Instance;
            }
        }

        private ObjectPool<AfterimageInstance> m_Pool;

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPool();
        }

        private void InitPool()
        {
            if (m_Pool != null) return;
            m_Pool = new ObjectPool<AfterimageInstance>(
                createFunc:       CreateInstance,
                actionOnGet:      OnGet,
                actionOnRelease:  OnRelease,
                actionOnDestroy:  OnDestroyInstance,
                collectionCheck:  false,
                defaultCapacity:  m_DefaultCapacity,
                maxSize:          m_MaxSize
            );
        }

        // ── 池操作 ─────────────────────────────────────────────

        private AfterimageInstance CreateInstance()
        {
            var go = new GameObject("Afterimage");
            go.transform.SetParent(transform, false);
            go.SetActive(false);
            return go.AddComponent<AfterimageInstance>();
        }

        private void OnGet(AfterimageInstance instance)
            => instance.gameObject.SetActive(true);

        private void OnRelease(AfterimageInstance instance)
        {
            instance.OnReturnToPool();
            instance.gameObject.SetActive(false);
        }

        private void OnDestroyInstance(AfterimageInstance instance)
            => Destroy(instance.gameObject);

        // ── 公开接口 ────────────────────────────────────────────

        /// <summary>
        /// 在 source 位置生成一个残影，以 tintColor 着色，持续 duration 秒后淡出并回收。
        /// </summary>
        public static void Spawn(Transform source, Color tintColor, float duration, Material overrideMaterial = null)
        {
            var pool = Instance;
            pool.InitPool();
            var instance = pool.m_Pool.Get();
            instance.Init(source, tintColor, duration, pool.m_Pool, overrideMaterial);
        }
    }
}
