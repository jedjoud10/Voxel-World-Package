using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static VoxelGraphUtility;
/// <summary>
/// The graph view that handles the nodes
/// </summary>
public class VoxelGraphView : GraphView
{
    //Main variables
    private readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    private readonly List<VoxelNode> voxelsNodeTypes = GetAllVoxelNodeTypes();
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
    /// Load the voxel graph from a saved voxel graph
    /// </summary>
    public void LoadVoxelGraph(SavedVoxelGraph savedVoxelGraph) 
    {
        //Create the nodes
        Dictionary<string, Node> dictionaryNodes = new Dictionary<string, Node>();
        Dictionary<string, Port> portData = new Dictionary<string, Port>();
        foreach (var savedVoxelNode in savedVoxelGraph.nodes)
        {
            Node newNode = CreateNode(savedVoxelNode.pos, voxelsNodeTypes[savedVoxelNode.type].GetType(), guid: savedVoxelNode.guid, objValue: savedVoxelNode.value, savedPorts: savedVoxelNode.savedPorts);
            dictionaryNodes.Add(savedVoxelNode.guid, newNode);
            VoxelNode voxelNode = ((GraphViewNodeData)newNode.userData).voxelNode;
            foreach (var port in voxelNode.ports) portData.Add(((GraphViewPortData)port.Value.userData).portguid, port.Value);
        }

        //Create the edges
        foreach (var savedVoxelEdge in savedVoxelGraph.edges)
        {
            Port input = portData[savedVoxelEdge.input.portguid];
            Port output = portData[savedVoxelEdge.output.portguid];
            Edge newEdge = new Edge() { input = input, output = output };
            input.Connect(newEdge);
            output.Connect(newEdge);
            this.Add(newEdge);
        }
        
    }

    /// <summary>
    /// Build a custom context menu
    /// </summary>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //Custom context menu items
        if (evt.target is GraphView)
        {
            foreach (VoxelNode voxelNodeType in voxelsNodeTypes)
            {
                if (voxelNodeType is VNResult || voxelNodeType is VNCSMResult || voxelNodeType is VNVoxelDetailsResult) continue;
                DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;

                switch (voxelGraphType)
                {
                    case VoxelGraphType.Density:
                        if (voxelNodeType is VNNormal || voxelNodeType is VNDensity || voxelNodeType is VNSurfacePosition || voxelNodeType is VNSurfaceNormal) status = DropdownMenuAction.Status.Disabled;
                        break;
                    case VoxelGraphType.CSM:
                        if (voxelNodeType is VNSurfacePosition || voxelNodeType is VNSurfaceNormal) status = DropdownMenuAction.Status.Disabled;
                        break;
                    case VoxelGraphType.VoxelDetails:
                        break;
                    default:
                        break;
                }

                evt.menu.AppendAction("Create Node/" + voxelNodeType.name, (e) =>
                {
                    CreateNode(e.eventInfo.localMousePosition - new Vector2(this.viewTransform.position.x, this.viewTransform.position.y), voxelNodeType.GetType());
                }, status);
            }

            evt.menu.AppendAction("Debug/Debug Nodes", (e) => DebugNodes() );
        }

        base.BuildContextualMenu(evt);
    }

    /// <summary>
    /// Generate a single node with a specified voxel node type
    /// <summary>
    private Node CreateNode(Vector2 pos, Type type, string guid = null, object objValue = null, List<string> savedPorts = null) 
    {
        VoxelNode voxelNode = Activator.CreateInstance(type) as VoxelNode;
        if (objValue != null && voxelNode is VNConstants) ((VNConstants)voxelNode).objValue = objValue;
        //Main data
        GraphViewNodeData data = new GraphViewNodeData()
        {
            guid = guid == null ? Guid.NewGuid().ToString() : guid,
            voxelNode = voxelNode,
            voxelNodeType = voxelsNodeTypes.FindIndex((x) => x.GetType() == type)
        };

        voxelNode.Setup(data.guid, savedPorts);
        //Set the custom const voxel node data

        var node = new Node
        {
            userData = data,
        };      

        //Generate the output ports
        var customData = data.voxelNode.GetCustomNodeData(node);
        Type nodeType = data.voxelNode.GetType();
        if (nodeType == typeof(VNResult) || nodeType == typeof(VNCSMResult) || nodeType == typeof(VNVoxelDetailsResult)) node.capabilities &= ~Capabilities.Deletable;
        node.title = customData.Item1;
        foreach (var item in customData.Item2) node.inputContainer.Add(item);
        foreach (var item in customData.Item3) node.outputContainer.Add(item);

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
