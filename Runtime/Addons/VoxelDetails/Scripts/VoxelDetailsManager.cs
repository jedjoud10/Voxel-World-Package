using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jedjoud.VoxelWorld;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
/// <summary>
/// Manages the spawning of VoxelDetails
/// </summary>
public class VoxelDetailsManager : MonoBehaviour
{
    //Main detail vars
    public bool generate;
    public GameObject[] voxelDetailsPrefabs;
    public VoxelDetail[] voxelDetails = new VoxelDetail[0];
    public ComputeBuffer detailsBuffer, countBuffer;
    private VoxelWorld voxelWorld;

    /// <summary>
    /// Initialize this details manager
    /// </summary>
    public VoxelDetailsManager Setup(VoxelWorld voxelWorld) 
    {
        this.voxelWorld = voxelWorld;
        detailsBuffer = new ComputeBuffer((VoxelWorld.resolution-3) * (VoxelWorld.resolution-3) * (VoxelWorld.resolution-3), sizeof(float) * 8, ComputeBufferType.Append);
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        voxelWorld.chunkManager.generationShader.SetBuffer(1, "detailsBuffer", detailsBuffer);
        return this;
    }

    /// <summary>
    /// Instantiate voxel details for a specific chunk
    /// </summary>
    /// <param name="chunk"></param>
    public void InstantiateVoxelDetails(Chunk chunk) 
    {
        //Instantiate the details
        for (int i = 0; i < voxelDetails.Length; i++)
        {
            Instantiate(voxelDetailsPrefabs[voxelDetails[i].type], voxelDetails[i].position, Quaternion.LookRotation(voxelDetails[i].forward), chunk.chunkGameObject.transform);
        }
    }

    /// <summary>
    /// Resest the counter value for the detailsBuffer
    /// </summary>
    public void ResetBufferCount() { detailsBuffer.SetCounterValue(0); }

    /// <summary>
    /// Gets the VoxelDetails data from the GPU buffer
    /// </summary>
    public void GetDataFromBuffer(Chunk chunk, OctreeNode node) 
    {
        if (!generate) return;
        //if (node.hierarchyIndex != voxelWorld.maxHierarchyIndex) return;
        ComputeBuffer.CopyCount(detailsBuffer, countBuffer, 0);
        int[] count = new int[1] { 0 };
        countBuffer.GetData(count);
        voxelDetails = new VoxelDetail[count[0]];
        detailsBuffer.GetData(voxelDetails);
    }

    //Release everything
    private void OnDestroy()
    {
        detailsBuffer.Release();
        countBuffer.Release();
    }
}
