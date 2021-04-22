using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static VoxelUtility;
using static VoxelGraphUtility;
using System;
using UnityEngine.UIElements;
/// <summary>
/// Voxel node data
/// </summary>
public class GraphViewNodeData
{
    //Main variables
    public string guid;
    public int voxelNodeType;
    public VoxelNode voxelNode;
}

/// <summary>
/// Voxel port data
/// </summary>
public class GraphViewPortData
{
    //Main variables
    public string portguid;
    public int localPortIndex;
}