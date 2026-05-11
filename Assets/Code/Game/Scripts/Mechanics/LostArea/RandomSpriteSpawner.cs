using UnityEngine;
using System.Collections.Generic; // 引入 List 需要的命名空间

public class RandomSpriteSpawner : MonoBehaviour
{
    [Header("1. 路径生成设置")]
    [Tooltip("放入组成路径的精灵图预制体池")]
    public GameObject[] pathSpritePool;
    
    [Tooltip("组成路径的精灵图数量")]
    public int pathSpriteCount = 10;

    [Header("1.1 路径坐标参数 (相对于 Spawner 物体)")]
    [Tooltip("路径起点坐标 (X, Y)")]
    public Vector2 pathStart = new Vector2(-5f, -5f);
    
    [Tooltip("路径终点坐标 (X, Y)")]
    public Vector2 pathEnd = new Vector2(5f, 5f);
    
    [Tooltip("曲线控制点坐标 (调节弧度) (X, Y)")]
    public Vector2 controlPoint = new Vector2(-5f, 5f);

    [Header("2. 原本的随机生成设置")]
    [Tooltip("放入在区域内随机生成的精灵图预制体池")]
    public GameObject[] randomPrefabPool;
    
    [Tooltip("期望生成的总随机数量")]
    public int randomSpawnCount = 10;

    [Header("2.1 空间与距离设置")]
    [Tooltip("随机生成区域的宽和高 (中心点为当前物体位置)")]
    public Vector2 spawnArea = new Vector2(10f, 10f);
    
    [Tooltip("所有精灵图(路径和随机之间)的最小间距 (圆柱体检测)")]
    public float minDistance = 1.5f;

    [Tooltip("每次随机生成最多尝试寻找空位的次数，防止死循环卡死")]
    public int maxSpawnAttempts = 30;

    // 用来记录所有已经成功生成的坐标点 (包括路径和随机点)
    private List<Vector2> spawnedPositions = new List<Vector2>();

    // 获取路径坐标的世界坐标版本 (Gizmos 绘制和插值使用)
    private Vector2 WorldPathStart => (Vector2)transform.position + pathStart;
    private Vector2 WorldPathEnd => (Vector2)transform.position + pathEnd;
    private Vector2 WorldControlPoint => (Vector2)transform.position + controlPoint;

    [ContextMenu("Generate")]
    public void InitializeLevel()
    {
        // 每次生成前，必须彻底清空所有旧的坐标历史记录
        spawnedPositions.Clear();

        // **步骤一：生成路径**
        GenerateCurvedPath();

        // **步骤二：执行原本的随机生成逻辑**
        // (此时 spawnedPositions 已经包含了路径坐标，随机逻辑会自动避开)
        GenerateRandomSprites();
    }

    // ================== 核心功能一：路径生成 ==================
    private void GenerateCurvedPath()
    {
        if (pathSpritePool == null || pathSpritePool.Length == 0 || pathSpriteCount <= 1)
        {
            Debug.LogWarning("路径生成池为空，或路径点数量过少。请赋值。");
            return;
        }

        // 使用二次贝塞尔曲线公式进行插值： P(t) = (1-t)^2 P0 + 2(1-t)t P1 + t^2 P2 for t in [0, 1]
        // 这里： P0 是起点，P2 是终点，P1 是控制点。

        for (int i = 0; i < pathSpriteCount; i++)
        {
            // 计算插值参数 t，从 0 到 1 (0 代表起点，1 代表终点)
            float t = (float)i / (pathSpriteCount - 1);

            // 计算该点的二次贝塞尔曲线位置
            Vector2 pathPosition = CalculateQuadraticBezierPoint(t, WorldPathStart, WorldControlPoint, WorldPathEnd);

            // 将该位置记录到 spawnedPositions，这样随机逻辑就能识别并避开它
            spawnedPositions.Add(pathPosition);

            // 实例化路径精灵图 (可选：也可以不记录路径点，如果路径精灵图只是装饰，但我强烈建议记录)
            SpawnSingle(pathPosition, pathSpritePool);
        }
    }

    // 计算二次贝塞尔曲线上的一个点
    private Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector2 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }

    // ================== 核心功能二：原本的防重叠随机生成 ==================
    private void GenerateRandomSprites()
    {
        if (randomPrefabPool == null || randomPrefabPool.Length == 0 || randomSpawnCount <= 0)
        {
            Debug.LogWarning("随机生成池为空或数量过低。");
            return;
        }

        int actualSpawnedCount = 0;

        // 注意：spawnedPositions 此刻不为空，它包含了路径点。
        int initialPathCount = spawnedPositions.Count;

        for (int i = 0; i < randomSpawnCount; i++)
        {
            Vector2? validPosition = FindValidRandomPosition();

            if (validPosition.HasValue)
            {
                SpawnSingle(validPosition.Value, randomPrefabPool);
                spawnedPositions.Add(validPosition.Value); // 记录随机点
                actualSpawnedCount++;
            }
            else
            {
                Debug.LogWarning($"空间不足！试图生成 {randomSpawnCount} 个随机精灵，但只避开了路径和旧随机点成功放下了 {actualSpawnedCount} 个。");
                break;
            }
        }
    }

    // 寻址随机有效点 (重用之前的防重叠检测逻辑)
    private Vector2? FindValidRandomPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // 随机一个点
            float randomX = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
            float randomY = Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f);
            Vector2 testPosition = new Vector2(transform.position.x + randomX, transform.position.y + randomY);

            // 检查这个点是否和所有已生成的点 (包含路径点和已生成的随机点) 冲突
            bool isOverlapping = false;
            foreach (Vector2 existingPos in spawnedPositions)
            {
                if (Vector2.Distance(testPosition, existingPos) < minDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
            {
                return testPosition;
            }
        }
        return null;
    }

    // 通用的生成单个物体的函数
    private void SpawnSingle(Vector2 position, GameObject[] pool)
    {
        int randomIndex = Random.Range(0, pool.Length);
        GameObject selectedPrefab = pool[randomIndex];
        Instantiate(selectedPrefab, position, Quaternion.identity, transform);
    }

    // ================== Gizmos 绘制 (非常重要，用于可视化调整坐标) ==================
    private void OnDrawGizmosSelected()
    {
        // 绘制生成区域
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0f));

        // 绘制最小距离 (参考圆)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position, minDistance / 2f); 

        // 绘制路径坐标点 (蓝色点)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(WorldPathStart, 0.2f);
        Gizmos.DrawWireSphere(WorldPathEnd, 0.2f);
        Gizmos.DrawWireSphere(WorldControlPoint, 0.2f);

        // 绘制路径曲线辅助线 (黑色虚线，仅当坐标非空时)
        Gizmos.color = Color.black;
        Gizmos.DrawLine(WorldPathStart, WorldControlPoint);
        Gizmos.DrawLine(WorldControlPoint, WorldPathEnd);

        // 可视化贝塞尔曲线路径 (绘制蓝色实线)
        if (pathSpriteCount > 1)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 1f); // 深蓝色
            Vector2 prevPoint = WorldPathStart;
            for (int i = 1; i < pathSpriteCount; i++)
            {
                float t = (float)i / (pathSpriteCount - 1);
                Vector2 currentPoint = CalculateQuadraticBezierPoint(t, WorldPathStart, WorldControlPoint, WorldPathEnd);
                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }
        }
    }
}