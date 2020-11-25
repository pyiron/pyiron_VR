using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellboxCollider : MonoBehaviour
{
    public static CellboxCollider Inst;

    public MeshCollider meshCollider;
    
    public MeshFilter colliderMeshFilter;

    private void Awake()
    {
        Inst = this;
    }

    public void SetCollider(Vector3[] data)
    {
        Vector3[] vertices = {Vector3.zero, data[0], data[1], data[2],
            data[0] + data[1], data[0] + data[2], data[1] + data[2], data[0] + data[1] + data[2]};
        int[] newTriangles =
        {
            0, 2, 1, 
            2, 4, 1,
            
            0, 1, 3,
            3, 1, 5,
            
            0, 3, 2,
            3, 6, 2,
            
            7, 6, 5,
            6, 3, 5,
            
            7, 5, 4,
            5, 1, 4,
            
            7, 4, 6,
            6, 4, 2
        };
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = newTriangles;
        
        // Set the collider to the new mesh
        colliderMeshFilter.mesh = newMesh;
        colliderMeshFilter.sharedMesh = newMesh;
        meshCollider.sharedMesh = newMesh;
    }
}
