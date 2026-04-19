using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mmang.Util
{
    public static class RandomUtil
    {

        #region 颜色

        public static Color RandomColorH(float s, float v)
        {
            float h = Random.value;
            return Color.HSVToRGB(h, s, v);
        }

        #endregion

        #region 概率随机
        /// <summary>
        /// 根据概率随机返回true
        /// </summary>
        /// <param name="successProbability"></param>
        /// <returns></returns>
        public static bool RandomSuccess(float successProbability)
        {
            if (successProbability <= 0f)
                return false;
            return Random.Range(0f, 1f) <= successProbability;
        }

        #endregion

        #region 简单功能
        /// <summary>
        /// 获取列表中随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandomListElement<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return default;
            int rand = Random.Range(0, list.Count);
            return list[rand];
        }

        /// <summary>
        /// 返回x ~ y的随机数(最大值不用+1)
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static int GetRandomValueInRange(Vector2Int range)
        {
            return Random.Range(range.x, range.y + 1);
        }

        /// <summary>
        /// 返回x ~ y的随机数
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static float GetRandomValueInRange(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }

        #endregion

        #region IWeight拓展
        
        public static float GetTotalWeight<T>(this IEnumerable<T> weights) where T : IWeighted
        {
            float totalWeight = 0f;
            foreach (var w in weights)
                totalWeight += w.Weight;
            return totalWeight;
        }

        #endregion

        #region 权重抽取

        /// <summary>
        /// 根据权重返回随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WeightElements"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>(this List<T> WeightElements) where T : IWeighted
        {
            if (WeightElements == null || WeightElements.Count == 0)
                return default;
            float totalWeight = WeightElements.GetTotalWeight();
            float rand = Random.Range(0f, totalWeight);
            float curWeight = 0f;
            foreach (var element in WeightElements)
            {
                curWeight += element.Weight;
                if (rand <= curWeight)
                    return element;
            }

            return default;
        }

        /// <summary>
        /// 根据权重返回随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WeightElements"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>(this List<T> WeightElements, out int index) where T : IWeighted
        {
            index = -1;
            if (WeightElements == null || WeightElements.Count == 0)
                return default;
            float totalWeight = WeightElements.GetTotalWeight();
            float rand = Random.Range(0f, totalWeight);
            float curWeight = 0f;
            index = 0;
            foreach (var element in WeightElements)
            {
                curWeight += element.Weight;
                if (rand <= curWeight)
                    return element;
                index++;
            }
            index = -1;
            return default;
        }

        /// <summary>
        /// 根据权重返回随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WeightElements"></param>
        /// <param name="edge">边界，例如传入3则只抽取前三个元素</param>
        /// <returns></returns>
        public static T GetRandomElement<T>(this List<T> WeightElements, int edge) where T : IWeighted
        {
            if (WeightElements == null || WeightElements.Count == 0 || edge <= 0)
                return default;
            float totalWeight = 0f;
            int count = Mathf.Min(WeightElements.Count, edge);
            for (int i = 0; i < count; i++)
                totalWeight += WeightElements[i].Weight;
            float rand = Random.Range(0f, totalWeight);
            float curWeight = 0f;
            foreach (var element in WeightElements)
            {
                curWeight += element.Weight;
                if (rand <= curWeight)
                    return element;
            }

            return default;
        }

        /// <summary>
        /// 根据权重返回随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WeightElements"></param>
        /// <param name="edge">边界，例如传入3则只抽取前三个元素</param>
        /// <returns></returns>
        public static T GetRandomElement<T>(this List<T> WeightElements, int edge, out int index) where T : IWeighted
        {
            index = -1;
            if (WeightElements == null || WeightElements.Count == 0 || edge <= 0)
                return default;
            float totalWeight = 0f;
            int count = Mathf.Min(WeightElements.Count, edge);
            for (int i = 0; i < count; i++)
                totalWeight += WeightElements[i].Weight;
            float rand = Random.Range(0f, totalWeight);
            float curWeight = 0f;
            index = 0;
            foreach (var element in WeightElements)
            {
                curWeight += element.Weight;
                if (rand <= curWeight)
                    return element;
                index++;
            }
            index = -1;
            return default;
        }

        /// <summary>
        /// 根据权重返回多个随机元素(不重复)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WeightElements"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetRandomElements<T>(this List<T> WeightElements, int count) where T : IWeighted
        {
            if (WeightElements == null || WeightElements.Count == 0)
                return default;
            
            List<T> selectedElements = new();
            float totalWeight = WeightElements.GetTotalWeight();

            float rand;
            float curWeight;
            for (int i = count - 1; i >= 0; i--)
            {
                rand = Random.Range(0f, totalWeight);
                curWeight = 0f;
                foreach (var element in WeightElements)
                {
                    if (selectedElements.Contains(element))
                        continue;
                    curWeight += element.Weight;
                    if (rand <= curWeight)
                    {
                        selectedElements.Add(element);
                        totalWeight -= element.Weight;
                        break;
                    }
                }
            }

            return selectedElements;
        }
        #endregion

        #region 价值抽取

        /// <summary>
        /// 随机获取物体，直到总价值大于等于给定价值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="objectValues"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public static List<T> GetRandomElementsAttainValue<T>(List<T> objects, List<float> objectValues, float targetValue)
        {
            List<T> result = new();
            if (objects == null || objectValues == null || objects.Count == 0 || objectValues.Count == 0)
                return result;
            
            int count = Mathf.Min(objects.Count, objectValues.Count);
            float curValue = 0f;
            while (curValue < targetValue)
            {
                int rand = Random.Range(0, count);
                result.Add(objects[rand]);
                curValue += objectValues[rand];
            }
            return result;
        }

        /// <summary>
        /// 随机获取物体，直到总价值大于等于给定价值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public static List<T> GetRandomElementsAttainValue<T>(this List<T> objects, float targetValue) where T : IValued
        {
            List<T> result = new();
            if (objects == null || objects.Count == 0)
                return result;
            
            int count = objects.Count;
            float curValue = 0f;
            while (curValue < targetValue)
            {
                int rand = Random.Range(0, count);
                result.Add(objects[rand]);
                curValue += objects[rand].Value;
            }
            return result;
        }

        /// <summary>
        /// 随机获取物体，总价值不会超过给定价值，并且会尽力逼近给定价值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public static List<T> GetRandomElementsWithinValue<T>(this List<T> objects, float targetValue) where T : IValued
        {
            List<T> result = new();
            if (objects == null || objects.Count == 0 || targetValue <= 0)
                return result;
            objects.Sort((x, y) => x.Value.CompareTo(y.Value));

            int edge = objects.Count;
            float remainValue = targetValue;
            while (remainValue > 0f)
            {
                int rand = Random.Range(0, edge);
                while ((objects[rand].Value > remainValue || objects[rand].Value <= 0) && rand >= 0)
                {
                    edge = rand;
                    rand = Random.Range(0, edge);
                }
                if (rand == -1)
                    return result;
                
                
                remainValue -= objects[rand].Value;
                result.Add(objects[rand]);
            }

            return result;
        }

        #endregion



        #region 权重价值混合抽取

        /// <summary>
        /// 按照权重随机获取物体，总价值不会超过给定价值，并且会尽力逼近给定价值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public static List<T> GetRandomWeightedElementsWithinValue<T>(this List<T> objects, float targetValue) where T : IValued, IWeighted
        {
            List<T> result = new();
            if (objects == null || objects.Count == 0 || targetValue <= 0)
                return result;
            objects.Sort((x, y) => x.Value.CompareTo(y.Value));

            int edge = objects.Count;
            float remainValue = targetValue;
            T selected;
            while (remainValue > 0f)
            {
                selected = objects.GetRandomElement(edge, out int index);
                //Debug.Log(selected + "价值: " + selected.Value + " Edge:" + edge);
                while (selected != null && index >= 0 && (selected.Value > remainValue || selected.Value <= 0))
                {
                    edge = index;
                    selected = objects.GetRandomElement(edge, out int _index);
                    index = _index;
                }
                if (index == -1)
                    return result;
                
                
                remainValue -= selected.Value;
                result.Add(selected);
            }

            return result;
        }

        #endregion



        #region 随机坐标
        /// <summary>
        /// 返回三角形区域中的随机点
        /// </summary>
        /// <param name="a">点A</param>
        /// <param name="b">点B</param>
        /// <param name="c">点C</param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            float rand1 = Random.value;
            float rand2 = Random.value;
            float sqrRand1 = Mathf.Sqrt(rand1);
            return (1 - sqrRand1) * a + sqrRand1 * (1 - rand2) * b + sqrRand1 * rand2 * c;
        }

        /// <summary>
        /// 返回三角形区域中的随机点
        /// </summary>
        /// <param name="a">点A</param>
        /// <param name="b">点B</param>
        /// <param name="c">点C</param>
        /// <returns></returns>
        public static Vector3 GetRandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            float rand1 = Random.value;
            float rand2 = Random.value;
            float sqrRand1 = Mathf.Sqrt(rand1);
            return (1 - sqrRand1) * a + sqrRand1 * (1 - rand2) * b + sqrRand1 * rand2 * c;
        }

        /// <summary>
        /// 返回三角形区域中的随机点(应该不太准确，未验证)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static Vector3 GetRandomPointInTriangleFast(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float r = Random.value;
            float s = Random.value;

            if (r + s >= 1)
            {
                r = 1 - r;
                s = 1 - s;
            }

            return v1 + r * (v2 - v1) + s * (v3 - v1);
        }

        /// <summary>
        /// 返回圆形区域中的随机点
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInCircle(Vector2 center, float radius)
        {
            return center + radius * Random.insideUnitCircle;
        }

        public static Vector2 GetRandomPointOnCircle(Vector2 center, float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            return radius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) + center;
        }

        /// <summary>
        /// 返回包围盒中的随机点
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInBounds(Bounds bounds)
        {
            return (Vector2)bounds.center + new Vector2(Random.Range(-bounds.extents.x, bounds.extents.x), Random.Range(-bounds.extents.y, bounds.extents.y));
        }

        /// <summary>
        /// 返回碰撞体区域中的随机点
        /// </summary>
        /// <param name="collider2D"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInCollider(Collider2D collider2D)
        {
            if (collider2D is BoxCollider2D boxCollider2D)
                return GetRandomPointInBoxCollider(boxCollider2D);
            if (collider2D is CircleCollider2D circleCollider2D)
                return GetRandomPointInCircleCollider(circleCollider2D);
            return GetRandomPointOverlapCollider(collider2D);
        }

        /// <summary>
        /// 返回BoxCollider区域中的随机点
        /// </summary>
        /// <param name="collider2D"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInBoxCollider(BoxCollider2D collider2D)
        {
            Vector2 boxExtents = collider2D.size / 2f;
            return new Vector2(Random.Range(-boxExtents.x, boxExtents.x), Random.Range(-boxExtents.y, boxExtents.y)) + (Vector2)collider2D.transform.position;
        }

        /// <summary>
        /// 返回圆形碰撞体区域中的随机点
        /// </summary>
        /// <param name="collider2D"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInCircleCollider(CircleCollider2D collider2D)
        {
            return collider2D.radius * Random.insideUnitCircle + collider2D.offset + (Vector2)collider2D.transform.position;
        }

        /// <summary>
        /// 通过随机生成点并判断返回碰撞体范围内的点
        /// </summary>
        /// <param name="collider2D"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointOverlapCollider(Collider2D collider2D)
        {
            int TryCount = 100;
            while (TryCount-- > 0)
            {
                Vector2 point = GetRandomPointInBounds(collider2D.bounds);
                if (collider2D.OverlapPoint(point))
                    return point;
            }
            return collider2D.transform.position;
        }
        #endregion

        #region 随机序列
        /// <summary>
        /// 返回随机排序的 0 ~ count-1 的数组
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] GetRandomSortIndexArray(int count)
        {
            if (count < 0)
                return null;
            int[] result = new int[count];
            for (int i = result.Length - 1; i >= 0; i--)
                result[i] = i;
            for (int i = result.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (result[randomIndex], result[i]) = (result[i], result[randomIndex]);
            }
            return result;
        }

        /// <summary>
        /// 返回随机排序后的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T[] GetCopyRandomSortArray<T>(IEnumerable<T> data)
        {
            var result = data.ToArray();
            for (int i = result.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (result[randomIndex], result[i]) = (result[i], result[randomIndex]);
            }
            return result;
        }

        #endregion
    }
}