using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WP
{
    [ExecuteInEditMode]
    public class NavMeshTest : MonoBehaviour
    {
        private PolyNode[] polyNodes;
 
        public NavMeshBuilder builder;
        public Transform startTarget;
        public Transform endTarget;

        private float testHeight;
        private Vector2 testMin;
        private Vector2 testMax;

        private PolyNode testPathNodeEnd;
        private Vector2[] testPath;

        void Start()
        {
            InitNavMesh();
        }

        void Update()
        {
            if (polyNodes == null)
                InitNavMesh();

            if (startTarget && endTarget)
                CalculatePath(startTarget.position, endTarget.position);
        }

        void OnDrawGizmos()
        {
            if (polyNodes != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < polyNodes.Length; i++)
                {
                    PolyNode node = polyNodes[i];
                    Vector3 size = Vector3.one * 0.1f;
                    if (testPathNodeEnd != null)
                    {
                        PolyNode tmp = testPathNodeEnd;
                        do
                        {
                            if (tmp == node)
                            {
                                size.y = 3;
                                break;
                            }
                            tmp = tmp.parent;
                        }
                        while (tmp != null);
                    }
                    Gizmos.DrawCube(new Vector3(node.center.x, testHeight, node.center.y), size);
                }

                Gizmos.color = Color.green;
                float width = testMax.x - testMin.x;
                float height = testMax.y - testMin.y;
                int row = 20;
                int column = 20;
                float deltaColumn = width / column;
                float deltaRow = height / row;
                Vector2 p = Vector2.zero;
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < column; j++)
                    {
                        p.x = testMin.x + deltaColumn * j;
                        p.y = testMin.y + deltaRow * i;
                        if (GetInsidePolyIndex(p) >= 0)
                        {
                            Gizmos.DrawRay(new Vector3(p.x, testHeight, p.y), Vector3.up);
                        }
                    }

                if (testPath != null)
                {
                    Gizmos.color = Color.blue;
                    for (int i = 0; i < testPath.Length; i++)
                    {
                        Gizmos.DrawCube(new Vector3(testPath[i].x, testHeight, testPath[i].y), new Vector3(0.2f, 1f, 0.2f));
                        if (i + 1 < testPath.Length)
                        {
                            Gizmos.DrawLine(new Vector3(testPath[i].x, testHeight + 0.1f, testPath[i].y), 
                                new Vector3(testPath[i + 1].x, testHeight + 0.1f, testPath[i + 1].y));
                        }
                    }
                }
            }
        }

        void InitNavMesh()
        {
            if (builder == null)
                return;
            builder.Refresh();

            List<Vector3> allVerts = new List<Vector3>();
            List<int[]> allPolys = new List<int[]>();
            foreach (var poly in builder.polys)
            {
                int[] indexs = new int[poly.vertices.Count];
                for (int i = 0; i < indexs.Length; i++)
                    indexs[i] = allVerts.Count + i;
                allVerts.AddRange(poly.vertices);
                allPolys.Add(indexs);
            }

            List<Vector3> newVerts = new List<Vector3>();
            Dictionary<int, int> indexOldToNew = new Dictionary<int, int>();
            for (int i = 0; i < allVerts.Count; i++)
            {
                int existedIndex = -1;
                for (int j = 0; j < newVerts.Count; j++)
                {
                    if (Vector3.Distance(allVerts[i], newVerts[j]) < 0.01)
                    {
                        existedIndex = j;
                        break;
                    }
                }
                if (existedIndex < 0)
                {
                    indexOldToNew[i] = newVerts.Count;
                    newVerts.Add(allVerts[i]);
                }
                else
                {
                    indexOldToNew[i] = existedIndex;
                }
            }
            foreach (var indexs in allPolys)
            {
                for (int i = 0; i < indexs.Length; i++)
                    indexs[i] = indexOldToNew[indexs[i]];
            }

            int polyCount = allPolys.Count;
            polyNodes = new PolyNode[polyCount];
            for (int i = 0; i < polyCount; i++)
            {
                var indexs = allPolys[i];
                PolyNode node = new PolyNode();
                node.vertexs = new Vector2[indexs.Length];
                for (int j = 0; j < indexs.Length; j++)
                    node.vertexs[j] = new Vector2(newVerts[indexs[j]].x, newVerts[indexs[j]].z);
                node.center = Math.CalculatePolyCenter2D(node.vertexs);
                polyNodes[i] = node;
                node.index = i;

                // only support convex ploygons
                if (!Math.IsConvexPoly(node.vertexs))
                {
                    Debug.LogError(builder.polys[i].name + " is not a convex polygon.");
                    return;
                }

                // make sure clockwise order
                if (!Math.IsClockwise2D(node.vertexs[0], node.vertexs[1], node.vertexs[2]))
                {
                    System.Array.Reverse(node.vertexs);
                    System.Array.Reverse(indexs);
                }
            }
            // find neighbors
            List<PolyNode> neighbors = new List<PolyNode>();
            List<int> neighborEdges = new List<int>();
            for (int i = 0; i < polyCount; i++)
            {
                var indexs = allPolys[i];
                neighbors.Clear();
                neighborEdges.Clear();
                for (int j = 0; j < polyCount; j++)
                {
                    if (i == j)
                        continue;
                    var indexs2 = allPolys[j];
                    int neighborEdge = -1;
                    for (int m = 0; m < indexs.Length; m++)
                    {
                        int e1 = indexs[m];
                        int e2 = indexs[m + 1 >= indexs.Length ? 0 : m + 1];
                        for (int n = 0; n < indexs2.Length; n++)
                        {
                            int e3 = indexs2[n];
                            int e4 = indexs2[n + 1 >= indexs2.Length ? 0 : n + 1];
                            if (e1 == e3 && e2 == e4 || e1 == e4 && e2 == e3)
                            {
                                neighborEdge = m;
                                break;
                            }
                        }
                        if (neighborEdge >= 0)
                            break;
                    }
                    if (neighborEdge >= 0)
                    {
                        neighbors.Add(polyNodes[j]);
                        neighborEdges.Add(neighborEdge);
                    }
                }
                polyNodes[i].neighbors = neighbors.ToArray();
                polyNodes[i].neighborEdges = neighborEdges.ToArray();
            }

            //test
            testHeight = newVerts[0].y;
            testMin = new Vector2(float.MaxValue, float.MaxValue);
            testMax = new Vector2(float.MinValue, float.MinValue);
            foreach (var v in newVerts)
            {
                testMin.x = Mathf.Min(v.x, testMin.x);
                testMin.y = Mathf.Min(v.z, testMin.y);
                testMax.x = Mathf.Max(v.x, testMax.x);
                testMax.y = Mathf.Max(v.z, testMax.y);
            }

            NavMesh navMesh = new NavMesh();
            navMesh.polyNodes = polyNodes;

            string testJson = NavMesh.Serialize(navMesh);
            Debug.Log(testJson);

            navMesh = NavMesh.Deserialize(testJson);
            polyNodes = navMesh.polyNodes;
        }

        //==========================================================
        void CalculatePath(Vector3 startPos, Vector3 endPos)
        {
            testPathNodeEnd = null;
            testPath = null;

            Vector2 startPos2D = new Vector2(startPos.x, startPos.z);
            Vector2 endPos2D = new Vector2(endPos.x, endPos.z);

            int startIndex = GetInsidePolyIndex(startPos2D);
            if (startIndex < 0)
                return;
            int endIndex = GetInsidePolyIndex(endPos2D);
            if (endIndex < 0)
                return;

            PolyNode nodeStart = polyNodes[startIndex];
            PolyNode nodeEnd = polyNodes[endIndex];

            if (!PolyAStar.FindPath(nodeStart, nodeEnd, polyNodes))
                return;

            testPath = PolyCornerPath.FindPath2D(startPos2D, endPos2D, nodeEnd);

            testPathNodeEnd = nodeEnd;
        }

        int GetInsidePolyIndex(Vector2 p)
        {
            int polyCount = polyNodes.Length;
            for (int i = 0; i < polyCount; i++)
            {
                if (Math.IsInsideConvexPoly(p, polyNodes[i].vertexs))
                    return i;
            }
            return -1;
        }
    }
}