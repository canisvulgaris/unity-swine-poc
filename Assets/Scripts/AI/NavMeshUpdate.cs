using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshUpdate : MonoBehaviour
{
    private NavMeshSurface surface;
    // Start is called before the first frame update
    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        InvokeRepeating("RebuildNavMesh", 3, 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RebuildNavMesh()
    {
        Debug.Log("Rebuilding NavMesh");
        surface.BuildNavMesh();
    }
}
