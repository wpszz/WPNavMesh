using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WP
{
    [ExecuteInEditMode]
    public class NavMeshTest : MonoBehaviour
    {
        public NavMeshBuilder builder;
        public Transform startTarget;
        public Transform endTarget;
        [Range(-10f, 10f)]
        public float offsetHeight = 0f;
        [Range(20, 100)]
        public int splitCount = 20;

        NavMesh navMesh;

        private float testHeight;
        private Vector2 testMin;
        private Vector2 testMax;

        private PolyNode testPathNodeEnd;
        private Vector2[] testPath;

        private Vector2 testStartClosest;
        private Vector2 testEndClosest;
        private float testStartClosestDis;
        private float testEndClosestDis;

        void Start()
        {
            InitNavMesh();
        }

        void Update()
        {
            if (navMesh == null)
                InitNavMesh();

            if (startTarget && endTarget)
                CalculatePath(startTarget.position, endTarget.position);
        }

        void OnDrawGizmos()
        {
            if (navMesh != null)
            {
                float showHeight = testHeight + offsetHeight;

                Gizmos.color = Color.red;
                for (int i = 0; i < navMesh.polyNodes.Length; i++)
                {
                    PolyNode node = navMesh.polyNodes[i];
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
                    Gizmos.DrawCube(new Vector3(node.center.x, showHeight, node.center.y), size);
                }

                Gizmos.color = Color.green;
                float width = testMax.x - testMin.x;
                float height = testMax.y - testMin.y;
                int row = splitCount;
                int column = splitCount;
                float deltaColumn = width / column;
                float deltaRow = height / row;
                Vector2 p = Vector2.zero;
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < column; j++)
                    {
                        p.x = testMin.x + deltaColumn * j;
                        p.y = testMin.y + deltaRow * i;
                        if (navMesh.GetInsidePolyIndex(p) >= 0)
                        {
                            Gizmos.DrawRay(new Vector3(p.x, showHeight, p.y), Vector3.up);
                        }
                    }

                if (testPath != null)
                {
                    Gizmos.color = Color.blue;
                    for (int i = 0; i < testPath.Length; i++)
                    {
                        Gizmos.DrawCube(new Vector3(testPath[i].x, showHeight, testPath[i].y), new Vector3(0.2f, 1f, 0.2f));
                        if (i + 1 < testPath.Length)
                        {
                            Gizmos.DrawLine(new Vector3(testPath[i].x, showHeight + 0.1f, testPath[i].y), 
                                new Vector3(testPath[i + 1].x, showHeight + 0.1f, testPath[i + 1].y));
                        }
                    }
                }

                if (testStartClosestDis > 0)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(new Vector3(testStartClosest.x, showHeight + 0.1f, testStartClosest.y), new Vector3(0.2f, 10, 0.2f));
                }
                if (testEndClosestDis > 0)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawCube(new Vector3(testEndClosest.x, showHeight + 0.1f, testEndClosest.y), new Vector3(0.2f, 10, 0.2f));
                }
            }
        }

        void InitNavMesh()
        {
            if (builder == null)
                return;

            string testJson;
            if (!NavMeshExport.Serialize(builder, out testJson))
                return;

            navMesh = NavMesh.Deserialize(testJson);

            testHeight = builder.polys[0].vertices[0].y;
            testMin = new Vector2(float.MaxValue, float.MaxValue);
            testMax = new Vector2(float.MinValue, float.MinValue);
            foreach (var node in navMesh.polyNodes)
                foreach (var v in node.vertexs)
                    {
                    testMin.x = Mathf.Min(v.x, testMin.x);
                    testMin.y = Mathf.Min(v.y, testMin.y);
                    testMax.x = Mathf.Max(v.x, testMax.x);
                    testMax.y = Mathf.Max(v.y, testMax.y);
                }
        }

        //==========================================================
        void CalculatePath(Vector3 startPos, Vector3 endPos)
        {
            testPathNodeEnd = null;
            testPath = null;
            testStartClosestDis = -1;
            testEndClosestDis = -1;

            Vector2 startPos2D = new Vector2(startPos.x, startPos.z);
            Vector2 endPos2D = new Vector2(endPos.x, endPos.z);

            int startIndex = navMesh.GetInsidePolyIndex(startPos2D);
            if (startIndex < 0)
            {
                testStartClosestDis = navMesh.FindClosestEdge2D(startPos2D, out testStartClosest);
            }
            int endIndex = navMesh.GetInsidePolyIndex(endPos2D);
            if (endIndex < 0)
            {
                testEndClosestDis = navMesh.FindClosestEdge2D(endPos2D, out testEndClosest);
            }
            if (testStartClosestDis > 0 || testEndClosestDis > 0)
                return;

            PolyNode nodeStart = navMesh.polyNodes[startIndex];
            PolyNode nodeEnd = navMesh.polyNodes[endIndex];

            if (!PolyAStar.FindPath(nodeStart, nodeEnd, navMesh.polyNodes))
                return;

            testPath = PolyCornerPath.FindPath2D(startPos2D, endPos2D, nodeEnd);

            testPathNodeEnd = nodeEnd;
        }
    }
}