using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using static VoxelGraphUtility;
using static VoxelSavedGraphUtility;
using System;
/// <summary>
/// A scriptable object for the voxel graphs
/// </summary>
[CreateAssetMenu(fileName = "NewVoxelGraph", menuName = "Voxel World/Voxel Graph")]
[System.Serializable]
public class VoxelGraphSO : ScriptableObject
{
    public SavedGlobalVoxelGraph globalVoxelGraph;

    /// <summary>
    /// Opens the voxel graph when double clicking the asset
    /// </summary>
    [OnOpenAsset(0)]
    public static bool OpenVoxelGraph(int instanceID, int line)
    {
        VoxelGraphSO obj = (VoxelGraphSO)EditorUtility.InstanceIDToObject(instanceID);
        if (!obj.globalVoxelGraph.defaultSet) obj.globalVoxelGraph.SetDefaultVars();
        VoxelGraphEditorWindow.OpenGraphWindow(obj);
        return false;
    }
    /// <summary>
    /// Wrapper around the saved global voxel graph to save part of it
    /// </summary>
    public void Save(VoxelGraphView graph, VoxelGraphType type) 
    {
        graph.SaveLocalVoxelGraph(globalVoxelGraph[type]);
        //Make sure to save
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Wrapper around the saved global voxel graph to save part of it
    /// </summary>
    public void SaveAskUser(VoxelGraphView graph, VoxelGraphType type, string message)
    {
        if (graph != null && globalVoxelGraph != null)
        {
            graph.SaveLocalVoxelGraph(globalVoxelGraph[type], message);
            //Make sure to save
            EditorUtility.SetDirty(this);
        }
    }

    private void OnAfterAssemblyReload()
    {
        if (!globalVoxelGraph.defaultSet) globalVoxelGraph.SetDefaultVars();
        VoxelGraphEditorWindow.OpenGraphWindow(this);
    }

    public void OnEnable()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }


    public void OnDisable()
    {
        AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
    }

}