using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static VoxelGraphUtility;
/// <summary>
/// A scriptable object for the voxel graphs
/// </summary>
[CreateAssetMenu(fileName = "NewVoxelGraph", menuName = "Voxel World/Voxel Graph")]
[System.Serializable]
public class VoxelGraphSO : ScriptableObject
{
    public SavedVoxelGraph densityGraph;
    public SavedVoxelGraph csmGraph;
    public SavedVoxelGraph voxelDetailsGraph;

    /// <summary>
    /// Opens the voxel graph when double clicking the asset
    /// </summary>
    [OnOpenAssetAttribute(0)]
    public static bool OpenVoxelGraph(int instanceID, int line)
    {
        VoxelGraphSO obj = (VoxelGraphSO)EditorUtility.InstanceIDToObject(instanceID);
        //Generate the default density graph

        if (obj.densityGraph == null || obj.densityGraph.nodes.Count == 0) 
        {
            obj.densityGraph = new SavedVoxelGraph();
            obj.densityGraph.nodes = new List<SavedVoxelNode>(1) { new SavedVoxelNode() 
            {
                guid = GUID.Generate().ToString(),
                pos = Vector2.zero,
                type = 5,
                value = null
            } };
        }

        if (obj.csmGraph == null) obj.csmGraph = new SavedVoxelGraph();
        if (obj.voxelDetailsGraph == null) obj.voxelDetailsGraph = new SavedVoxelGraph();
        VoxelGraph.OpenGraphWindow(obj);
        return true; // we did not handle the open
    }

    /// <summary>
    /// Save a specific voxel graph
    /// </summary>
    public void SaveVoxelGraph(VoxelGraphView graph, VoxelGraphType voxelGraphType) 
    {
        densityGraph.nodes = new List<SavedVoxelNode>();
        densityGraph.edges = new List<SavedVoxelEdge>();
        //Nodes
        foreach (var node in graph.nodes)
        {
            SavedVoxelNode savedNode = new SavedVoxelNode()
            {
                guid = ((GraphViewNodeData)node.userData).guid,
                pos = node.GetPosition().position,
                type = ((GraphViewNodeData)node.userData).voxelNodeType,                
            };
            if (((GraphViewNodeData)node.userData).voxelNode is VNConstants) 
            {
                savedNode.value = ((VNConstants)((GraphViewNodeData)node.userData).voxelNode).objValue;
            }
            densityGraph.nodes.Add(savedNode);
        }
        //Edges
        foreach (var edge in graph.edges)
        {
            SavedVoxelEdge savedEdge = new SavedVoxelEdge()
            {
                nodeGUID = (((GraphViewNodeData)(edge.input.node.userData))).guid,
                nodeGUID1 = (((GraphViewNodeData)(edge.output.node.userData))).guid,
                localPortCount = (((GraphViewPortData)(edge.input.userData))).localPortCount,
                localPortCount1 = (((GraphViewPortData)(edge.output.userData))).localPortCount,
            };
            densityGraph.edges.Add(savedEdge);
        }
        //Make sure to save
        EditorUtility.SetDirty(this);
    }
}

/// <summary>
/// A saved voxel graph
/// </summary>
[System.Serializable]
public class SavedVoxelGraph 
{
    //Main variables
    public List<SavedVoxelNode> nodes;
    public List<SavedVoxelEdge> edges;
}

/// <summary>
/// A node that was saved in the SavedVoxelGraph
/// </summary>
[System.Serializable]
public class SavedVoxelNode 
{
    //Main variables
    public string guid;
    public Vector2 pos;
    public int type;

    //Optional value for constant numbers
    public object value;
}

/// <summary>
/// An edge that was saved in the SavedVoxelGraph
/// </summary>
[System.Serializable]
public class SavedVoxelEdge
{
    //Main variables
    public string nodeGUID;
    public string nodeGUID1;
    public int localPortCount;
    public int localPortCount1;
}