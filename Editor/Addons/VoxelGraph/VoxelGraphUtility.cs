using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using static VoxelUtility;
using static VoxelGraphUtility;
using static VoxelSavedGraphUtility;
using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// Utility class for everything related to the generation graph
/// </summary>
public static partial class VoxelGraphUtility
{
    /// <summary>
    /// An interface used to check sdfs against octree nodes and ignore ones that don't intersect with the surface
    /// </summary>
    public interface IBoundCheckOptimizator
    {
        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public VoxelAABBBound GetAABB();
    }

    public static List<VoxelNode> templateVoxelNodes = GetAllVoxelNodes();

    /// <summary>
    /// Gets all of the VoxelGenerationObject classes
    /// </summary>
    /// <returns></returns>
    private static List<VoxelNode> GetAllVoxelNodes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(VoxelNode)) && !type.IsAbstract)
            .Select(type => (VoxelNode)Activator.CreateInstance(type))
            .ToList();
    }
}