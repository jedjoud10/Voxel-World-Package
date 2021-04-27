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
    /// A whole voxel graph that's going to get wrapped
    /// </summary>
    [System.Serializable]
    public class SavedGlobalVoxelGraph
    {
        //Main graphs variables
        public bool defaultSet;
        public SavedLocalVoxelGraph densityGraph;
        public SavedLocalVoxelGraph csmGraph;
        public SavedLocalVoxelGraph voxelDetailsGraph;

        //Indexer
        public SavedLocalVoxelGraph this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return densityGraph;
                    case 1:
                        return csmGraph;
                    case 2:
                        return voxelDetailsGraph;
                    default:
                        return null;
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        densityGraph = value;
                        break;
                    case 1:
                        csmGraph = value;
                        break;
                    case 2:
                        voxelDetailsGraph = value;
                        break;
                }
            }
        }
        public SavedLocalVoxelGraph this[VoxelGraphType type]
        {
            get
            {
                switch (type)
                {
                    case VoxelGraphType.Density:
                        return densityGraph;
                    case VoxelGraphType.CSM:
                        return csmGraph;
                    case VoxelGraphType.VoxelDetails:
                        return voxelDetailsGraph;
                    default:
                        return null;
                }
            }

            set
            {
                switch (type)
                {
                    case VoxelGraphType.Density:
                        densityGraph = value;
                        break;
                    case VoxelGraphType.CSM:
                        csmGraph = value;
                        break;
                    case VoxelGraphType.VoxelDetails:
                        voxelDetailsGraph = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Make sure the default nodes are present in the local graphs
        /// </summary>
        public void SetDefaultVars()
        {
            //Generate the default Density graph node
            densityGraph = new SavedLocalVoxelGraph();
            var defaultNode = new SavedVoxelNode()
            {
                posx = 0,
                posy = 0,
                type = 5,
                value = null,
                guid = GUID.Generate().ToString()
            };
            densityGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { defaultNode.guid, defaultNode } };
            densityGraph.edges = new Dictionary<string, SavedVoxelEdge>();

            //Generate the default CSM graph if it wasn't generated yet
            csmGraph = new SavedLocalVoxelGraph();
            defaultNode = new SavedVoxelNode()
            {
                posx = 0,
                posy = 0,
                type = 6,
                value = null,
                guid = GUID.Generate().ToString()
            };
            csmGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { defaultNode.guid, defaultNode } };
            csmGraph.edges = new Dictionary<string, SavedVoxelEdge>();

            //Generate the default VoxelDetails graph if it wasn't generated yet        
            voxelDetailsGraph = new SavedLocalVoxelGraph();
            defaultNode = new SavedVoxelNode()
            {
                posx = 0,
                posy = 0,
                type = 7,
                value = null,
                guid = GUID.Generate().ToString()
            };
            voxelDetailsGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { defaultNode.guid, defaultNode } };
            voxelDetailsGraph.edges = new Dictionary<string, SavedVoxelEdge>();
            defaultSet = true;
            BinaryLoaderSaver.Save(Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Packages/Voxel-World-Package/Editor/Addons/VoxelGraph/DefaultVoxelGraph.voxelgraph", this);
        }
    }

    /// <summary>
    /// A saved local voxel graph (like density graph / csm graph / voxeldetails graph)
    /// </summary>
    [System.Serializable]
    public class SavedLocalVoxelGraph
    {
        //Main variables
        public Dictionary<string, SavedVoxelNode> nodes = new Dictionary<string, SavedVoxelNode>();//Uses Node GUID
        public Dictionary<string, SavedVoxelEdge> edges = new Dictionary<string, SavedVoxelEdge>();//Uses input port GUID
    }
}