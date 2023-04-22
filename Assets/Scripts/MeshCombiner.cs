using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    void Start()
    {
        CombineMeshes();
    }

    void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        MeshFilter parentMeshFilter = gameObject.AddComponent<MeshFilter>();
        parentMeshFilter.mesh = new Mesh();
        parentMeshFilter.mesh.CombineMeshes(combine);

        MeshRenderer parentMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        parentMeshRenderer.sharedMaterial = meshFilters[0].gameObject.GetComponent<MeshRenderer>().sharedMaterial;

        // Add a Mesh Collider and set the combined mesh as the sharedMesh for the collider
        MeshCollider parentMeshCollider = gameObject.AddComponent<MeshCollider>();
        parentMeshCollider.sharedMesh = parentMeshFilter.mesh;
        parentMeshCollider.convex = true; // Set to true if you want the collider to be able to collide with other mesh colliders

        gameObject.SetActive(true);
    }
}
