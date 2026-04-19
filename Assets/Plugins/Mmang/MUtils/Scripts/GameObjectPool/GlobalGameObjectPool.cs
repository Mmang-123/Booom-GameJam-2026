using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Util
{
    public class GlobalGameObjectPool : SingletonMono<GlobalGameObjectPool>
    {
        public class Pool
        {
            public int Hash;
            public List<GameObject> GameObjects { get; } = new();
        
            public Pool(int hash)
            {
                Hash = hash;
            }
        }
        public Dictionary<int, Pool> PoolMap { get; } = new();

        public static Pool GetPool(string name)
        {
            if (Instance == null)
                return null;
            int hash = name.GetHashCode();
            if (Instance.PoolMap.ContainsKey(hash))
                return Instance.PoolMap[hash];
            Instance.PoolMap.Add(hash, new Pool(hash));
            return Instance.PoolMap[hash];
        }

        public static Pool GetPool(int hash)
        {
            if (Instance == null)
                return null;
            if (Instance.PoolMap.ContainsKey(hash))
                return Instance.PoolMap[hash];
            Instance.PoolMap.Add(hash, new Pool(hash));
            return Instance.PoolMap[hash];
        }

        public static void Release(Component component, string poolName)
            => Release(component.gameObject, poolName.GetHashCode());
        public static void Release(GameObject gameObject, string poolName)
            => Release(gameObject, poolName.GetHashCode());
        public static void Release(Component component, int poolHash)
            => Release(component.gameObject, poolHash);
        public static void Release(GameObject gameObject, int poolHash)
        {
            var pool = GetPool(poolHash);
            pool.GameObjects.Add(gameObject);
            gameObject.SetActive(false);
            gameObject.transform.SetParent(Instance.transform);
        }

        public static GameObject GetGameObject(string poolName, Vector3 position, Quaternion rotation, MonoBehaviour prefab = null)
            => GetGameObject(poolName.GetHashCode(), position, rotation, prefab);
        public static GameObject GetGameObject(int poolHash, Vector3 position, Quaternion rotation, MonoBehaviour prefab = null)
        {
            var pool = GetPool(poolHash);
            if (pool.GameObjects.Count > 0)
            {
                var go = pool.GameObjects[^1];
                pool.GameObjects.RemoveAt(pool.GameObjects.Count - 1);
                go.transform.SetParent(null, true);
                go.transform.position = position;
                go.transform.rotation = rotation;
                go.SetActive(true);
                return go;
            }

            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab, position, rotation, null);
                return go.gameObject;
            }

            return null;
        }

        public static GameObject GetGameObject(string poolName, Vector3 position, Quaternion rotation, GameObject prefab = null)
            => GetGameObject(poolName.GetHashCode(), position, rotation, prefab);
        public static GameObject GetGameObject(int poolHash, Vector3 position, Quaternion rotation, GameObject prefab = null)
        {
            var pool = GetPool(poolHash);
            if (pool.GameObjects.Count > 0)
            {
                var go = pool.GameObjects[^1];
                pool.GameObjects.RemoveAt(pool.GameObjects.Count - 1);
                go.transform.SetParent(null, true);
                go.transform.position = position;
                go.transform.rotation = rotation;
                go.SetActive(true);
                return go;
            }

            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab, position, rotation, null);
                return go;
            }

            return null;
        }


        public static T Get<T>(string poolName, Vector3 position, T prefab) where T : MonoBehaviour
            => Get<T>(poolName.GetHashCode(), position, prefab);
        public static T Get<T>(int poolHash, Vector3 position, T prefab) where T : MonoBehaviour
            => Get<T>(poolHash, position, Quaternion.identity, prefab);


        public static T Get<T>(string poolName, Vector3 position, Quaternion rotation, T prefab) where T : MonoBehaviour
            => Get<T>(poolName.GetHashCode(), position, rotation, prefab);
        public static T Get<T>(int poolHash, Vector3 position, Quaternion rotation, T prefab) where T : MonoBehaviour
        {
            var go = GetGameObject(poolHash, position, rotation, prefab);
            if (go != null)
                return go.GetComponent<T>();
            return null;
        }
    }
}