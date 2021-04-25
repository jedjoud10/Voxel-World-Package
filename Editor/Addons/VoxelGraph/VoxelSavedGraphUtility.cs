using System;
using System.Collections.Generic;
using System.Linq;
using static VoxelSavedGraphUtility;
using static VoxelGraphUtility;
using static VoxelUtility;
using UnityEngine;
using UnityEditor;

public static class VoxelSavedGraphUtility 
{
    /// <summary>
    /// Serializes and deserializes a binary file containing the savedglobalvoxelgraph
    /// </summary>
    public class VoxelGraphSerializer 
    {
        public string path;
        public SavedGlobalVoxelGraph globalGraph;
        /// <summary>
        /// Save a specific local graph
        /// </summary>
        public void SaveLocalGraph(VoxelGraphView localGraphView, VoxelGraphType type) 
        { 
            localGraphView.SaveLocalVoxelGraph(globalGraph[type]);
            SaveGlobalGraph();
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// Ask the user if they want to save a specific voxel graph
        /// </summary>
        public void SaveLocalGraphAskUser(VoxelGraphView localGraphView, VoxelGraphType type, string message) 
        {
            if (globalGraph != null && localGraphView != null)
            { 
                localGraphView.SaveLocalVoxelGraph(globalGraph[type], message);
                SaveGlobalGraph();
            }
        }
        
        /// <summary>
        /// Save the whole graph
        /// </summary>
        public void SaveGlobalGraph() { BinaryLoaderSaver.Save(path, globalGraph); }
        
        /// <summary>
        /// Load the whole graph
        /// </summary>
        public void LoadGlobalGraph(string globalPath = null) { globalGraph = BinaryLoaderSaver.Load(globalPath ?? path) as SavedGlobalVoxelGraph; }
        
        /// <summary>
        /// Load the default voxel graph that came in with the plugin
        /// </summary>
        public void LoadDefaultVoxelGraph() { LoadGlobalGraph(Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Packages/Voxel-World-Package/Editor/Addons/VoxelGraph/DefaultVoxelGraph.voxelgraph"); }
    }


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
                posx = 0, posy = 0,
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
                posx = 0, posy = 0,
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
                posx = 0, posy = 0,
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
        public Dictionary<string, SavedVoxelNode> nodes;//Uses Node GUID
        public Dictionary<string, SavedVoxelEdge> edges;//Uses input port GUID
    }

    /// <summary>
    /// Custom IEqualityComparer from https://stackoverflow.com/questions/6413108/how-do-i-check-if-two-objects-are-equal-in-terms-of-their-properties-only-withou
    /// </summary>
    public class SavedVoxelNodeComparer : IEqualityComparer<KeyValuePair<string, SavedVoxelNode>>
    {
        public bool Equals(KeyValuePair<string, SavedVoxelNode> x, KeyValuePair<string, SavedVoxelNode> y)
        {
            if (x.Value == null || y.Key == null)
                return false;

            bool position = x.Value.posx == y.Value.posx && x.Value.posy == y.Value.posy;
            bool type = x.Value.type == y.Value.type;
            bool savedPorts = x.Value.savedPorts.SequenceEqual(y.Value.savedPorts);
            bool key = x.Key == y.Key;

            return (key && position && type && savedPorts);
            throw new System.NotImplementedException();
        }

        public int GetHashCode(KeyValuePair<string, SavedVoxelNode> obj)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// A node that was saved in the SavedVoxelGraph
    /// </summary>
    [System.Serializable]
    public class SavedVoxelNode
    {
        //Main variables
        public float posx, posy;
        public int type;
        public string guid;
        public object value;
        public List<string> savedPorts;

        //Optional value for constant numbers

        public static GraphViewNodeData ConvertToGraphViewNodeData(SavedVoxelNode savedVoxelNode) 
        {
            return new GraphViewNodeData() { guid = savedVoxelNode.guid, voxelNode = CreateVoxelNode(savedVoxelNode.type, savedVoxelNode.value), voxelNodeType = savedVoxelNode.type };
        }
    }

    /// <summary>
    /// An edge that was saved in the SavedVoxelGraph
    /// </summary>
    [System.Serializable]
    public class SavedVoxelEdge
    {
        //Main variables
        public SavedVoxelPort input;
        public SavedVoxelPort output;
    }

    /// <summary>
    /// A port that was saved in the SavedVoxelGraph
    /// </summary>
    [System.Serializable]
    public class SavedVoxelPort
    {
        //Main variables
        public string nodeGuid;
        public string portGuid;
    }
}