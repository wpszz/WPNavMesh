using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class NavMeshPoly : MonoBehaviour
{
    [SerializeField]
    public Color color = new Color(1f, 0f, 1f, 0.5f);

    [SerializeField]
    public List<Vector3> vertices = new List<Vector3>();

    public MeshFilter meshFilter
    {
        get
        {
            return this.GetComponent<MeshFilter>();
        }
    }

    public MeshCollider meshCollider
    {
        get
        {
            return this.GetComponent<MeshCollider>();
        }
    }

    public MeshRenderer meshRenderer
    {
        get
        {
            return this.GetComponent<MeshRenderer>();
        }
    }

    public NavMeshBuilder navMeshBuilder
    {
        get
        {
            if (this.transform.parent)
                return this.transform.parent.GetComponent<NavMeshBuilder>();
            return null;
        }
    }

    private void Awake()
    {
        this.meshRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        this.Clear();
    }

    private void OnDrawGizmos()
    {
        NavMeshBuilder builder = navMeshBuilder;
        float radius = builder ? builder.repeatDistance : 0.1f;
        float height = builder ? builder.viewHeight : 0.01f;

        Vector3 vHeight = new Vector3(0, height, 0);

        if (meshFilter.sharedMesh)
        {
            Gizmos.color = color;
            Gizmos.DrawMesh(meshFilter.sharedMesh, vHeight);
        }

        if (vertices.Count > 1)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Vector3 v1 = vertices[i];
                Vector3 v2 = vertices[i + 1];
                Gizmos.DrawLine(v1, v2);
            }
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(vertices[i] + vHeight, radius);
        }
    }

    private void Update()
    {
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
    }

    private void Clear()
    {
        if (meshFilter.sharedMesh)
        {
            GameObject.DestroyImmediate(meshFilter.sharedMesh);
        }
        meshFilter.sharedMesh = null;
        meshCollider.sharedMesh = null;
    }

    public void Refresh()
    {
        Clear();

        // 计算本节点偏移
        Vector3 offset = transform.localPosition;
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] += offset;
        }
        transform.localPosition = Vector3.zero;

        if (vertices.Count > 2)
        {
            Mesh mesh = new Mesh();

            List<int> triangles = new List<int>();

            for (int i = 1; i < vertices.Count - 1; i++)
            {
                PushClockwiseTriangle(vertices, 0, i, i + 1, triangles);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }

    private void PushClockwiseTriangle(List<Vector3> vertices, int a, int b, int c, List<int> triangles)
    {
        if (IsClockwise(vertices[a], vertices[b], vertices[c]))
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }
        else
        {
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(a);
        }
    }

    private bool IsClockwise(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 v21 = v2 - v1;
        Vector3 v32 = v3 - v2;

        Vector3 cross = Vector3.Cross(v21, v32).normalized;

        //Debug.LogWarning(cross);

        return cross.y > 0;
    }
}
