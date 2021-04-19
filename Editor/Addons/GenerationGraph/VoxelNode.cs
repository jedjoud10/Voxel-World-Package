using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static VoxelUtility;
using static GenerationGraphUtility;
using System;
using UnityEngine.UIElements;
/// <summary>
/// Voxel node data
/// </summary>
public class VoxelNodeData
{
    //Main variables
    public string GUID;
    public VoxelNodeType obj;
    public bool connected;
}

/// <summary>
/// Voxel port data
/// </summary>
public class VoxelPortData
{
    //Main variables
    public bool csmPort;
}
