using System.Collections.Generic;
using UnityEngine;

namespace WP
{
    public static class PolyCornerPath
    {
        public static void FindPath2D(Vector2 startPos2D, Vector2 endPos2D, PolyNode nodeAStar, List<Vector2> path)
        {
            path.Clear();

            PolyNode origin = nodeAStar;
            PolyNode prev = origin;
            PolyNode current = prev.parent;
            PolyNode next = null;
            PolyNode nodeP1 = current;
            PolyNode nodeP2 = current;
            Vector2 p0 = endPos2D;
            Vector2 p1 = Vector2.zero;
            Vector2 p2 = Vector2.zero;
            Vector2 p3 = Vector2.zero;
            Vector2 p4 = Vector2.zero;
            int e1, e2, e3, e4;
            bool flag1, flag2, flag3, flag4;

            if (current != null)
            {
                e2 = current.neighborEdges[prev.parentEdge];
                e1 = e2 + 1 >= current.vertexs.Length ? 0 : e2 + 1;
                p1 = current.vertexs[e1];
                p2 = current.vertexs[e2];

                while (true)
                {
                    next = current.parent;
                    if (next == null)
                    {
                        flag1 = Math.IsClockwiseMargin2D(p0, p1, startPos2D);
                        flag2 = Math.IsClockwiseMargin2D(p0, startPos2D, p2);

                        if (flag1 && flag2)
                        {
                            // startPos2D is inside of the ∠p0_p1_p2
                            break;
                        }

                        if (!flag1)
                        {
                            // p1 is the next corner
                            path.Add(p0);
                            p0 = p1;
                            origin = nodeP1;
                        }

                        if (!flag2)
                        {
                            // p2 is the next corner
                            path.Add(p0);
                            p0 = p2;
                            origin = nodeP2;
                        }

                        //reset vector checking from current node
                        prev = origin;
                        current = prev.parent;
                        if (current == null)
                        {
                            // origin is the last node
                            break;
                        }
                        e2 = current.neighborEdges[prev.parentEdge];
                        e1 = e2 + 1 >= current.vertexs.Length ? 0 : e2 + 1;
                        p1 = current.vertexs[e1];
                        p2 = current.vertexs[e2];
                        continue;
                    }

                    // compare next points in the edge
                    e4 = next.neighborEdges[current.parentEdge];
                    e3 = e4 + 1 >= next.vertexs.Length ? 0 : e4 + 1;
                    p3 = next.vertexs[e3];
                    p4 = next.vertexs[e4];

                    flag1 = Math.IsClockwiseMargin2D(p0, p1, p3);
                    flag2 = Math.IsClockwiseMargin2D(p0, p3, p2);
                    flag3 = Math.IsClockwiseMargin2D(p0, p1, p4);
                    flag4 = Math.IsClockwiseMargin2D(p0, p4, p2);

                    if (flag1 && flag2)
                    {
                        // p3 is inside of the ∠p0_p1_p2
                        p1 = p3;
                        nodeP1 = current;
                    }

                    if (flag3 && flag4)
                    {
                        // p4 is inside of the ∠p0_p1_p2
                        p2 = p4;
                        nodeP2 = current;
                    }

                    if (!flag2 && !flag4)
                    {
                        // p2 is the next corner
                        path.Add(p0);
                        p0 = p2;
                        origin = nodeP2;
                    }

                    if (!flag1 && !flag3)
                    {
                        // p1 is the next corner
                        path.Add(p0);
                        p0 = p1;
                        origin = nodeP1;
                    }

                    if (!flag2 && !flag4 || !flag1 && !flag3)
                    {
                        // reset vector checking from current node
                        prev = origin;
                        current = prev.parent;
                        e2 = current.neighborEdges[prev.parentEdge];
                        e1 = e2 + 1 >= current.vertexs.Length ? 0 : e2 + 1;
                        p1 = current.vertexs[e1];
                        p2 = current.vertexs[e2];
                    }
                    else
                    {
                        // move to next
                        prev = current;
                        current = next;
                    }
                }
            }

            path.Add(p0);
            path.Add(startPos2D);

            // reverse because nodeAStar is the end of the node path.
            path.Reverse();
        }

        public static Vector2[] FindPath2D(Vector2 startPos2D, Vector2 endPos2D, PolyNode nodeAStar)
        {
            List<Vector2> path = new List<Vector2>();

            FindPath2D(startPos2D, endPos2D, nodeAStar, path);

            return path.ToArray();
        }
    }
}