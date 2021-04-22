using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using static VoxelGraphUtility;
/// <summary>
/// A scriptable object for the voxel graphs
/// </summary>
[CreateAssetMenu(fileName = "NewVoxelGraph", menuName = "Voxel World/Voxel Graph")]
[System.Serializable]
public class VoxelGraphSO : ScriptableObject
{
    //[HideInInspector]
    public SavedVoxelGraph densityGraph;
    //[HideInInspector]
    public SavedVoxelGraph csmGraph;
    //[HideInInspector]
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
            var defaultNode = new SavedVoxelNode()
            {
                pos = Vector2.zero,
                type = 5,
                value = null
            };
            obj.densityGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { GUID.Generate().ToString(), defaultNode } };
            obj.densityGraph.edges = new Dictionary<string, SavedVoxelEdge>();
        }
        //Generate the default CSM graph if it wasn't generated yet
        if (obj.csmGraph == null || obj.csmGraph.nodes == null)
        {
            obj.csmGraph = new SavedVoxelGraph();
            var defaultNode = new SavedVoxelNode()
            {
                pos = Vector2.zero,
                type = 6,
                value = null
            };
            obj.csmGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { GUID.Generate().ToString(), defaultNode } };
            obj.csmGraph.edges = new Dictionary<string, SavedVoxelEdge>();
        }
        //Generate the default VoxelDetails graph if it wasn't generated yet
        if (obj.voxelDetailsGraph == null || obj.voxelDetailsGraph.nodes == null)
        {
            obj.voxelDetailsGraph = new SavedVoxelGraph();
            var defaultNode = new SavedVoxelNode()
            {
                pos = Vector2.zero,
                type = 7,
                value = null
            };
            obj.voxelDetailsGraph.nodes = new Dictionary<string, SavedVoxelNode>(1) { { GUID.Generate().ToString(), defaultNode } };
            obj.voxelDetailsGraph.edges = new Dictionary<string, SavedVoxelEdge>();
        }
        VoxelGraphEditorWindow.OpenGraphWindow(obj);
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
        savedVoxelGraph.nodes = new Dictionary<string, SavedVoxelNode>();
        savedVoxelGraph.edges = new Dictionary<string, SavedVoxelEdge>();
        //Nodes
        foreach (var node in graph.nodes)
        {
            var nodeData = ((GraphViewNodeData)node.userData);
            SavedVoxelNode savedNode = new SavedVoxelNode()
            {
                pos = node.GetPosition().position,
                type = ((GraphViewNodeData)node.userData).voxelNodeType,   
                savedPorts = new List<string>(),
            };

            //Save ports
            foreach (var port in nodeData.voxelNode.savedPorts) savedNode.savedPorts.Add(port);


            //Save constant value
            if (((GraphViewNodeData)node.userData).voxelNode is VNConstants) 
            {
                savedNode.value = ((VNConstants)((GraphViewNodeData)node.userData).voxelNode).objValue;
            }
            savedVoxelGraph.nodes.Add(nodeData.guid, savedNode);
        }
        //Edges
        foreach (var edge in graph.edges)
        {
            SavedVoxelEdge savedEdge = new SavedVoxelEdge()
            {
                input = new SavedVoxelPort()
                {
                    portguid = ((GraphViewPortData)(edge.input.userData)).portguid,
                    nodeguid = ((GraphViewNodeData)edge.input.node.userData).guid
                },
                output = new SavedVoxelPort()
                {
                    portguid = ((GraphViewPortData)(edge.output.userData)).portguid,
                    nodeguid =  ((GraphViewNodeData)edge.output.node.userData).guid
                },
            };
            savedVoxelGraph.edges.Add(savedEdge.input.nodeguid, savedEdge);
        }
        savedVoxelGraph.SaveDictionaries();
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
    public Dictionary<string, SavedVoxelNode> nodes;//Uses Node GUID
    public Dictionary<string, SavedVoxelEdge> edges;//Uses input port GUID

    //Godamnit Unity why aren't dictionaries serializable
    public List<string> node_keys, edges_keys;
    public List<SavedVoxelNode> node_values;
    public List<SavedVoxelEdge> edges_values;

    public void SaveDictionaries() 
    {
        node_keys = nodes.Keys.ToList();
        node_values = nodes.Values.ToList();

        edges_keys = edges.Keys.ToList();
        edges_values = edges.Values.ToList();
    }
    public void LoadDictionaries() 
    {
        //https://stackoverflow.com/questions/4038978/map-two-lists-into-a-dictionary-in-c-sharp
        if (node_keys == null) { node_keys = new List<string>(); node_values = new List<SavedVoxelNode>(); }
        if (edges_keys == null) { edges_keys = new List<string>(); edges_values = new List<SavedVoxelEdge>(); } ;

        nodes ??= node_keys.Select((k, i) => new { k, v = node_values[i] }).ToDictionary(x => x.k, x => x.v);
        edges ??= edges_keys.Select((k, i) => new { k, v = edges_values[i] }).ToDictionary(x => x.k, x => x.v);
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
    public int type;
    public List<string> savedPorts;

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
    public string nodeguid;
    public string portguid;
}