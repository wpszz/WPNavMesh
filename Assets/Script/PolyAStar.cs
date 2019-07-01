using System.Collections.Generic;
using UnityEngine;

namespace WP
{
    public static class PolyAStar
    {
        private static List<PolyNode> _cacheOpenList;
        private static List<PolyNode> cacheOpenList
        {
            get
            {
                if (_cacheOpenList == null)
                    _cacheOpenList = new List<PolyNode>(64);
                return _cacheOpenList;
            }
        }

        private static int times = 0;

        private static PolyNode PopOpenNodeByMinF()
        {
            int tail = cacheOpenList.Count - 1;
            PolyNode min = cacheOpenList[0];
            int minIndex = 0;
            for (int i = 1; i <= tail; i++)
            {
                if (cacheOpenList[i].F < min.F)
                {
                    min = cacheOpenList[i];
                    minIndex = i;
                }
            }
            cacheOpenList[minIndex] = cacheOpenList[tail];
            cacheOpenList.RemoveAt(tail);
            return min;
        }

        /*
        private static int[] BacktracePolyIndexs(PolyNode endNode)
        {
            int count = 0;
            PolyNode tmp = endNode;
            do
            {
                count++;
                tmp = tmp.parent;
            }
            while (tmp != null);

            int[] polyIndexs = new int[count];
            do
            {
                count--;
                polyIndexs[count] = endNode.index;
                endNode = endNode.parent;
            }
            while (endNode != null);
            return polyIndexs;
        }
        */

        public static bool FindPath(PolyNode nodeStart, PolyNode nodeEnd, PolyNode[] polys)
        {
            cacheOpenList.Clear();
            times++;

            PolyNode node;
            PolyNode neighbor;
            float G;

            node = nodeStart;
            node.opened = true;
            node.closed = false;
            node.parent = null;
            node.parentEdge = -1;
            node.times = times;
            cacheOpenList.Add(node);

            while (cacheOpenList.Count > 0)
            {
                node = PopOpenNodeByMinF();
                node.closed = true;
                if (node == nodeEnd)
                    return true;

                for (int i = node.neighbors.Length - 1; i >= 0; i--)
                {
                    neighbor = node.neighbors[i];
                    if (neighbor.times == times && neighbor.closed)
                        continue;

                    G = node.G + Vector2.Distance(node.center, neighbor.center);

                    if (neighbor.times != times || !neighbor.opened || G < neighbor.G)
                    {
                        if (neighbor.times != times || !neighbor.opened)
                            cacheOpenList.Add(neighbor);

                        neighbor.G = G;
                        neighbor.H = Mathf.Abs(neighbor.center.x - nodeEnd.center.x) + Mathf.Abs(neighbor.center.y - nodeEnd.center.y);
                        neighbor.F = neighbor.G + neighbor.H;
                        neighbor.opened = true;
                        neighbor.closed = false;
                        neighbor.parent = node;
                        neighbor.parentEdge = i;
                        neighbor.times = times;
                    }
                }
            }

            return false;
        }
    }
}