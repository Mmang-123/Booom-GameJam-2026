using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public static class FishUtils
    {
        public static readonly LayerMask FishLayer = LayerMask.GetMask("Fish");
        private static Collider2D[] s_ColliderCache = new Collider2D[64];

        public static void GetFishInCircle(Vector2 center, float radius, List<Fish> result, bool onlyLiving = true, Fish ignoreFish = null)
        {
            result.Clear();
            ContactFilter2D filter = new()
            {
                useTriggers = false,
                useLayerMask = true,
                layerMask = FishLayer
            };

            int colliderCount = Physics2D.OverlapCircle(center, radius, filter, s_ColliderCache);
            HashSet<Fish> fishSet = HashSetPool<Fish>.Get();
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
                    }
                }
            }

            result.AddRange(fishSet);
        }
    }
}