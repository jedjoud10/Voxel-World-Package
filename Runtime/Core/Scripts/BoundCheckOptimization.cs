using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoxelUtility;
/// <summary>
/// Checks if a specific octree node touches the surface, if not then ignore it
/// </summary>
public static class BoundCheckOptimization
{
    /// <summary>
    /// If this returns true, then that means that node does intersect the surface and we should generate it
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool CheckNode(OctreeNode node) 
    {
        return true;
        return NodeIntersectWithBounds(node, new VoxelAABBBound() { min = new Vector3(-50000, -1, -50000), max = new Vector3(50000, 1, 50000) });
        return node.chunkPosition.y < 50 && node.chunkPosition.y > -90;
    }
}
