using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using static TerrainUtility;
/// <summary>
/// Create the skirts for the terrain in the x, y and z axis
/// </summary>
public struct SkirtsJobX : IJobParallelFor
{
    //Marching cubes skirts variables
    private const int resolution = VoxelWorld.resolution;
    public int slicePoint;
    public float reductionFactorChunkScaled;
    public float chunkSize;
    public float isolevel;
    public bool flip;
    public NativeList<MeshTriangle>.ParallelWriter triangles;
    [ReadOnly] public NativeArray<Voxel> voxels;

    //Static marching cubes skirts lookup tables variables
    private static readonly int3[,] edgesX = new int3[,]
    {
            { math.int3(0, 0, 0), math.int3(0, 1, 0) },
            { math.int3(0, 1, 0), math.int3(0, 1, 1) },
            { math.int3(0, 1, 1), math.int3(0, 0, 1) },
            { math.int3(0, 0, 1), math.int3(0, 0, 0) },
    };
    private static readonly int[,] edgesCornersX = new int[,]
    {
        { 0, resolution },
        { resolution, resolution + resolution * resolution },
        { resolution + resolution * resolution, resolution * resolution },
        { resolution * resolution, 0 },
    };
    public void Execute(int index)
    {
        int2 pos = math.int2(index % (resolution - 3), index / (resolution - 3));
        int i = TerrainUtility.FlattenIndex(math.int3(slicePoint + 1, pos.x + 1, pos.y + 1), resolution);
        //Indexing
        int msCase = 0;
        if (voxels[i].density < 0) msCase |= 1;
        if (voxels[i + resolution * resolution].density < 0) msCase |= 2;
        if (voxels[i + resolution + resolution * resolution].density < 0) msCase |= 4;
        if (voxels[i + resolution].density < 0) msCase |= 8;
        //Get the corners
        SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
        cornerVoxels[0] = new SkirtVoxel(voxels[i], math.float3(slicePoint, pos) * reductionFactorChunkScaled);
        cornerVoxels[1] = new SkirtVoxel(voxels[i + resolution], math.float3(slicePoint, pos.x + 1, pos.y) * reductionFactorChunkScaled);
        cornerVoxels[2] = new SkirtVoxel(voxels[i + resolution + resolution * resolution], math.float3(slicePoint, pos.x + 1, pos.y + 1) * reductionFactorChunkScaled);
        cornerVoxels[3] = new SkirtVoxel(voxels[i + resolution * resolution], math.float3(slicePoint, pos.x, pos.y + 1) * reductionFactorChunkScaled);
        //Get each edge's skirtVoxel
        SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
        for (int e = 0; e < 4; e++)
        {
            Voxel a = voxels[i + edgesCornersX[e, 0]];
            Voxel b = voxels[i + edgesCornersX[e, 1]];
            float lerpValue = math.unlerp(a.density, b.density, isolevel);
            edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (math.lerp(edgesX[e, 0], edgesX[e, 1], lerpValue) + new int3(slicePoint, pos.x, pos.x)) * (reductionFactorChunkScaled));
        }
        SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
    }
    //Triangulate the marching squares case
    void SolveMarchingSquareCase(int msCase, SkirtVoxel[] cornerVoxels, SkirtVoxel[] edgeMiddleVoxels, bool flip)
    {
        //Please, someone help me, how can I do something other than this!?!?!
        switch (msCase)
        {
            case 0:
                break;
            case 1:
                AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], edgeMiddleVoxels[3], flip);
                break;
            case 2:
                AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[2], cornerVoxels[3], flip);
                break;
            case 3:
                AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], edgeMiddleVoxels[2], flip);
                AddTriangle(edgeMiddleVoxels[2], cornerVoxels[3], cornerVoxels[0], flip);
                break;
            case 4:
                AddTriangle(edgeMiddleVoxels[2], edgeMiddleVoxels[1], cornerVoxels[2], flip);
                break;
            case 5:
                AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], edgeMiddleVoxels[3], flip);
                AddTriangle(cornerVoxels[2], edgeMiddleVoxels[1], edgeMiddleVoxels[2], flip);
                break;
            case 6:
                AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[1], cornerVoxels[2], flip);
                AddTriangle(cornerVoxels[2], cornerVoxels[3], edgeMiddleVoxels[3], flip);
                break;
            case 7:
                AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], cornerVoxels[3], flip);
                AddTriangle(cornerVoxels[3], edgeMiddleVoxels[1], cornerVoxels[2], flip);
                AddTriangle(edgeMiddleVoxels[0], edgeMiddleVoxels[1], cornerVoxels[3], flip);
                break;
            case 8:
                AddTriangle(edgeMiddleVoxels[1], edgeMiddleVoxels[0], cornerVoxels[1], flip);
                break;
            case 9:
                AddTriangle(cornerVoxels[1], edgeMiddleVoxels[1], edgeMiddleVoxels[3], flip);
                AddTriangle(edgeMiddleVoxels[3], cornerVoxels[0], cornerVoxels[1], flip);
                break;
            case 10:
                AddTriangle(cornerVoxels[1], edgeMiddleVoxels[1], edgeMiddleVoxels[0], flip);
                AddTriangle(cornerVoxels[3], edgeMiddleVoxels[3], edgeMiddleVoxels[2], flip);
                break;
            case 11:
                AddTriangle(cornerVoxels[1], edgeMiddleVoxels[1], cornerVoxels[0], flip);
                AddTriangle(cornerVoxels[0], edgeMiddleVoxels[2], cornerVoxels[3], flip);
                AddTriangle(edgeMiddleVoxels[1], edgeMiddleVoxels[2], cornerVoxels[0], flip);
                break;
            case 12:
                AddTriangle(edgeMiddleVoxels[2], edgeMiddleVoxels[0], cornerVoxels[1], flip);
                AddTriangle(cornerVoxels[1], cornerVoxels[2], edgeMiddleVoxels[2], flip);
                break;
            case 13:
                AddTriangle(cornerVoxels[1], edgeMiddleVoxels[3], cornerVoxels[0], flip);
                AddTriangle(cornerVoxels[2], edgeMiddleVoxels[2], cornerVoxels[1], flip);
                AddTriangle(cornerVoxels[1], edgeMiddleVoxels[2], edgeMiddleVoxels[3], flip);
                break;
            case 14:
                AddTriangle(cornerVoxels[2], edgeMiddleVoxels[0], cornerVoxels[1], flip);
                AddTriangle(cornerVoxels[3], edgeMiddleVoxels[3], cornerVoxels[2], flip);
                AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[0], cornerVoxels[2], flip);
                break;
            case 15:
                break;
            default:
                break;
        }
    }
    //Add a single trianle to the mesh (Used for skirts)
    void AddTriangle(SkirtVoxel a, SkirtVoxel b, SkirtVoxel c, bool flip)
    {
        triangles.AddNoResize(flip ? new MeshTriangle(a, b, c) : new MeshTriangle(c, b, a));
    }
}