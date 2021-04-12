using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Manages and optimizes the voxel edits
public class VoxelEditsManager
{
    private VoxelWorld voxelWorld;
    public List<VoxelEditRequestBatch> voxelEditRequestBatches;//Voxel edit requests to edit chunks
    public VoxelEditsManager(VoxelWorld _voxelWorld) 
    {
        voxelWorld = _voxelWorld;
        voxelEditRequestBatches = new List<VoxelEditRequestBatch>();
    }
    //Edit the terrain using a cubic tool
    public void Edit(VoxelEditRequestBatch voxelEditRequestBatch)
    {
        //Create the batch before checking the nodes in the octree
        voxelEditRequestBatches.Add(voxelEditRequestBatch);
        //Edit the octree to check what nodes we have to update
        voxelWorld.octree.CheckNodesToEdit(voxelEditRequestBatch);
    }
    //Force the optimization of the voxelEditRequestBatches using k-means clustering
    public void ForceOptimize() 
    {
        //Only optimize when the voxelEditRequestBatches are valid and that no chunks are updating
        if (voxelEditRequestBatches.Count > 0 && voxelWorld.chunksUpdating.Count == 0)
        {

        }
    }
    //Save the edits on the disk
    public void SaveEdits() 
    {
        
    }
    //Loads edits from a specific .vedits file
    public void LoadEdits() 
    {
        
    }
}
//An AABB Bound
public struct VoxelAABBBound
{
    //The min and max of the bound
    public Vector3 min, max;
}
//A voxel edit requested that is going to be saved / loaded
public struct VoxelEditRequest
{
    public EditRequest editRequest;
    public VoxelAABBBound bound;
}
//A voxel edit request batch, this is used for saving time when doing mass edits
public struct VoxelEditRequestBatch
{
    public List<VoxelEditRequest> voxelEditRequests;
    public VoxelAABBBound bound;
    //Add a voxel edit request and update the bound if needed
    public void AddVoxelEditRequest(VoxelEditRequest request)
    {
        if (voxelEditRequests != null)
        {
            voxelEditRequests.Add(request);
            bound.max = Vector3.Max(bound.max, request.bound.max);
            bound.min = Vector3.Min(bound.min, request.bound.min);
        }
        else
        {
            voxelEditRequests = new List<VoxelEditRequest>();
        }
    }

    //Create a batch for one voxel editEditRequest
    public VoxelEditRequestBatch(VoxelEditRequest request)
    {
        voxelEditRequests = new List<VoxelEditRequest>();
        voxelEditRequests.Add(request);
        bound = request.bound;
    }
}
//A singular edit request that will be passed to the edit compute shader
public struct EditRequest
{
    public Vector3 center;
    public Vector3 color;
    public float size;
    public int shape;
    public int editType;
}