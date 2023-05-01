using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshUpdate : MonoBehaviour
{
    private NavMeshSurface surface;
    public float refreshTime = 1;
    // Start is called before the first frame update
    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        InvokeRepeating("RebuildNavMesh", refreshTime, refreshTime);
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
