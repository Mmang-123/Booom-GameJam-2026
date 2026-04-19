using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Util
{
    public static class GameObjectUtil
    {
        public static bool IsInLayerMask(this GameObject obj, LayerMask layerMask)
        {
            // 根据Layer数值进行移位获得用于运算的Mask值
            int objLayerMask = 1 << obj.layer;
            return (layerMask.value & objLayerMask) > 0;
        }

        public static bool ContainsLayer(this LayerMask layerMask, int layer)
        {
            int objLayerMask = 1 << layer;
            return (layerMask.value & objLayerMask) > 0;
        }

        /// <summary>
        /// 获取包括自身在内的所有子物体
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<GameObject> GetUnfoldGameObjects(this GameObject root)
        {
            static void GetChildren(Transform root, List<GameObject> list)
            {
                int childCount = root.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var child = root.GetChild(i);
                    list.Add(child.gameObject);
                    GetChildren(child, list);
                }
            }
            if (root is null)
                return null;
            List<GameObject> result = new() { root };
            GetChildren(root.transform, result);

            return result;
        }


    }

    public static class ColliderUtil
    {
        public readonly static LayerMask EverythingLayer = ~0;

        public static RaycastHit GetNearestCollider(Vector3 origin, RaycastHit[] hits, int count)
        {
            if (count <= 0)
                return default;
            int resultIdx = 0;
            float nearestDis = float.MaxValue;
            for (int i = count - 1; i >= 0; i--)
            {
                float newDis = Vector3.Distance(origin, hits[i].point);
                if (newDis < nearestDis)
                {
                    nearestDis = newDis;
                    resultIdx = i;
                }
            }
            return hits[resultIdx];
        }
    }
}