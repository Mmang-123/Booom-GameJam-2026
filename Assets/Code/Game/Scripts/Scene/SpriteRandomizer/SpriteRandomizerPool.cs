using System;
using UnityEngine;

namespace Sloane
{
    [CreateAssetMenu(fileName = "SpriteRandomizerPool", menuName = "Sloane/Sprite Randomizer Pool")]
    public class SpriteRandomizerPool : ScriptableObject
    {
        public enum Direction
        {
            Right      = 0, // E
            UpperRight = 1, // NE
            Up         = 2, // N
            UpperLeft  = 3, // NW
            Left       = 4, // W
            LowerLeft  = 5, // SW
            Down       = 6, // S
            LowerRight = 7, // SE
        }

        [Serializable]
        public class DirectionPool
        {
            public Direction Direction;
            public Sprite[] Sprites;
        }

        [SerializeField] private DirectionPool[] m_Pools = new DirectionPool[8];

        public Sprite GetSprite(Vector2 direction, int seed)
        {
            if (m_Pools == null || m_Pools.Length == 0) return null;

            Direction dir = AngleToDirection(direction);
            foreach (var pool in m_Pools)
            {
                if (pool.Direction == dir && pool.Sprites != null && pool.Sprites.Length > 0)
                {
                    var rng = new System.Random(seed);
                    return pool.Sprites[rng.Next(pool.Sprites.Length)];
                }
            }
            return null;
        }

        private static Direction AngleToDirection(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // 归一化到 [0, 360)
            if (angle < 0) angle += 360f;
            // 每个方向占 45 度，从 East 开始，逆时针排列
            // 加 22.5 使得 0 度正好是 Right 的中心
            int index = Mathf.RoundToInt(angle / 45f) % 8;
            return (Direction)index;
        }
    }
}
