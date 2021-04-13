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
        for (int i = 0; i < mcTriangles.Capacity; i++)
        {
            if (!hashmap.ContainsKey(vertex))
            {
                //First time we generate this vertex
                hashmap.Add(vertex, vertices.Count);
                map.Add(vertices.Count);
                vertices.Add(vertex);
                Voxel a = localVoxels[i + edgesToCornerIndices[tri]];
                Voxel b = localVoxels[i + edgesToCornerIndices2[tri]];
                normals.Add(lowpoly ? Vector3.one : Vector3.Lerp(a.normal, b.normal, lerpValue));
                Vector3 color = Vector3.Lerp(a.color, b.color, lerpValue);
                colors.Add(new Color(color.x, color.y, color.z, 1.0f));
                currentUV.x = Mathf.Lerp(a.smoothness, b.smoothness, lerpValue);
                currentUV.y = Mathf.Lerp(a.metallic, b.metallic, lerpValue);
                uvs.Add(currentUV);
            }
            else
            {
                //Reuse the vertex
                map.Add(hashmap[vertex]);
            }
            triangles.Add(map[triangles.Count]);
            vertexCount++;
        }
    }
}
