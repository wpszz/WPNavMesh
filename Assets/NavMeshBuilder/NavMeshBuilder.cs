using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class NavMeshBuilder : MonoBehaviour
{
    [SerializeField]
    public float repeatDistance = 0.2f;
    [SerializeField]
    public float viewHeight = 0.01f;
    [SerializeField]
    public List<NavMeshPoly> polys = new List<NavMeshPoly>();

    private void Awake()
    {
        this.gameObject.name = "NavMeshBuilder";
        this.gameObject.tag = "EditorOnly";

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        
    }

    private void OnDrawGizmos()
    {
        
    }

    private void Update()
    {
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
    }

    public void Refresh()
    {
        for (int i = polys.Count - 1; i >= 0; i--)
        {
            if (polys[i])
            {
                polys[i].Refresh();
            }
            else
            {
                polys.RemoveAt(i);
            }
        }
    }
}
