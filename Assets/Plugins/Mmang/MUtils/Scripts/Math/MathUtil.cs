using UnityEngine;

namespace Mmang.Util
{
    public static class MathUtil
    {
        public static float GetTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return 0.5f * Vector3.Cross(v1 - v3, v2 - v3).magnitude;
        }

        #region Clamp
        public static float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public static Vector2 Clamp(Vector2 value, float min, float max)
        {
            return new Vector2(Clamp(value.x, min, max), Clamp(value.y, min, max));
        }

        public static Vector3 Clamp(Vector3 value, float min, float max)
        {
            return new Vector3(Clamp(value.x, min, max), Clamp(value.y, min, max), Clamp(value.z, min, max));
        }

        public static Vector4 Clamp(Vector4 value, float min, float max)
        {
            return new Vector4(Clamp(value.x, min, max), Clamp(value.y, min, max), Clamp(value.z, min, max), Clamp(value.w, min, max));
        }


        #endregion

        #region Remap
        public static float Remap(float value, float rawX, float rawY, float targetX, float targetY)
        {
            return targetX + (value - rawX) / (rawY - rawX) * (targetY - targetX);
        }

        public static float ClampRemap(float value, float rawX, float rawY, float targetX, float targetY)
        {
            return Remap(Mathf.Clamp(value, rawX, rawY), rawX, rawY, targetX, targetY);
        }

        #endregion

        #region Saturate
        public static float Saturate(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static Vector2 Saturate(Vector2 value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static Vector3 Saturate(Vector3 value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static Vector4 Saturate(Vector4 value)
        {
            return Clamp(value, 0f, 1f);
        }
        #endregion

        #region 线段
        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

        /// <summary>
        /// 求ab和cd的交点
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="IntrPos"></param>
        /// <returns></returns>
        public static bool GetSegmentsIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 IntrPos)
        {
            //v1×v2=x1y2-y1x2 
            //以线段ab为准，是否c，d在同一侧
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            float abXac = Cross(ab, ac);

            Vector2 ad = d - a;
            float abXad = Cross(ab, ad);

            if (abXac * abXad >= 0)
            {
                IntrPos = Vector2.zero;
                return false;
            }

            //以线段cd为准，是否ab在同一侧
            Vector2 cd = d - c;
            Vector2 ca = a - c;
            Vector2 cb = b - c;

            float cdXca = Cross(cd, ca);
            float cdXcb = Cross(cd, cb);
            if (cdXca * cdXcb >= 0)
            {
                IntrPos = Vector2.zero;
                return false;
            }
            //计算交点坐标  
            float t = Cross(a - c, d - c) / Cross(d - c, b - a);
            float dx = t * (b.x - a.x);
            float dy = t * (b.y - a.y);

            IntrPos = new Vector2() { x = a.x + dx, y = a.y + dy };
            return true;
        }

        /// <summary>
        /// 计算两条直线的交点
        /// </summary>
        /// <param name="p1">直线1的起点</param>
        /// <param name="p2">直线1的终点</param>
        /// <param name="p3">直线2的起点</param>
        /// <param name="p4">直线2的终点</param>
        /// <param name="intersection">输出：交点坐标</param>
        /// <returns>是否有交点</returns>
        public static bool GetLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            // 计算两个向量
            Vector2 b = p2 - p1;
            Vector2 d = p4 - p3;
            Vector2 c = p3 - p1;

            // 计算叉乘（在2D中是行列式）
            float crossDet = b.x * d.y - b.y * d.x;

            // 如果叉乘接近0，说明直线平行（包括共线），无唯一交点
            if (Mathf.Approximately(crossDet, 0))
            {
                return false;
            }

            // 计算参数 t 和 u
            float t = (c.x * d.y - c.y * d.x) / crossDet;
            //float u = (c.x * b.y - c.y * b.x) / crossDet;

            // 计算交点位置： P1 + t * (P2 - P1)
            intersection = p1 + t * b;

            return true;
        }

        #endregion

        #region Vector

        public static Vector2 GetXY(this Vector3 value)
        {
            return new Vector2(value.x, value.y);
        }

        public static Vector2 GetXZ(this Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }

        public static Vector2 GetYZ(this Vector3 value)
        {
            return new Vector2(value.y, value.z);
        }

        public static Vector2 GetZY(this Vector3 value)
        {
            return new Vector2(value.z, value.y);
        }

        public static Vector3 GetX0Z(this Vector3 value)
        {
            return new Vector3(value.x, 0, value.z);
        }

        #endregion

        #region Vector Floor
        public static Vector2 Floor(this Vector2 value)
        {
            return new(Mathf.Floor(value.x), Mathf.Floor(value.y));
        }
        public static Vector3 Floor(this Vector3 value)
        {
            return new(Mathf.Floor(value.x), Mathf.Floor(value.y), Mathf.Floor(value.z));
        }

        public static Vector2Int FloorToInt(this Vector2 value)
        {
            return new(Mathf.FloorToInt(value.x), Mathf.FloorToInt(value.y));
        }
        public static Vector3Int FloorToInt(this Vector3 value)
        {
            return new(Mathf.FloorToInt(value.x), Mathf.FloorToInt(value.y), Mathf.FloorToInt(value.z));
        }

        #endregion

        #region Raycast

        public static Vector3 RaycastPlane(Vector3 start, Vector3 direction, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - start, planeNormal) / Vector3.Dot(direction, planeNormal);
            return d * direction + start;
        }

        #endregion


        #region 几何

        /// <summary>
        /// 两个圆求交点
        /// </summary>
        /// <param name="c1p"></param>
        /// <param name="c2p"></param>
        /// <param name="c1r"></param>
        /// <param name="c2r"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static bool CalculateCircleIntersect(Vector3 c1p, Vector3 c2p, float c1r, float c2r, out Vector3 p0, out Vector3 p1)
        {
            // c1p = circle one position
            // c1r = circle one radius

            var P0 = c1p;
            var P1 = c2p;

            float d, a, h;
            p0 = Vector3.zero;
            p1 = Vector3.zero;

            d = Vector3.Distance(P0, P1);

            if (d > c1r + c2r || d <= 0) return false;
            if (Vector3.Distance(c2p, c1p) + c1r < c2r) return false;
            if (Vector3.Distance(c2p, c1p) + c2r < c1r) return false;

            a = (c1r * c1r - c2r * c2r + d * d) / (2 * d);

            h = Mathf.Sqrt(c1r * c1r - a * a);

            Vector3 P2 = P1 - P0;
            P2 *= a / d;
            P2 += P0;

            float x3, y3, x4, y4;

            x3 = P2.x + h * (P1.y - P0.y) / d;
            y3 = P2.y - h * (P1.x - P0.x) / d;

            x4 = P2.x - h * (P1.y - P0.y) / d;
            y4 = P2.y + h * (P1.x - P0.x) / d; ;

            // out parameters
            p0 = new Vector3(x3, y3, 0);
            p1 = new Vector3(x4, y4, 0);

            return true;
        }

        //求射线矩形交点
        public static bool CalculateRayRectIntersect(Vector2 o, Vector2 dir, Vector2 min, Vector2 max, out Vector2 pNear, out Vector2 pFar)
        {
            pNear = Vector2.zero;
            pFar = Vector2.zero;

            float tmin;
            float tmax;
            if (Mathf.Approximately(dir.x, 0)) //y轴平行
            {
                if (o.x < min.x || o.x > max.x)
                    return false;

                tmin = (min.y - o.y) * dir.y; //dir.y不是1就是-1
                tmax = (max.y - o.y) * dir.y;
                if (tmin > tmax)
                {
                    float temp = tmin;
                    tmin = tmax;
                    tmax = temp;
                }
            }
            else if (Mathf.Approximately(dir.y, 0)) //x轴平行
            {
                if (o.y < min.y || o.y > max.y)
                    return false;

                //用上面那种: *dir.x的方式也是一样的
                if (dir.x > 0)
                {
                    tmin = min.x - o.x;
                    tmax = max.x - o.x;
                }
                else
                {
                    tmin = o.x - max.x;
                    tmax = o.x - min.x;
                }
            }
            else
            {

                float invDirX = 1 / dir.x;
                float tx1 = (min.x - o.x) * invDirX; //x-slab第1个交点
                float tx2 = (max.x - o.x) * invDirX; //x-slab第2个交点
                if (tx1 > tx2) //射线在x方向上从右往左时
                {
                    float temp = tx1;
                    tx1 = tx2;
                    tx2 = temp;
                }

                float invDirY = 1 / dir.y;
                float ty1 = (min.y - o.y) * invDirY; //y-slab第1个交点
                float ty2 = (max.y - o.y) * invDirY; //y-slab第2个交点
                if (ty1 > ty2) //射线在y方向上从上往下时
                {
                    float temp = ty1;
                    ty1 = ty2;
                    ty2 = temp;
                }

                //共线线段无重叠：max(两线段的min端点) > min(两线段的max端点)
                tmin = Mathf.Max(tx1, ty1);
                tmax = Mathf.Min(tx2, ty2);
                if (tmin > tmax) //线段没相交
                    return false;
            }

            if (tmax < 0) //射线起点不在AABB内
                return false;

            pFar = o + dir * tmax;
            if (tmin < 0)
                pNear = pFar;
            else
                pNear = o + dir * tmin;
            return true;
        }

        /// <summary>
        /// 求点到线的距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float PointToLineDistance(Vector3 p1, Vector3 p2, Vector3 target)
        {
            // p1->p2的向量
            Vector3 p1_2 = p2 - p1;
            // p1->target向量
            Vector3 p1_target = target - p1;
            // 计算投影p1->f
            Vector3 p1f = Vector3.Project(p1_target, p1_2);
            // 加上p1坐标 然后计算距离
            float distance = Vector3.Distance(target, p1f + p1);
            return distance;
        }

        #endregion


        #region Other

        /// <summary>
        /// 返回大于等于输入值的最小的2的幂
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int ComputePowerTwo(int input)
        {
            const int MAXIMUM = 1 << 30;
            if (input >= MAXIMUM)
            {
                return MAXIMUM;
            }
            int temp = input - 1;
            temp |= temp >> 1;
            temp |= temp >> 2;
            temp |= temp >> 4;
            temp |= temp >> 8;
            temp |= temp >> 16;
            return (temp < 0) ? 1 : temp + 1;
        }

        #endregion
    }
}