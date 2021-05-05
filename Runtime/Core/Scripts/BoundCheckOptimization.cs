using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jedjoud.VoxelWorld;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
using static Unity.Mathematics.math;
/// <summary>
/// Checks if a specific octree node touches the surface, if not then ignore it
/// </summary>
namespace Jedjoud.VoxelWorld
{
    public static class BoundCheckOptimization
    {
        /// <summary>
        /// If this returns true, then that means that node does intersect the surface and we should generate it
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool CheckNode(OctreeNode node, CameraData cameraData)
        {
            return true;
            return NodeIntersectWithBounds(node, new VoxelAABBBound() { min = new Vector3(-5, -5, -5), max = new Vector3(5, 5, 5) });
        }
    }
}
