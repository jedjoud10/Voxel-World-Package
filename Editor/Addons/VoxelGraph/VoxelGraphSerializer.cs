using System;
using System.Collections.Generic;
using System.Linq;
using static VoxelSavedGraphUtility;
using static VoxelGraphUtility;
using static VoxelUtility;
using UnityEngine;
using UnityEditor;

public static partial class VoxelSavedGraphUtility
{
    /// <summary>
    /// Serializes and deserializes a binary file containing the savedglobalvoxelgraph
    /// </summary>
    public class VoxelGraphSerializer
    {
        //Main variables
        public static string defaultPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Packages/Voxel-World-Package/Editor/Addons/VoxelGraph/DefaultVoxelGraph.voxelgraph";
        public SavedGlobalVoxelGraph globalGraph = new SavedGlobalVoxelGraph();
        public string path = "";

        /// <summary>
        /// Save a specific local graph
        /// </summary>
        public SavedLocalVoxelGraph SaveLocalGraph(VoxelGraphView localGraphView, VoxelGraphType type)
        {
            globalGraph[type] = localGraphView.SaveLocalVoxelGraph();
            SaveGlobalGraph();
            AssetDatabase.Refresh();
            return globalGraph[type];
        }

        /// <summary>
        /// Ask the user if they want to save a specific voxel graph
        /// </summary>
        public SavedLocalVoxelGraph SaveLocalGraphAskUser(VoxelGraphView localGraphView, VoxelGraphType type, string message)
        {
            if (globalGraph != null && localGraphView != null)
            {
                bool save = false;
                globalGraph[type] = localGraphView.SaveLocalVoxelGraph(globalGraph[type], message, out save);
                if (save || path == defaultPath)
                {
                    SaveGlobalGraph();
                    AssetDatabase.Refresh();
                }
                return globalGraph[type];
            }
            return globalGraph[type];
        }

        /// <summary>
        /// Save the whole graph
        /// </summary>
        public void SaveGlobalGraph()
        {
            if (string.IsNullOrEmpty(path) || defaultPath == path) path = EditorUtility.SaveFilePanel("Save VoxelGraph:", "Assets/", "NewVoxelGraph", "voxelgraph");
            if (!string.IsNullOrEmpty(path)) BinaryLoaderSaver.Save(path, globalGraph);
        }

        /// <summary>
        /// Load the whole graph
        /// </summary>
        public void LoadGlobalGraph(string globalPath)
        {
            globalGraph = BinaryLoaderSaver.Load(globalPath ?? defaultPath) as SavedGlobalVoxelGraph;
            path = globalPath;
        }

        /// <summary>
        /// Load the default voxel graph that came in with the plugin
        /// </summary>
        public void LoadDefaultVoxelGraph() { LoadGlobalGraph(defaultPath); }
    }
}