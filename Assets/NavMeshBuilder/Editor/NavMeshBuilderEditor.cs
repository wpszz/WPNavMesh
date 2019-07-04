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
                    if (rd.gameObject.layer == Layer.sWalkable && rd.gameObject.name != "laya_heightmap")
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
                    if (rd.gameObject.layer == Layer.sWalkable && rd.gameObject.name != "laya_heightmap")
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

        if (GUILayout.Button("Export laya hightmap mesh"))
        {
            List<Vector3> newVerts = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            Dictionary<int, int> indexOldToNew = new Dictionary<int, int>();

            Mesh combineMesh = new Mesh();
            CombineInstance[] cis = new CombineInstance[builder.polys.Count];
            for (int i = 0; i < builder.polys.Count; i++)
            {
                cis[i] = new CombineInstance()
                {
                    mesh = builder.polys[i].meshFilter.sharedMesh,
                    transform = builder.polys[i].transform.localToWorldMatrix,
                };
            }
            combineMesh.CombineMeshes(cis, false, false);
            combineMesh.RecalculateNormals();

            Vector3[] verts = combineMesh.vertices;
            int[] tris = combineMesh.triangles;
            Vector3[] normals = combineMesh.normals;
            Vector3 center = combineMesh.bounds.center;
            for (int i = 0; i < verts.Length; i++)
            {
                int existedIndex = -1;
                for (int j = 0; j < newVerts.Count; j++)
                {
                    if (Vector3.Distance(verts[i], newVerts[j]) < 0.01)
                    {
                        existedIndex = j;
                        break;
                    }
                }
                if (existedIndex < 0)
                {
                    indexOldToNew[i] = newVerts.Count;
                    newVerts.Add(verts[i] - center);
                    newNormals.Add(normals[i]);
                }
                else
                {
                    indexOldToNew[i] = existedIndex;
                }
                //Debug.LogError(verts[i] + "  " + existedIndex);
            }

            GameObject.DestroyImmediate(combineMesh);

            string dir = SceneManager.GetActiveScene().path.Replace(".unity", "");
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            string path = dir + "/laya_heightmap.obj";
            string contents = "";
            for (int i = 0; i < newVerts.Count; i++)
            {
                // obj x-axis is different with unity
                contents += string.Format("v {0:0.000000} {1:0.000000} {2:0.000000}\r\n", -newVerts[i].x, newVerts[i].y, newVerts[i].z);
            }
            for (int i = 0; i < newVerts.Count; i++)
            {
                contents += string.Format("vn {0:0.000000} {1:0.000000} {2:0.000000}\r\n", newNormals[i].x, newNormals[i].y, newNormals[i].z);
            }
            for (int i = 0; i < tris.Length; i += 3)
            {
                // obj x-axis is different with unity
                contents += string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\r\n",
                    indexOldToNew[tris[i + 2]] + 1, indexOldToNew[tris[i + 1]] + 1, indexOldToNew[tris[i]] + 1);
            }
            System.IO.File.WriteAllText(path, contents);

            AssetDatabase.ImportAsset(path);
            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
            mi.importMaterials = false;

            if (System.IO.Directory.Exists(dir + "/Materials"))
            {
                System.IO.Directory.Delete(dir + "/Materials", true);
                System.IO.File.Delete(dir + "/Materials.meta");
            }

            GameObject go = GameObject.Find("laya_heightmap");
            if (go)
                GameObject.DestroyImmediate(go);
            go = new GameObject("laya_heightmap");
            go.transform.position = center;
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.layer = Layer.sWalkable;

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            MeshRenderer render = go.AddComponent<MeshRenderer>();

            Collider collider = meshCollider;

            Bounds aabb = mesh.bounds;
            aabb.center += center;
            //Transform trans = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
            //trans.localScale = aabb.size;
            //trans.position = aabb.center;
            //collider = trans.GetComponent<Collider>();

            Texture2D heightMap = HeightMapBuilder.Create(aabb, collider);
            byte[] heightData = heightMap.EncodeToPNG();
            GameObject.DestroyImmediate(heightMap);
            string imgPath = dir + "/laya_heightmap.png";
            System.IO.File.WriteAllBytes(imgPath, heightData);
            AssetDatabase.ImportAsset(imgPath);
            TextureImporter ti = AssetImporter.GetAtPath(imgPath) as TextureImporter;
            ti.isReadable = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;

            //Texture2D aStarMap = AStarMapBuilder.Create(aabb, collider);
            //byte[] aStarData = aStarMap.EncodeToPNG();
            //GameObject.DestroyImmediate(aStarMap);
            //string astarPath = dir + "/laya_astar.png";
            //System.IO.File.WriteAllBytes(astarPath, aStarData);
            //AssetDatabase.ImportAsset(astarPath);
            //TextureImporter astarTI = AssetImporter.GetAtPath(astarPath) as TextureImporter;
            //astarTI.isReadable = true;
            //astarTI.textureCompression = TextureImporterCompression.Uncompressed;
            //astarTI.npotScale = TextureImporterNPOTScale.None;
            //astarTI.filterMode = FilterMode.Point;
            //astarTI.mipmapEnabled = false;

            string matPath = dir + "/laya_heightmap.mat";
            Shader shader = Shader.Find("LayaAir3D/BlinnPhong");
            Material mat = new Material(shader);
            mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture>(imgPath));
            //mat.SetTexture("_SpecGlossMap", AssetDatabase.LoadAssetAtPath<Texture>(astarPath));
            AssetDatabase.CreateAsset(mat, matPath);

            string heightInfoPath = dir + "/laya_heightinfo.txt";
            string heightInfo = "";
            heightInfo += string.Format("min_x: {0}", aabb.min.x);
            heightInfo += "\r\n";
            heightInfo += string.Format("max_x: {0}", aabb.max.x);
            heightInfo += "\r\n";
            heightInfo += string.Format("min_y: {0}", aabb.min.y);
            heightInfo += "\r\n";
            heightInfo += string.Format("max_y: {0}", aabb.max.y);
            heightInfo += "\r\n";
            heightInfo += string.Format("min_z: {0}", aabb.min.z);
            heightInfo += "\r\n";
            heightInfo += string.Format("max_z: {0}", aabb.max.z);
            heightInfo += "\r\n";
            heightInfo += string.Format("size_x: {0}", aabb.size.x);
            heightInfo += "\r\n";
            heightInfo += string.Format("size_y: {0}", aabb.size.y);
            heightInfo += "\r\n";
            heightInfo += string.Format("size_z: {0}", aabb.size.z);
            heightInfo += "\r\n";
            System.IO.File.WriteAllText(heightInfoPath, heightInfo);

            render.material = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            GameObject goHeightRange = new GameObject("height_range");
            goHeightRange.transform.SetParent(go.transform);
            goHeightRange.transform.localPosition = Vector3.zero;
            goHeightRange.transform.localRotation = Quaternion.identity;
            goHeightRange.transform.localScale = new Vector3(aabb.min.y, aabb.max.y, 0);

            AssetDatabase.Refresh();

            MarkSceneDirty();
        }

        if (GUILayout.Button("Export WP nav mesh data"))
        {
            string json;
            if (WP.NavMeshExport.Serialize(builder, out json))
            {
                string dir = SceneManager.GetActiveScene().path.Replace(".unity", "");
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                string path = dir + "/wp_navmesh.json";

                System.IO.File.WriteAllText(path, json);
                AssetDatabase.ImportAsset(path);
            }
        }
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
