using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class Layer
{
    public const int sWalkable = 10;

    public const int mkWalkable = 1 << sWalkable;
}

[CustomEditor(typeof(NavMeshBuilder), true)]
public class NavMeshBuilderEditor : UnityEditor.Editor
{
    [MenuItem("GameObject/Create Nav Mesh Builder")]
    public static void CreateNavMeshBuilder()
    {
        GameObject go = new GameObject("NavMeshBuilder");
        go.AddComponent<NavMeshBuilder>();
        Selection.activeGameObject = go;
    }

    NavMeshBuilder builder
    {
        get
        {
            return this.target as NavMeshBuilder;
        }
    }

    NavMeshPoly tempPoly;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh all polygons"))
        {
            builder.Refresh();

            MarkSceneDirty();
        }
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear NavMesh"))
        {
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
        }
        if (GUILayout.Button("Bake NavMesh"))
        {
            BakeNavMesh();
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear walkable colliders"))
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Renderer rd in go.GetComponentsInChildren<Renderer>(true))
                {
                    if (rd.gameObject.layer == Layer.sWalkable)
                    {
                        Collider cld = rd.GetComponent<Collider>();
                        if (cld)
                            GameObject.DestroyImmediate(cld);
                    }
                }
            }
            MarkSceneDirty();
        }
        if (GUILayout.Button("Attach walkable colliders"))
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Renderer rd in go.GetComponentsInChildren<Renderer>(true))
                {
                    if (rd.gameObject.layer == Layer.sWalkable)
                    {
                        Collider cld = rd.GetComponent<Collider>();
                        if (!cld)
                        {
                            cld = rd.gameObject.AddComponent<MeshCollider>();
                        }
                    }
                }
            }
            MarkSceneDirty();
        }
        GUILayout.EndHorizontal();
    }

    private void OnDestroy()
    {
        if (tempPoly)
            GameObject.DestroyImmediate(tempPoly.gameObject);
    }

    void OnSceneGUI()
    {
        if (Event.current.alt)
        {
            if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
            {
                Vector3 pickPos;
                if (PickWalkablePos(out pickPos))
                {
                    // 存储到临时面
                    //Debug.LogWarning("Pick hit: " + pickPos);
                    PutPosToTempPoly(pickPos);
                }
                else
                {
                    Debug.LogWarning("Invalid pick");
                }
                //Event.current.Use();
            }
        }
        else
        {
            // 存档
            if (tempPoly)
            {
                if (tempPoly.vertices.Count > 2)
                {
                    tempPoly.Refresh();
                    builder.polys.Add(tempPoly);
                }
                else
                {
                    GameObject.DestroyImmediate(tempPoly.gameObject);
                }
                tempPoly = null;
            }
        }
    }

    void PutPosToTempPoly(Vector3 pos)
    {
        float repeatDistance = builder.repeatDistance;

        bool repeat = false;
        foreach (var poly in builder.polys)
        {
            if (!poly)
                continue;
            foreach (var pos2 in poly.vertices)
            {
                if (Vector3.Distance(pos, pos2) < repeatDistance)
                {
                    pos = pos2;
                    repeat = true;
                    break;
                }
            }
        }

        if (!repeat && tempPoly)
        {
            foreach (var pos2 in tempPoly.vertices)
            {
                if (Vector3.Distance(pos, pos2) < repeatDistance)
                {
                    pos = pos2;
                    repeat = true;
                    break;
                }
            }
        }

        if (tempPoly && tempPoly.vertices.Count > 0)
        {
            if (Vector3.Distance(pos, tempPoly.vertices[tempPoly.vertices.Count - 1]) < repeatDistance ||
                Vector3.Distance(pos, tempPoly.vertices[0]) < repeatDistance)
                return;
        }

        if (!tempPoly)
        {
            tempPoly = new GameObject(builder.polys.Count.ToString()).AddComponent<NavMeshPoly>();
            tempPoly.transform.SetParent(builder.transform);
            tempPoly.transform.localPosition = Vector3.zero;
            tempPoly.transform.localRotation = Quaternion.identity;
            tempPoly.transform.localScale = Vector3.one;
            GameObjectUtility.SetStaticEditorFlags(tempPoly.gameObject, StaticEditorFlags.NavigationStatic);
        }
        tempPoly.vertices.Add(pos);
        tempPoly.Refresh();
        MarkSceneDirty();
    }

    bool PickWalkablePos(out Vector3 pos)
    {
        pos = Vector3.zero;
        SceneView sceneView = SceneView.lastActiveSceneView;
        Camera cam = sceneView.camera;
        Vector3 mpos = Event.current.mousePosition;
        mpos.y = cam.pixelHeight - mpos.y;
        Ray ray = cam.ScreenPointToRay(mpos);
        //Debug.LogWarning(mpos + "  " + cam.pixelHeight);
        //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 50);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 50, Layer.mkWalkable))
        {
            pos = hit.point;
            return true;
        }
        return false;
    }

    public void BakeNavMesh()
    {
        foreach (var poly in builder.polys)
        {
            if (!poly)
                continue;
            poly.meshRenderer.enabled = true;
        }

        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        foreach (var poly in builder.polys)
        {
            if (!poly)
                continue;
            poly.meshRenderer.enabled = false;
        }
    }

    void MarkSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
