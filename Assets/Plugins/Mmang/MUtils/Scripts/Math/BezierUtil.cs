using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Util
{
    public static class BezierUtil
    {
        public static Vector3 LineBezier(Vector3 point1, Vector3 point2, float t)
        {
            return point1 + (point2 - point1) * t;
        }

        public static Vector3 QuardaticBezier(Vector3 point1, Vector3 point2, Vector3 point3, float t)
        {
            Vector3 aa = point1 + (point2 - point1) * t;
            Vector3 bb = point2 + (point3 - point2) * t;
            return aa + (bb - aa) * t;
        }

        public static Vector3 CubicBezier(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4, float t)
        {
            Vector3 aa = point1 + (point2 - point1) * t;
            Vector3 bb = point2 + (point3 - point2) * t;
            Vector3 cc = point3 + (point4 - point3) * t;

            Vector3 aaa = aa + (bb - aa) * t;
            Vector3 bbb = bb + (cc - bb) * t;
            return aaa + (bbb - aaa) * t;
        }

    }

    public class BezierPath
    {
        private Vector3 p0, p1, p2, p3;
        private List<(float distance, float t)> arcLengthTable = new List<(float, float)>();
        private float totalLength;
        public float TotalLength => totalLength;

        // 1. 初始化，建立查表
        public void Initialize(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples = 100)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            arcLengthTable.Clear();
            float currentDist = 0;
            Vector3 lastPoint = p0;

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 point = CalculateCubicBezier(t, p0, p1, p2, p3);
                currentDist += Vector3.Distance(lastPoint, point);
                arcLengthTable.Add((currentDist, t));
                lastPoint = point;
            }
            totalLength = currentDist;
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            distance = Mathf.Clamp(distance, 0, totalLength);
            float t = GetTFromDistance(distance);
            return CalculateCubicBezier(t, p0, p1, p2, p3);
        }

        private float GetTFromDistance(float dist)
        {
            for (int i = 0; i < arcLengthTable.Count - 1; i++)
            {
                if (dist >= arcLengthTable[i].distance && dist <= arcLengthTable[i + 1].distance)
                {
                    // 区间内插值
                    float segmentDist = arcLengthTable[i + 1].distance - arcLengthTable[i].distance;
                    float fraction = (dist - arcLengthTable[i].distance) / segmentDist;
                    return Mathf.Lerp(arcLengthTable[i].t, arcLengthTable[i + 1].t, fraction);
                }
            }
            return 1f;
        }

        private Vector3 CalculateCubicBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }
    }
}