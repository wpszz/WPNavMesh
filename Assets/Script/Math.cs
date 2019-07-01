using UnityEngine;

namespace WP
{
    public static class Math
    {
        public static bool IsClockwise2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // check cross sign
            return CrossMagnitude2D(p1, p2, p3) < 0;
        }

        public static bool IsClockwiseMargin2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // check cross sign
            return CrossMagnitude2D(p1, p2, p3) <= 0;
        }

        public static bool IsConvexPoly(Vector2[] vertexs)
        {
            int len = vertexs.Length;
            bool flag = IsClockwise2D(vertexs[len - 1], vertexs[0], vertexs[1]);
            if (IsClockwise2D(vertexs[len - 2], vertexs[len - 1], vertexs[0]) != flag)
                return false;
            for (int i = 2; i < len; i++)
            {
                if (IsClockwise2D(vertexs[i - 2], vertexs[i - 1], vertexs[i]) != flag)
                    return false;
            }
            return true;
        }

        public static bool IsInsideConvexPoly(Vector2 p, Vector2[] vertexs)
        {
            int len = vertexs.Length;
            bool flag = IsClockwise2D(vertexs[len - 1], vertexs[0], p);
            for (int i = 1; i < len; i++)
            {
                if (IsClockwise2D(vertexs[i - 1], vertexs[i], p) != flag)
                    return false;
            }
            return true;
        }

        public static Vector2 CalculatePolyCenter2D(Vector2[] vertexs)
        {
            int len = vertexs.Length;
            float sumX = 0;
            float sumY = 0;
            float sumArea = 0;
            float area = 0;
            Vector2 v0 = vertexs[0];
            Vector2 v1;
            Vector2 v2;
            for (int i = 2; i < len; i++)
            {
                v1 = vertexs[i - 1];
                v2 = vertexs[i];
                area = CalculateTriangleArea2D(v0, v1, v2);
                sumArea += area;
                sumX += (v0.x + v1.x + v2.x) * area;
                sumY += (v0.y + v1.y + v2.y) * area;
            }
            float delta = 1 / (sumArea * 3);
            return new Vector2(sumX * delta, sumY * delta);
        }

        public static float CalculateTriangleArea2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float area = CrossMagnitude2D(p1, p2, p3);
            return area * 0.5f;
        }

        public static float CrossMagnitude2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // signed magnitude of cross(p2 - p1, p3 - p1)
            return p1.x * p2.y + p2.x * p3.y + p3.x * p1.y - p1.x * p3.y - p2.x * p1.y - p3.x * p2.y;
        }

        public static bool CalculateSegmentIntersect2D(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 ret)
        {
            ret = new Vector2();

            float x1 = p1.x;
            float y1 = p1.y;
            float x2 = p2.x;
            float y2 = p2.y;
            float x3 = p3.x;
            float y3 = p3.y;
            float x4 = p4.x;
            float y4 = p4.y;

            // a1x + b1y + c1 = 0
            float a1 = y2 - y1;
            float b1 = x1 - x2;
            float c1 = x2 * y1 - x1 * y2;

            // a2x + b2y + c2 = 0
            float a2 = y4 - y3;
            float b2 = x3 - x4;
            float c2 = x4 * y3 - x3 * y4;

            float delta = a1 * b2 - a2 * b1;
            if (delta <= float.Epsilon && delta >= -float.Epsilon)
                return false;

            ret.x = (b1 * c2 - b2 * c1) / delta;
            ret.y = (a2 * c1 - a1 * c2) / delta;
            return true;
        }
    }
}