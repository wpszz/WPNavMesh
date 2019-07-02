using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
namespace WP
{
    public class NavMeshExport
    {
        public static bool Serialize(NavMeshBuilder builder, out string json)
        {
            builder.Refresh();

            json = "";

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
            PolyNode[] polyNodes = new PolyNode[polyCount];
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
                    Debug.LogError("Polygon " + builder.polys[i].name + " is not a convex polygon.", builder.polys[i]);
                    return false;
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

            NavMesh navMesh = new NavMesh();
            navMesh.polyNodes = polyNodes;

            json = NavMesh.Serialize(navMesh);
            Debug.Log(json);

            return true;
        }
    }
}