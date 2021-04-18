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
        CreateNode(Vector2.zero, typeof(VNVoxel));
        CreateNode(new Vector2(200, 0), typeof(VNResult));
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
                if (voxelNodeType is VNVoxel || voxelNodeType is VNResult) continue;
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
        var node = new GenerationNode
        {
            GUID = Guid.NewGuid().ToString(),
            obj = Activator.CreateInstance(type) as VoxelNodeType,
        };

        //Generate the output ports
        var customData = node.obj.GetCustomNodeData(node);
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
            if (startPort != port && startPort.node!= port.node && startPort.portType == port.portType) compatiblePorts.Add(port);
        }));
        return compatiblePorts;
    }
}
