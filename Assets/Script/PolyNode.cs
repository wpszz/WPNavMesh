using UnityEngine;

namespace WP
{
    public class PolyNode
    {
        public Vector2[] vertexs;
        public Vector2 center;
        public PolyNode[] neighbors;
        public int[] neighborEdges;

        // astar node info
        public float F;
        public float G;
        public float H;
        public bool opened;
        public bool closed;
        public PolyNode parent;
        public int parentEdge;
        public int times;
    }
}