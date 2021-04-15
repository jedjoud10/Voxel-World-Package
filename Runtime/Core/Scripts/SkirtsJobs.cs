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
[BurstCompile]
public struct SkirtsJob : IJobParallelFor
{
    //Marching cubes skirts variables
    private const int resolution = VoxelWorld.resolution;
    public float reductionFactorChunkScaled;
    public float chunkSize;
    public float isolevel;
    public NativeList<MeshTriangle>.ParallelWriter mcTriangles;
    [ReadOnly] public NativeArray<Voxel> voxels;

    //Static marching cubes skirts lookup tables variables
    private static readonly int3[] edgesX = new int3[]
    {
         new int3(0, 0, 0), new int3(0, 1, 0),
         new int3(0, 1, 0), new int3(0, 1, 1),
         new int3(0, 1, 1), new int3(0, 0, 1),
         new int3(0, 0, 1), new int3(0, 0, 0),
    };
    private static readonly int[] edgesCornersX = new int[]
    {
        0, resolution,
        resolution, resolution + resolution * resolution ,
        resolution + resolution * resolution, resolution * resolution,
        resolution * resolution, 0,
    };
    private static readonly int3[] edgesY = new int3[]
    {
        new int3(0, 0, 0), new int3(1, 0, 0),
        new int3(1, 0, 0), new int3(1, 0, 1),
        new int3(1, 0, 1), new int3(0, 0, 1),
        new int3(0, 0, 1), new int3(0, 0, 0),
    };
    private static readonly int[] edgesCornersY = new int[]
    {
        0, 1,
        1, resolution * resolution + 1,
        resolution * resolution + 1, resolution * resolution,
        resolution * resolution, 0,
    };
    private static readonly int3[] edgesZ = new int3[]
    {
        new int3(0, 0, 0), new int3(0, 1, 0),
        new int3(0, 1, 0), new int3(1, 1, 0),
        new int3(1, 1, 0), new int3(1, 0, 0),
        new int3(1, 0, 0), new int3(0, 0, 0),
    };
    private static readonly int[] edgesCornersZ = new int[]
    {
        0, resolution ,
        resolution, resolution + 1 ,
        resolution + 1, 1 ,
        1, 0 ,
    };
    public void Execute(int index)
    {
        NativeArray<SkirtVoxel> cornerVoxels = new NativeArray<SkirtVoxel>(4, Allocator.Temp);
        NativeArray<SkirtVoxel> edgeMiddleVoxels = new NativeArray<SkirtVoxel>(4, Allocator.Temp);
        int i = index / ((resolution - 3) * (resolution - 3));
        int index2 = index % ((resolution - 3) * (resolution - 3));
        switch (i)
        {
            case 0:
                SolveXSkirt(index2, 0, false, cornerVoxels, edgeMiddleVoxels);
                break;
            case 1:
                SolveYSkirt(index2, 0, true, cornerVoxels, edgeMiddleVoxels);
                break;
            case 2:
                SolveZSkirt(index2, 0, true, cornerVoxels, edgeMiddleVoxels);
                break;
            case 3:
                SolveXSkirt(index2, resolution - 3, true, cornerVoxels, edgeMiddleVoxels);
                break;
            case 4:
                SolveYSkirt(index2, resolution - 3, false, cornerVoxels, edgeMiddleVoxels);
                break;
            case 5:
                SolveZSkirt(index2, resolution - 3, false, cornerVoxels, edgeMiddleVoxels);
                break;
            default:
                break;
        }
        cornerVoxels.Dispose();
        edgeMiddleVoxels.Dispose();
    }
    /// <summary>
    /// Solve the skirt in the X axis
    /// </summary>
    private void SolveXSkirt(int index, int slicePoint, bool flip, NativeArray<SkirtVoxel> cornerVoxels, NativeArray<SkirtVoxel> edgeMiddleVoxels) 
    {
        int2 pos = math.int2(index / (resolution - 3), index % (resolution - 3));
        int i = FlattenIndex(math.int3(slicePoint + 1, pos.x + 1, pos.y + 1), resolution);
        //Indexing
        int msCase = 0;
        if (voxels[i].density < 0) msCase |= 1;
        if (voxels[i + resolution * resolution].density < 0) msCase |= 2;
        if (voxels[i + resolution + resolution * resolution].density < 0) msCase |= 4;
        if (voxels[i + resolution].density < 0) msCase |= 8;
        //Get the corners
        cornerVoxels[0] = new SkirtVoxel(voxels[i], math.float3(slicePoint, pos.x, pos.y) * reductionFactorChunkScaled);
        cornerVoxels[1] = new SkirtVoxel(voxels[i + resolution], math.float3(slicePoint, pos.x + 1, pos.y) * reductionFactorChunkScaled);
        cornerVoxels[2] = new SkirtVoxel(voxels[i + resolution + resolution * resolution], math.float3(slicePoint, pos.x + 1, pos.y + 1) * reductionFactorChunkScaled);
        cornerVoxels[3] = new SkirtVoxel(voxels[i + resolution * resolution], math.float3(slicePoint, pos.x, pos.y + 1) * reductionFactorChunkScaled);
        //Get each edge's skirtVoxel
        for (int e = 0; e < 4; e++)
        {
            Voxel a = voxels[i + edgesCornersX[e * 2 + 0]];
            Voxel b = voxels[i + edgesCornersX[e * 2 + 1]];
            float lerpValue = math.unlerp(a.density, b.density, isolevel);
            edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (math.lerp(edgesX[e * 2 + 0], edgesX[e * 2 + 1], lerpValue) + new int3(slicePoint, pos.x, pos.y)) * (reductionFactorChunkScaled));
        }
        SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
    }
    
    /// <summary>
    /// Solve the skirt in the Y axis
    /// </summary>
    private void SolveYSkirt(int index, int slicePoint, bool flip, NativeArray<SkirtVoxel> cornerVoxels, NativeArray<SkirtVoxel> edgeMiddleVoxels)
    {
        int2 pos = math.int2(index % (resolution - 3), index / (resolution - 3));
        int i = FlattenIndex(math.int3(pos.x + 1, slicePoint + 1, pos.y + 1), resolution);
        //Indexing
        int msCase = 0;
        if (voxels[i].density < 0) msCase |= 1;
        if (voxels[i + resolution * resolution].density < 0) msCase |= 2;
        if (voxels[i + resolution * resolution + 1].density < 0) msCase |= 4;
        if (voxels[i + 1].density < 0) msCase |= 8;
        //Get the corners
        cornerVoxels[0] = new SkirtVoxel(voxels[i], math.float3(pos.x, slicePoint, pos.y) * reductionFactorChunkScaled);
        cornerVoxels[1] = new SkirtVoxel(voxels[i + resolution * resolution], math.float3(pos.x + 1, slicePoint, pos.y) * reductionFactorChunkScaled);
        cornerVoxels[2] = new SkirtVoxel(voxels[i + resolution * resolution + 1], math.float3(pos.x + 1, slicePoint, pos.y + 1) * reductionFactorChunkScaled);
        cornerVoxels[3] = new SkirtVoxel(voxels[i + 1], math.float3(pos.x, slicePoint, pos.y + 1) * reductionFactorChunkScaled);
        //Get each edge's skirtVoxel
        for (int e = 0; e < 4; e++)
        {
            Voxel a = voxels[i + edgesCornersY[e * 2 + 0]];
            Voxel b = voxels[i + edgesCornersY[e * 2 + 1]];
            float lerpValue = math.unlerp(a.density, b.density, isolevel);
            edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (math.lerp(edgesY[e * 2 + 0], edgesY[e * 2 + 1], lerpValue) + new int3(pos.x, slicePoint, pos.y)) * (reductionFactorChunkScaled));
        }
        SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
    }
    
    /// <summary>
    /// Solve the skirt in the Z axis
    /// </summary>
    private void SolveZSkirt(int index, int slicePoint, bool flip, NativeArray<SkirtVoxel> cornerVoxels, NativeArray<SkirtVoxel> edgeMiddleVoxels)
    {
        int2 pos = math.int2(index % (resolution - 3), index / (resolution - 3));
        int i = FlattenIndex(math.int3(pos.x + 1, pos.y + 1, slicePoint + 1), resolution);
        //Indexing
        int msCase = 0;
        if (voxels[i].density < 0) msCase |= 1;
        if (voxels[i + 1].density < 0) msCase |= 2;
        if (voxels[i + resolution + 1].density < 0) msCase |= 4;
        if (voxels[i + resolution].density < 0) msCase |= 8;
        //Get the corners
        cornerVoxels[0] = new SkirtVoxel(voxels[i], new Vector3(pos.x, pos.y, slicePoint) * reductionFactorChunkScaled);
        cornerVoxels[1] = new SkirtVoxel(voxels[i + resolution], new Vector3(pos.x, pos.y + 1, slicePoint) * reductionFactorChunkScaled);
        cornerVoxels[2] = new SkirtVoxel(voxels[i + resolution + 1], new Vector3(pos.x + 1, pos.y + 1, slicePoint) * reductionFactorChunkScaled);
        cornerVoxels[3] = new SkirtVoxel(voxels[i + 1], new Vector3(pos.x + 1, pos.y, slicePoint) * reductionFactorChunkScaled);
        //Get each edge's skirtVoxel
        //Run on each edge
        for (int e = 0; e < 4; e++)
        {
            Voxel a = voxels[i + edgesCornersZ[e * 2 + 0]];
            Voxel b = voxels[i + edgesCornersZ[e * 2 + 1]];
            float lerpValue = math.unlerp(a.density, b.density, isolevel);
            edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (math.lerp(edgesZ[e * 2 + 0], edgesZ[e * 2 + 1], lerpValue) + math.int3(pos.x, pos.y, slicePoint)) * reductionFactorChunkScaled);
        }
        SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
    }

    /// <summary>
    /// Solve a single marching square case
    /// </summary>
    private void SolveMarchingSquareCase(int msCase, NativeArray<SkirtVoxel> cornerVoxels, NativeArray<SkirtVoxel> edgeMiddleVoxels, bool flip)
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
                if ((cornerVoxels[0].smoothnessMetallicDensity.z + cornerVoxels[1].smoothnessMetallicDensity.z + cornerVoxels[2].smoothnessMetallicDensity.z + cornerVoxels[3].smoothnessMetallicDensity.z) / 4 > -chunkSize / 15f)
                {
                    AddTriangle(cornerVoxels[0], cornerVoxels[1], cornerVoxels[2], flip);
                    AddTriangle(cornerVoxels[0], cornerVoxels[2], cornerVoxels[3], flip);
                }
                break;                
            default:
                break;
        }
    }
    /// <summary>
    /// Add a single triangle to the mesh
    /// </summary>
    private void AddTriangle(SkirtVoxel a, SkirtVoxel b, SkirtVoxel c, bool flip)
    {
        mcTriangles.AddNoResize(flip ? new MeshTriangle(a, b, c) : new MeshTriangle(c, b, a));
    }
}