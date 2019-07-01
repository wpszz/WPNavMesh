using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(NavMeshPoly), true)]
public class NavMeshPolyEditor : UnityEditor.Editor
{
    NavMeshPoly poly
    {
        get
        {
            return this.target as NavMeshPoly;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh"))
        {
            poly.Refresh();

            MarkSceneDirty();
        }
    }

    void MarkSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
