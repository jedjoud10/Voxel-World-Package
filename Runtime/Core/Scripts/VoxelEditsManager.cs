using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
using static Unity.Mathematics.math;

namespace Jedjoud.VoxelWorld
{
    /// <summary>
    /// Manages and optimizes voxel edits
    /// </summary>
    public class VoxelEditsManager : BaseVoxelComponent
    {
        public List<VoxelEditRequestBatch> voxelEditRequestBatches = new List<VoxelEditRequestBatch>();//Voxel edit requests to edit chunks

        /// <summary>
        /// Initialize this edits manager
        /// </summary>
        public override void Setup(VoxelWorld voxelWorld)
        {
            base.Setup(voxelWorld);
            voxelEditRequestBatches = new List<VoxelEditRequestBatch>();
        }

        //Force the optimization of the voxelEditRequestBatches using k-means clustering
        public void ForceOptimize()
        {

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
}