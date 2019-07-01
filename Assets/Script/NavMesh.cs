﻿using UnityEngine;
using System.Collections.Generic;

namespace WP
{
    public class NavMesh
    {
        public PolyNode[] polyNodes;

        public int GetInsidePolyIndex(Vector2 p)
        {
            int polyCount = polyNodes.Length;
            for (int i = 0; i < polyCount; i++)
            {
                if (Math.IsInsideConvexPoly(p, polyNodes[i].vertexs))
                    return i;
            }
            return -1;
        }

        //=======================================================

        public static string Serialize(NavMesh navMesh)
        {
            List<object> roots = new List<object>();
            for(int i = 0; i < navMesh.polyNodes.Length; i++)
            {
                PolyNode polyNode = navMesh.polyNodes[i];
                Dictionary<string, object> jsonNode = new Dictionary<string, object>();
                roots.Add(jsonNode);

                List<object> vertexs = new List<object>();
                jsonNode["vertexs"] = vertexs;
                for(int j = 0; j < polyNode.vertexs.Length; j++)
                {
                    Vector2 vert = polyNode.vertexs[j];
                    vertexs.Add(new float[] { vert.x, vert.y });
                }

                jsonNode["center"] = new float[] { polyNode.center.x, polyNode.center.y };

                List<int> neighbors = new List<int>();
                jsonNode["neighbors"] = neighbors;
                for (int j = 0; j < polyNode.neighbors.Length; j++)
                {
                    neighbors.Add(polyNode.neighbors[j].index);
                }

                List<int> neighborEdges = new List<int>();
                jsonNode["neighborEdges"] = neighborEdges;
                for (int j = 0; j < polyNode.neighborEdges.Length; j++)
                {
                    neighborEdges.Add(polyNode.neighborEdges[j]);
                }
            }
            return MiniJSON.Json.Serialize(roots);
        }

        public static NavMesh Deserialize(string json)
        {
            NavMesh navMesh = new NavMesh();
            List<object> roots = MiniJSON.Json.Deserialize(json) as List<object>;
            int nodeCount = roots.Count;
            navMesh.polyNodes = new PolyNode[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                PolyNode polyNode = new PolyNode();
                navMesh.polyNodes[i] = polyNode;
                Dictionary<string, object> jsonNode = roots[i] as Dictionary<string, object>;

                List<object> vertexs = jsonNode["vertexs"] as List<object>;
                int vertCount = vertexs.Count;
                polyNode.vertexs = new Vector2[vertCount];
                for(int j = 0; j < vertCount; j++)
                {
                    List<object> vert = vertexs[j] as List<object>;
                    polyNode.vertexs[j] = new Vector2((float)(double)vert[0], (float)(double)vert[1]);
                }

                List<object> center = jsonNode["center"] as List<object>;
                polyNode.center = new Vector2((float)(double)center[0], (float)(double)center[1]);

                List<object> neighborEdges = jsonNode["neighborEdges"] as List<object>;
                int neighborEdgesCount = neighborEdges.Count;
                polyNode.neighborEdges = new int[neighborEdgesCount];
                for (int j = 0; j < neighborEdgesCount; j++)
                {
                    polyNode.neighborEdges[j] = (int)(long)neighborEdges[j];
                }
            }
            for (int i = 0; i < nodeCount; i++)
            {
                PolyNode polyNode = navMesh.polyNodes[i];
                Dictionary<string, object> jsonNode = roots[i] as Dictionary<string, object>;

                List<object> neighbors = jsonNode["neighbors"] as List<object>;
                int neighborsCount = neighbors.Count;
                polyNode.neighbors = new PolyNode[neighborsCount];
                for (int j = 0; j < neighborsCount; j++)
                {
                    polyNode.neighbors[j] = navMesh.polyNodes[(int)(long)neighbors[j]];
                }
            }
            return navMesh;
        }

    }
}