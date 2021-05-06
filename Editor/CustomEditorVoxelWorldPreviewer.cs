using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Jedjoud.VoxelWorld;
[CustomEditor(typeof(VoxelWorldPreviewer))]
public class CustomEditorVoxelWorldPreviewer : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VoxelWorldPreviewer previewer = (VoxelWorldPreviewer)target;
        EditorGUILayout.Space();
        if (previewer.previewTexture == null) return;
        previewer.UpdateTexture();
        EditorGUI.DrawPreviewTexture(Rect.zero, previewer.previewTexture);
    }
}
