using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Jedjoud.VoxelWorld;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
using static Unity.Mathematics.math;
/// <summary>
/// Job that transforms the triangles from the MarchingCubesJob into an actual mesh, with vertex sharing
/// </summary>
namespace Jedjoud.VoxelWorld
{
    [BurstCompile]
    public struct VertexMergingJob : IJob
    {
        //Marching Cubes variables
        [ReadOnly] public NativeList<MeshTriangle> mcTriangles;
        //Mesh variables
        public NativeList<float3> vertices, normals;
        public NativeList<float4> colors;
        public NativeList<float2> uvs;
        public NativeList<int> triangles;
        public void Execute()
        {
            int vertexCount = 0;
            NativeHashMap<float3, int> hashmap = new NativeHashMap<float3, int>(triangles.Length * 3, Allocator.Temp);
            NativeList<int> map = new NativeList<int>(triangles.Length * 3, Allocator.Temp);
            for (int i = 0; i < mcTriangles.Length; i++)
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
                        colors.Add(float4(vert.color, 1));
                        normals.Add(vert.normal);
                        uvs.Add(vert.uv);
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
            hashmap.Dispose();
            map.Dispose();
        }
    }
}