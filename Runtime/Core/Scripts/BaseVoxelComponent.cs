using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jedjoud.VoxelWorld
{
    /// <summary>
    /// A base voxel class for the multiple voxel managers
    /// </summary>
    public class BaseVoxelComponent : MonoBehaviour
    {
        //Main vars
        protected VoxelWorld voxelWorld;

        /// <summary>
        /// Setup this base voxel class
        /// </summary>
        public virtual void Setup(VoxelWorld voxelWorld) 
        {
            this.voxelWorld = voxelWorld;
        }

        /// <summary>
        /// When we need to release everything that this voxel component holds
        /// </summary>
        public virtual void Release() 
        { 
        
        }
    }
}