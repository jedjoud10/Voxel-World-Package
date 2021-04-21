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
    [HideInInspector]
    public SavedVoxelGraph densityGraph;
    [HideInInspector]
    public SavedVoxelGraph csmGraph;
    [HideInInspector]
    public SavedVoxelGraph voxelDetailsGraph;

    /// <summary>
    /// Opens the voxel graph when double clicking the asset
    /// </summary>
    [OnOpenAsset(0)]
    public static bool OpenVoxelGraph(int instanceID, int line)
    {
        VoxelGraphSO obj = (VoxelGraphSO)EditorUtility.InstanceIDToObject(instanceID);
        //Generate the default Density graph if it wasn't generated yet
        if (obj.densityGraph == null || obj.densityGraph.nodes == null) 
        {
            obj.densityGraph = new SavedVoxelGraph();
            obj.densityGraph.nodes = new List<SavedVoxelNode>(1) { new SavedVoxelNode() 
            {
                guid = GUID.Generate().ToString(),
                pos = Vector2.zero,
                type = 5,
                value = null
            } };
            obj.densityGraph.edges = new List<SavedVoxelEdge>();
        }
        //Generate the default CSM graph if it wasn't generated yet
        if (obj.csmGraph == null || obj.csmGraph.nodes == null)
        {
            obj.csmGraph = new SavedVoxelGraph();
            obj.csmGraph.nodes = new List<SavedVoxelNode>(1) { new SavedVoxelNode()
            {
                guid = GUID.Generate().ToString(),
                pos = Vector2.zero,
                type = 6,
                value = null
            } };
            obj.csmGraph.edges = new List<SavedVoxelEdge>();
        }
        //Generate the default VoxelDetails graph if it wasn't generated yet
        if (obj.voxelDetailsGraph == null || obj.voxelDetailsGraph.nodes == null)
        {
            obj.voxelDetailsGraph = new SavedVoxelGraph();
            obj.voxelDetailsGraph.nodes = new List<SavedVoxelNode>(1) { new SavedVoxelNode()
            {
                guid = GUID.Generate().ToString(),
                pos = Vector2.zero,
                type = 7,
                value = null
            } };
            obj.voxelDetailsGraph.edges = new List<SavedVoxelEdge>();
        }
        VoxelGraph.OpenGraphWindow(obj);
        return false;
    }

    /// <summary>
    /// Save a specific voxel graph
    /// </summary>
    public void SaveVoxelGraph(VoxelGraphView graph, VoxelGraphType voxelGraphType) 
    {
        SavedVoxelGraph savedVoxelGraph = densityGraph;
        switch (voxelGraphType)
        {
            case VoxelGraphType.Density:
                savedVoxelGraph = densityGraph;
                break;
            case VoxelGraphType.CSM:
                savedVoxelGraph = csmGraph;
                break;
            case VoxelGraphType.VoxelDetails:
                savedVoxelGraph = voxelDetailsGraph;
                break;
            default:
                break;
        }
        savedVoxelGraph.nodes = new List<SavedVoxelNode>();
        savedVoxelGraph.edges = new List<SavedVoxelEdge>();
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
            savedVoxelGraph.nodes.Add(savedNode);
        }
        //Edges
        foreach (var edge in graph.edges)
        {
            SavedVoxelEdge savedEdge = new SavedVoxelEdge()
            {
                inputNodeGUID = (((GraphViewNodeData)(edge.input.node.userData))).guid,
                outputNodeGUID = (((GraphViewNodeData)(edge.output.node.userData))).guid,
                localPortCountInput = (((GraphViewPortData)(edge.input.userData))).localPortCount,
                localPortCountOutput = (((GraphViewPortData)(edge.output.userData))).localPortCount,
            };
            savedVoxelGraph.edges.Add(savedEdge);
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
    public string inputNodeGUID;
    public string outputNodeGUID;
    public int localPortCountInput;
    public int localPortCountOutput;
}