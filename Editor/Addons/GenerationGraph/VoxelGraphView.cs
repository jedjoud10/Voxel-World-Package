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
    private readonly List<VoxelNodeType> voxelsNodeTypes = GetAllVoxelNodeTypes();
    public bool normalGraph;

    /// <summary>
    /// Constructor
    /// </summary>
    public VoxelGraphView(bool normalGraph, Vector3 viewTransform)
    {
        this.viewTransform.position = viewTransform;
        StyleSheet styleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.jedjoud.voxelworld/Editor/Addons/Resources/VoxelGraphStyleSheet.uss", typeof(StyleSheet));
        styleSheets.Add(styleSheet);
        this.normalGraph = normalGraph;
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        if (normalGraph) CreateNode(new Vector2(0, 0), typeof(VNNormalResult));
        else CreateNode(new Vector2(0, 0), typeof(VNResult));

        var gridBackground = new GridBackground();
        Insert(0, gridBackground);
        gridBackground.StretchToParentSize();
    }

    /// <summary>
    /// Build a custom context menu
    /// </summary>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //Custom context menu items
        if (evt.target is GraphView)
        {
            foreach (VoxelNodeType voxelNodeType in voxelsNodeTypes)
            {
                if (voxelNodeType is VNResult || voxelNodeType is VNNormalResult) continue;
                DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;
                if ((voxelNodeType.GetType() == typeof(VNNormal) && !normalGraph) || (voxelNodeType.GetType() == typeof(VNDensity) && !normalGraph)) status = DropdownMenuAction.Status.Disabled;
                evt.menu.AppendAction("Create Node/" + voxelNodeType.name, (e) =>
                {
                    CreateNode(e.eventInfo.localMousePosition - new Vector2(this.viewTransform.position.x, this.viewTransform.position.y), voxelNodeType.GetType());
                }, status);
            }

            evt.menu.AppendAction("Debug/Debug Nodes", (e) =>
            {
                foreach (var item in nodes)
                {
                    VoxelNodeData data = (VoxelNodeData)item.userData;
                    VNConstantFloat constant = data.obj as VNConstantFloat;
                    if(constant != null) Debug.Log(constant.value);
                }                
            });
        }

        base.BuildContextualMenu(evt);
    }
    /// <summary>
    /// Generate a single node with a specified voxel node type
    /// <summary>
    private VoxelNodeType CreateNode(Vector2 pos, Type type, List<Port> ports = null) 
    {
        var node = new Node
        {
            userData = new VoxelNodeData() 
            {
                GUID = Guid.NewGuid().ToString(),
                obj = Activator.CreateInstance(type) as VoxelNodeType
            },
        };      

        //Generate the output ports
        var customData = ((VoxelNodeData)node.userData).obj.GetCustomNodeData(node);
        Type nodeType = ((VoxelNodeData)node.userData).obj.GetType();
        if (nodeType == typeof(VNResult) || nodeType == typeof(VNNormalResult)) node.capabilities &= ~Capabilities.Deletable;
        node.title = customData.Item1;
        foreach (var item in customData.Item2) node.inputContainer.Add(item);
        foreach (var item in customData.Item3) node.outputContainer.Add(item);

        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(pos, defaultNodeSize));
        this.AddElement(node);
        return ((VoxelNodeData)node.userData).obj;
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
