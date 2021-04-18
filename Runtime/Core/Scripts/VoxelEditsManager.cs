using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoxelUtility;
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