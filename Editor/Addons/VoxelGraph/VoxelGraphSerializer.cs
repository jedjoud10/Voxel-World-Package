using System;
using System.Collections.Generic;
using System.Linq;
using static SavedVoxelGraphUtility;
using static VoxelGraphUtility;
using static VoxelUtility;
using UnityEngine;
using UnityEditor;

public static partial class SavedVoxelGraphUtility
{
    /// <summary>
    /// Serializes and deserializes a binary file containing the savedglobalvoxelgraph
    /// </summary>
    public class VoxelGraphSerializer
    {
        //Main variables
        public static string defaultPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Packages/Voxel-World-Package/Editor/Addons/VoxelGraph/DefaultVoxelGraph.voxelgraph";
        public SavedGlobalVoxelGraph globalGraph = new SavedGlobalVoxelGraph();

        /// <summary>
        /// Save a specific local graph
        /// </summary>
        public SavedLocalVoxelGraph SaveLocalGraph(SavedLocalVoxelGraph savedLocalVoxelGraph, VoxelGraphType type)
        {
            globalGraph[type] = savedLocalVoxelGraph;            
            return globalGraph[type];
        }

        /// <summary>
        /// Save the whole graph
        /// </summary>
        public void SaveGlobalGraph(string path)
        {
            if (!string.IsNullOrEmpty(path) && defaultPath != path) BinaryLoaderSaver.Save(path, globalGraph);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Load the whole graph
        /// </summary>
        public void LoadGlobalGraph(string globalPath)
        {
            globalGraph = BinaryLoaderSaver.Load(globalPath ?? defaultPath) as SavedGlobalVoxelGraph;
        }

        /// <summary>
        /// Load the default voxel graph that came in with the voxelgraph addon
        /// </summary>
        public void LoadDefaultVoxelGraph() { LoadGlobalGraph(defaultPath); }
    }
}