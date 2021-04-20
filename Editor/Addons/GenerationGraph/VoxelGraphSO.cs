using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
/// <summary>
/// A scriptable object for the voxel graphs
/// </summary>
public class VoxelGraphSO : ScriptableObject
{
    public Node[] nodes;
    public Edge[] edges;

    [OnOpenAssetAttribute(0)]
    public static bool OpenVoxelGraph(int instanceID, int line)
    {
        string name = EditorUtility.InstanceIDToObject(instanceID).name;
        Debug.Log("Open Asset step: 1 (" + name + ")");
        return false; // we did not handle the open
    }
}
