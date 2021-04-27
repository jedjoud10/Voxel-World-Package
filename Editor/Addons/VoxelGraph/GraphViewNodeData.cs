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
[System.Serializable]
public class GraphViewNodeData
{
    //Main variables
    public string guid;
    public VoxelNode voxelNode;
}

/// <summary>
/// Voxel port data
/// </summary>
[System.Serializable]
public class GraphViewPortData
{
    //Main variables
    public string portGuid;
    public int localPortIndex;
}