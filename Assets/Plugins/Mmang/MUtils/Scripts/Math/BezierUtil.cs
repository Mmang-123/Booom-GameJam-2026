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
}