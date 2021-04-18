using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using static VoxelUtility;
public struct MeshingJob : IJobParallelFor
{
    //Marching Cubes variables
    [ReadOnly] public NativeList<MeshTriangle> mcTriangles;
    public bool lowpoly;
    //Mesh variables
    public NativeList<float3>.ParallelWriter vertices, normals;
    public NativeList<float4>.ParallelWriter colors;
    public NativeList<float2>.ParallelWriter uvs;
    public NativeList<int>.ParallelWriter triangles;
    public void Execute(int index)
    {
        MeshTriangle triangle = mcTriangles[index];
        NativeList<int> triangleIndices = new NativeList<int>(3, Allocator.Temp);
        NativeList<float3> triangleVertices = new NativeList<float3>(3, Allocator.Temp);
        NativeList<float3> triangleNormals = new NativeList<float3>(3, Allocator.Temp);
        NativeList<float4> triangleColors = new NativeList<float4>(3, Allocator.Temp);
        NativeList<float2> triangleUVs = new NativeList<float2>(3, Allocator.Temp);
        for (int v = 0; v < 3; v++)
        {
            MeshVertex vertex = triangle[v];
            triangleVertices.Add(vertex.position);
            triangleNormals.Add(lowpoly ? math.float3(1) : vertex.normal);
            triangleColors.Add(math.float4(vertex.color, 1));
            triangleUVs.Add(vertex.uv);
            triangleIndices.Add(index * 3 + v);
        }
        triangles.AddRangeNoResize(triangleIndices);
        vertices.AddRangeNoResize(triangleVertices);
        normals.AddRangeNoResize(triangleNormals);
        colors.AddRangeNoResize(triangleColors);
        uvs.AddRangeNoResize(triangleUVs);
        triangleIndices.Dispose();
        triangleVertices.Dispose();
        triangleNormals.Dispose();
        triangleColors.Dispose();
        triangleUVs.Dispose();
    }
}
