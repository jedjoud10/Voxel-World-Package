using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Jedjoud.VoxelWorld;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
//Custom editor window that allows us to manage a selected voxel world
public class VoxelWorldManagerEditorWindow : EditorWindow
{
    //Main voxel world ref
    public VoxelWorld voxelWorld;
    [MenuItem("Voxel World/ Voxel World Manager")]
    public static void ShowWindow()
    {
        var window = (VoxelWorldManagerEditorWindow)GetWindow(typeof(VoxelWorldManagerEditorWindow));
        if (Selection.activeGameObject.GetComponent<VoxelWorld>() != null) window.voxelWorld = Selection.activeGameObject.GetComponent<VoxelWorld>();
        if (window.voxelWorld == null) Debug.LogError("You have to select a VoxelWorld before opening the window!");
    }

    //Main code goes here
    private void OnEnable()
    {
        
    }
}
