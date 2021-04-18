using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static VoxelUtility;
using static GenerationGraphUtility;
/// <summary>
/// A single generation node
/// </summary>
public class GenerationNode : Node
{
    public string GUID;
    public VoxelNodeType obj;
}
