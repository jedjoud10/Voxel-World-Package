using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Jedjoud.VoxelWorld;
[CustomEditor(typeof(VoxelWorld))]
public class CustomEditorVoxelWorld : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VoxelWorld voxelWorld = (VoxelWorld)target;

        //Caclulate the globalWorldSize
        float globalWorldSize = Mathf.Pow(2, voxelWorld.octreeManager.maxHierarchyIndex) * (VoxelWorld.resolution-3) * VoxelWorld.voxelSize;
        string sign = "m";
        //When the terrain is in km
        if (globalWorldSize > 1000)
        {
            globalWorldSize /= 1000.0f;
            sign = "km";
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Global World Size: " + globalWorldSize + sign);
    }
}
