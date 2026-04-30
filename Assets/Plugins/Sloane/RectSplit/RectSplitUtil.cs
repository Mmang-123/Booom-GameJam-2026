using System.Collections.Generic;
using UnityEngine;

namespace Sloane
{
    public static class RectUtil
    {
        public static void SliceRectX(List<Rect> rectSet, float xCut)
        {
            int setLength = rectSet.Count;

            for (int i = 0; i < setLength; i++)
            {
                Rect original = rectSet[i];

                if ((xCut < Mathf.Min(original.xMin, original.xMax)) || (xCut > Mathf.Max(original.xMin, original.xMax))) continue;

                Rect leftRect = new Rect(original.xMin, original.yMin, xCut - original.xMin, original.height);
                Rect rightRect = new Rect(xCut, original.yMin, original.xMax - xCut, original.height);

                rectSet[i] = leftRect;
                rectSet.Add(rightRect);
            }
        }

        public static void SliceRectY(List<Rect> rectSet, float yCut)
        {
            int setLength = rectSet.Count;

            for (int i = 0; i < setLength; i++)
            {
                Rect original = rectSet[i];

                if ((yCut < Mathf.Min(original.yMin, original.yMax)) || (yCut > Mathf.Max(original.yMin, original.yMax))) continue;

                Rect downRect = new Rect(original.xMin, original.yMin, original.width, yCut - original.yMin);
                Rect upRect = new Rect(original.xMin, yCut, original.width, original.yMax - yCut);

                rectSet[i] = downRect;
                rectSet.Add(upRect);
            }
        }

        public static void SliceRectWithRect(List<Rect> rectSet, Rect rect)
        {
            SliceRectX(rectSet, rect.xMin);
            SliceRectX(rectSet, rect.xMax);
            SliceRectY(rectSet, rect.yMin);
            SliceRectY(rectSet, rect.yMax);
        }

        public static bool Contains(this Rect rect, Rect other)
        {
            return rect.ContainsWithin(new Vector2(other.xMin, other.yMin)) && rect.ContainsWithin(new Vector2(other.xMax, other.yMin)) && rect.ContainsWithin(new Vector2(other.xMin, other.yMax)) && rect.ContainsWithin(new Vector2(other.xMax, other.yMax));
        }

        public static bool ContainsWithin(this Rect rect, Vector2 point)
        {
            return point.x >= Mathf.Min(rect.xMin, rect.xMax) &&
                point.x <= Mathf.Max(rect.xMin, rect.xMax) &&
                point.y >= Mathf.Min(rect.yMin, rect.yMax) &&
                point.y <= Mathf.Max(rect.yMin, rect.yMax);
        }

        public static Rect Merge(this Rect a, Rect b)
        {
            float xMin = Mathf.Min(a.xMin, b.xMin);
            float xMax = Mathf.Max(a.xMax, b.xMax);
            float yMin = Mathf.Min(a.yMin, b.yMin);
            float yMax = Mathf.Max(a.yMax, b.yMax);

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}