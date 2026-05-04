using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public static class FishUtils
    {
        public static readonly LayerMask FishLayer = LayerMask.GetMask("Fish");
        public static readonly LayerMask WithoutFishLayer = ~FishLayer;
        private static Collider2D[] s_ColliderCache = new Collider2D[64];

        public static void GetFishInCircle(Vector2 center, float radius, List<Fish> result, bool onlyLiving = true, bool clearResultList = true, Fish ignoreFish = null)
        {
            HashSet<Fish> fishSet = HashSetPool<Fish>.Get();
            fishSet.Clear();
            if (clearResultList)
                result.Clear();
            else
            {
                foreach (var fish in result)
                {
                    fishSet.Add(fish);
                }
            }

            ContactFilter2D filter = new()
            {
                useTriggers = false,
                useLayerMask = true,
                layerMask = FishLayer
            };

            int colliderCount = Physics2D.OverlapCircle(center, radius, filter, s_ColliderCache);
            for (int i = 0; i < colliderCount; i++)
            {
                var collider = s_ColliderCache[i];
                if (collider.TryGetComponent<FishCollider>(out var fishCollider))
                {
                    var fish = fishCollider.Fish;
                    if (!fishSet.Contains(fish) && fish != null
                    && (ignoreFish == null || fish != ignoreFish)
                    && (!onlyLiving || fish.IsLiving))
                    {
                        fishSet.Add(fish);
                        result.Add(fish);
                    }
                }
            }

            HashSetPool<Fish>.Release(fishSet);
        }

        public static (FishAIComponent ai, Fish fish) Create(FishAIComponent aiPrefab, Fish fishPrefab, Vector2 position, Quaternion rotation)
        {
            (FishAIComponent ai, Fish fish) result = new();
            if (aiPrefab == null || fishPrefab == null)
                return result;
            
            result.ai = Object.Instantiate(aiPrefab, position, rotation);
            result.fish = Object.Instantiate(fishPrefab, position, rotation);

            result.ai.SetFishBeforeInit(result.fish);

            return result;
        }
    }
}