using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static VoxelGraphUtility;
using static VoxelSavedGraphUtility;
/// <summary>
/// The graph view that handles the nodes
/// </summary>
public class VoxelGraphView : GraphView
{
    //Main variables
    private readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    private readonly List<Type> voxelsNodeTypes = voxelNodesTypes;
    public VoxelGraphType voxelGraphType;

    /// <summary>
    /// Constructor
    /// </summary>
    public VoxelGraphView(VoxelGraphType voxelGraphType, Vector3 viewTransform)
    {
        this.viewTransform.position = viewTransform;
        StyleSheet styleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.jedjoud.voxelworld/Editor/Addons/Resources/VoxelGraphStyleSheet.uss", typeof(StyleSheet));
        styleSheets.Add(styleSheet);
        this.voxelGraphType = voxelGraphType;
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var gridBackground = new GridBackground();
        Insert(0, gridBackground);
        gridBackground.StretchToParentSize();
    }

    /// <summary>
    /// Load the local voxel graph from a saved voxel graph
    /// </summary>
    public void LoadLocalVoxelGraph(SavedLocalVoxelGraph savedVoxelGraph) 
    {
        //Create the nodes
        Dictionary<string, Node> dictionaryNodes = new Dictionary<string, Node>();
        Dictionary<string, Port> portData = new Dictionary<string, Port>();
        foreach (var savedVoxelNode in savedVoxelGraph.nodes)
        {
            Node newNode = CreateNode(
                new Vector2(savedVoxelNode.Value.posx, savedVoxelNode.Value.posy),
                type: savedVoxelNode.Value.nodeData.voxelNode.GetType(),
                graphViewNodeData: savedVoxelNode.Value.nodeData);
            dictionaryNodes.Add(savedVoxelNode.Key, newNode);
            VoxelNode voxelNode = ((GraphViewNodeData)newNode.userData).voxelNode;
            foreach (var port in voxelNode.ports) portData.Add(((GraphViewPortData)port.Value.userData).portGuid, port.Value);
        }

        //Create the edges
        foreach (var savedVoxelEdge in savedVoxelGraph.edges)
        {
            Port input = portData[savedVoxelEdge.Value.input.portGuid];
            Port output = portData[savedVoxelEdge.Value.output.portGuid];
            Edge newEdge = new Edge() { input = input, output = output };
            input.Connect(newEdge);
            output.Connect(newEdge);
            this.Add(newEdge);
        }        
    }

    /// <summary>
    /// Save this specific graph view
    /// </summary>
    public SavedLocalVoxelGraph SaveLocalVoxelGraph()
    {        
        SavedLocalVoxelGraph savedVoxelGraph = new SavedLocalVoxelGraph();
        //Nodes
        foreach (var node in nodes)
        {
            var graphViewNodeData = ((GraphViewNodeData)node.userData);
            SavedVoxelNode savedNode = new SavedVoxelNode()
            {
                posx = node.GetPosition().position.x,
                posy = node.GetPosition().position.y,
                guid = graphViewNodeData.guid,
                nodeData = graphViewNodeData
            };

            savedVoxelGraph.nodes.Add(graphViewNodeData.guid, savedNode);
        }
        //Edges
        foreach (var edge in edges)
        {
            SavedVoxelEdge savedEdge = new SavedVoxelEdge()
            {
                input = new SavedVoxelPort()
                {
                    portGuid = ((GraphViewPortData)(edge.input.userData)).portGuid,
                    nodeGuid = ((GraphViewNodeData)edge.input.node.userData).guid
                },
                output = new SavedVoxelPort()
                {
                    portGuid = ((GraphViewPortData)(edge.output.userData)).portGuid,
                    nodeGuid = ((GraphViewNodeData)edge.output.node.userData).guid
                },
            };
            savedVoxelGraph.edges.Add(savedEdge.input.portGuid, savedEdge);
        }

        return savedVoxelGraph;
    }

    /// <summary>
    /// Save a specific voxel graph after asking the user
    /// </summary>
    public SavedLocalVoxelGraph SaveLocalVoxelGraph(SavedLocalVoxelGraph currentSLVG, string message, out bool save)
    {
        SavedLocalVoxelGraph newSLVG = new SavedLocalVoxelGraph();
        newSLVG = SaveLocalVoxelGraph();

        //Check if we have a different count, if so then it means it's different
        bool differentCount = (newSLVG.nodes.Count != currentSLVG.nodes.Count || newSLVG.edges.Count != currentSLVG.edges.Count);
        bool differentElements = false;

        SavedVoxelNodeComparer comparer = new SavedVoxelNodeComparer();
        differentElements = !newSLVG.nodes.SequenceEqual(currentSLVG.nodes, comparer);
        bool different = differentCount || differentElements;

        save = false;

        if (different)
        {
            save = EditorUtility.DisplayDialog("Want to save?", message, "Yes", "No");
            return newSLVG;
        }
        else
        {
            return newSLVG;
        }

        return null;
    }

    /// <summary>
    /// Build a custom context menu
    /// </summary>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //Custom context menu items
        if (evt.target is GraphView)
        {
            for (int i = 0; i < voxelsNodeTypes.Count; i++)
            {
                Type voxelNodeType = voxelNodesTypes[i];
                VoxelNode templateVoxelNode = voxelNodes[i];
                if (voxelNodeType == typeof(VNResult) || voxelNodeType == typeof(VNCSMResult) || voxelNodeType == typeof(VNVoxelDetailsResult)) continue;
                DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;

                switch (voxelGraphType)
                {
                    case VoxelGraphType.Density:
                        if (voxelNodeType == typeof(VNNormal) || voxelNodeType == typeof(VNDensity) || voxelNodeType == typeof(VNSurfacePosition) || voxelNodeType == typeof(VNSurfaceNormal)) status = DropdownMenuAction.Status.Disabled;
                        break;
                    case VoxelGraphType.CSM:
                        if (voxelNodeType == typeof(VNSurfacePosition) || voxelNodeType == typeof(VNSurfaceNormal)) status = DropdownMenuAction.Status.Disabled;
                        break;
                    case VoxelGraphType.VoxelDetails:
                        break;
                    default:
                        break;
                }

                evt.menu.AppendAction("Create Node/" + templateVoxelNode.name, (e) =>
                {
                    CreateNode(e.eventInfo.localMousePosition - new Vector2(this.viewTransform.position.x, this.viewTransform.position.y), voxelNodeType);
                }, status);
            }
            evt.menu.AppendAction("Debug/Debug Nodes", (e) => DebugNodes() );
        }

        base.BuildContextualMenu(evt);
    }

    /// <summary>
    /// Generate a single node with a specified voxel node type
    /// <summary>
    private Node CreateNode(Vector2 pos, Type type = null, GraphViewNodeData graphViewNodeData = null) 
    {
        GraphViewNodeData data;
        if (graphViewNodeData == null) 
        {
            VoxelNode voxelNode = (VoxelNode)Activator.CreateInstance(type);
            //Main data
            data = new GraphViewNodeData()
            {
                guid = Guid.NewGuid().ToString(),
                voxelNode = voxelNode,
            };
            voxelNode.Setup(data.guid);
        }
        else data = graphViewNodeData;

        Node node = new Node()
        {            
            userData = data
        };

        //Generate the output ports
        var customData = data.voxelNode.GetCustomNodeData(data.voxelNode);
        Type nodeType = data.voxelNode.GetType();
        if (nodeType == typeof(VNResult) || nodeType == typeof(VNCSMResult) || nodeType == typeof(VNVoxelDetailsResult)) node.capabilities &= ~Capabilities.Deletable;
        data.voxelNode.title = customData.Item1;
        foreach (var item in customData.Item2) data.voxelNode.inputContainer.Add(item);
        foreach (var item in customData.Item3) data.voxelNode.outputContainer.Add(item);

        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(pos, defaultNodeSize));
        this.AddElement(node);
        return node;
    }

    /// <summary>
    /// For debbugging purposes
    /// </summary>
    private void DebugNodes() 
    {
        foreach (var item in nodes)
        {
            GraphViewNodeData data = (GraphViewNodeData)item.userData;
            if (data.voxelNode is VNConstants) Debug.Log(((VNConstants)data.voxelNode).objValue);
        }
    }

    /// <summary>
    /// Get the compatible ports that a port can connect to
    /// </summary>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port => 
        {
            if (
            startPort != port &&
            startPort.node != port.node &&
            startPort.portType == port.portType &&
            startPort.direction != port.direction) 
            {
                compatiblePorts.Add((port));    
            }
        }));
        return compatiblePorts;
    }
}
