using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static TerrainUtility;
/// <summary>
/// Job that transforms the triangles from the MarchingCubesJob into an actual mesh, with vertex sharing
/// </summary>
public struct VertexMergingJob : IJob
{
    //Marching Cubes variables
    [ReadOnly] public NativeList<MeshTriangle> mcTriangles;
    //Mesh variables
    public NativeList<float3> vertices, normals, colors;
    public NativeList<float2> uvs;
    public NativeList<int> triangles;
    public void Execute()
    {
        int vertexCount = 0;
        NativeHashMap<float3, int> hashmap = new NativeHashMap<float3, int>(triangles.Length * 3, Allocator.Temp);
        NativeList<int> map = new NativeList<int>(Allocator.Temp);
        for (int i = 0; i < mcTriangles.Capacity; i++)
        {
            for (int v = 0; v < 3; v++)
            {
                MeshVertex vert = mcTriangles[i][v];
                if (!hashmap.ContainsKey(vert.position))
                {
                    //First time we generate this vertex
                    hashmap.Add(vert.position, vertices.Length);
                    map.Add(vertices.Length);
                    vertices.Add(vert.position);
                }
                else
                {
                    //Reuse the vertex
                    map.Add(hashmap[vert.position]);
                }
                triangles.Add(map[triangles.Length]);
                vertexCount++;
            }
        }
    }
}
