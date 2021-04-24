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
                pos = Vector2.zero,
                nodeData = new GraphViewNodeData { voxelNode = (VoxelNode)Activator.CreateInstance(voxelNodesTypes[5]), guid = GUID.Generate().ToString() },
            };
            densityGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { defaultNode.nodeData.guid, defaultNode } };
            densityGraph.edges = new Dictionary<string, SavedVoxelEdge>();

            //Generate the default CSM graph if it wasn't generated yet
            csmGraph = new SavedLocalVoxelGraph();
            defaultNode = new SavedVoxelNode()
            {
                pos = Vector2.zero,
                nodeData = new GraphViewNodeData { voxelNode = (VoxelNode)Activator.CreateInstance(voxelNodesTypes[6]), guid = GUID.Generate().ToString() },
            };
            csmGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { defaultNode.nodeData.guid, defaultNode } };
            csmGraph.edges = new Dictionary<string, SavedVoxelEdge>();

            //Generate the default VoxelDetails graph if it wasn't generated yet        
            voxelDetailsGraph = new SavedLocalVoxelGraph();
            defaultNode = new SavedVoxelNode()
            {
                pos = Vector2.zero,
                nodeData = new GraphViewNodeData { voxelNode = (VoxelNode)Activator.CreateInstance(voxelNodesTypes[7]), guid = GUID.Generate().ToString() },
            };
            voxelDetailsGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { defaultNode.nodeData.guid, defaultNode } };
            voxelDetailsGraph.edges = new Dictionary<string, SavedVoxelEdge>();

            defaultSet = true;
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

        //Godamnit Unity why aren't dictionaries serializable
        public List<string> node_keys, edges_keys;
        public List<SavedVoxelNode> node_values;
        public List<SavedVoxelEdge> edges_values;

        /// <summary>
        /// Turn the dictionaries into the lists
        /// </summary>
        public void SaveDictionaries()
        {
            node_keys = nodes.Keys.ToList();
            node_values = nodes.Values.ToList();

            edges_keys = edges.Keys.ToList();
            edges_values = edges.Values.ToList();
        }

        /// <summary>
        /// Turn the lists into dictionaries
        /// </summary>
        public void LoadDictionaries()
        {
            //https://stackoverflow.com/questions/4038978/map-two-lists-into-a-dictionary-in-c-sharp
            if (node_keys == null) { node_keys = new List<string>(); node_values = new List<SavedVoxelNode>(); }
            if (edges_keys == null) { edges_keys = new List<string>(); edges_values = new List<SavedVoxelEdge>(); };

            nodes ??= node_keys.Select((k, i) => new { k, v = node_values[i] }).ToDictionary(x => x.k, x => x.v);
            edges ??= edges_keys.Select((k, i) => new { k, v = edges_values[i] }).ToDictionary(x => x.k, x => x.v);
        }
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

            bool position = x.Value.pos == y.Value.pos;
            bool type = x.Value.nodeData.GetType() == y.Value.nodeData.GetType();
            bool savedPorts = x.Value.nodeData.voxelNode.savedPorts.SequenceEqual(y.Value.nodeData.voxelNode.savedPorts);
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
        public Vector2 pos;
        public GraphViewNodeData nodeData;
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
        public GraphViewPortData portData;
        public string nodeGuid;
    }
}