using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static GenerationGraphUtility;
/// <summary>
/// The graph view that handles the nodes
/// </summary>
public class GenerationGraphView : GraphView
{
    //Main variables
    private readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    private readonly List<VoxelNodeType> voxelsNodeTypes = GetAll();

    /// <summary>
    /// Constructor
    /// </summary>
    public GenerationGraphView()
    {
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        CreateNode(Vector2.zero, typeof(VNInput));
        CreateNode(new Vector2(200, 0), typeof(VNResult));
        graphViewChanged = OnGraphChange;
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
                if (voxelNodeType is VNInput || voxelNodeType is VNResult) continue;
                evt.menu.AppendAction("Create Node/" + voxelNodeType.name, (e) =>
                {
                    CreateNode(Vector2.zero, voxelNodeType.GetType());
                });
            }
        }

        base.BuildContextualMenu(evt);
    }

    /// <summary>
    /// Generate a single node with a specified voxel node type
    /// <summary>
    private void CreateNode(Vector2 pos, Type type) 
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
        if (nodeType == typeof(VNInput) || nodeType == typeof(VNResult))
        {
            node.capabilities &= ~Capabilities.Deletable;
        }
        node.title = customData.Item1;
        foreach (var item in customData.Item2) node.inputContainer.Add(item);
        foreach (var item in customData.Item3) node.outputContainer.Add(item);

        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(pos, defaultNodeSize));
        this.AddElement(node);
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
                //Checking if this port is a CSM port or not
                if (((VoxelPortData)startPort.userData).csmPort == ((VoxelPortData)port.userData).csmPort || (!((VoxelNodeData)(startPort.node).userData).connected && !((VoxelNodeData)(port.node).userData).connected))
                {
                    compatiblePorts.Add((port));
                }                
            }
        }));
        return compatiblePorts;
    }

    /// <summary>
    /// Check if the selection is valid and delete it if it is
    /// </summary>
    /// <returns></returns>
    public override EventPropagation DeleteSelection()
    {
        return base.DeleteSelection();
    }

    /// <summary>
    /// When the graph changes
    /// </summary>
    private GraphViewChange OnGraphChange(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate)
            {
                //Foreach edge to add
                ((VoxelNodeData)edge.input.node.userData).connected = true;
                ((VoxelNodeData)edge.output.node.userData).connected = true;

                ((VoxelPortData)edge.input.userData).csmPort = ((VoxelPortData)edge.output.userData).csmPort;
            }
        }

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement e in change.elementsToRemove)
            {
                if (e.GetType() == typeof(Edge)) 
                {
                    //Foeach edge to remove
                    Edge edge = (Edge)e;

                    ((VoxelNodeData)edge.input.node.userData).connected = false;
                    ((VoxelNodeData)edge.output.node.userData).connected = false;

                    ((VoxelPortData)edge.input.userData).csmPort = false;
                }
            }
        }

        if (change.movedElements != null)
        {
            foreach (GraphElement e in change.movedElements)
            {
                if (e.GetType() == typeof(Node))
                {
                    //Foreach node that was moved
                    Node node = (Node)e;
                }
            }
        }

        return change;
    }
}
