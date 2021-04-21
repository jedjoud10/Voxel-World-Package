using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static VoxelGraphUtility;
/// <summary>
/// Actual editor window that handles the editor stuff
/// </summary>
public class VoxelGraphEditorWindow : EditorWindow
{
    //Main variables
    private VoxelGraphView currentGraphView;
    private VisualElement graphViewsHolder;
    private VoxelGraphSO voxelGraphSOData;
    private VoxelGraphType currentVoxelGraphType;

    /// <summary>
    /// Actually creates the graph window
    /// </summary>
    public static void OpenGraphWindow(VoxelGraphSO voxelGraphSOData) 
    {
        var window = GetWindow<VoxelGraphEditorWindow>();
        window.voxelGraphSOData = voxelGraphSOData;
        window.titleContent = new GUIContent("Generation Graph");
        window.GenerateWindow();
        window.SwitchGraphView("Density Graph", VoxelGraphType.Density);
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void SwitchGraphView(string name, VoxelGraphType voxelGraphType) 
    {
        currentVoxelGraphType = voxelGraphType;
        if (graphViewsHolder.childCount > 0) graphViewsHolder.Remove(currentGraphView);

        var graphView = new VoxelGraphView(voxelGraphType, currentGraphView == null ? Vector3.zero : currentGraphView.viewTransform.position) { name = name, };

        switch (voxelGraphType)
        {
            case VoxelGraphType.Density:
                graphView.LoadVoxelGraph(voxelGraphSOData.densityGraph);
                break;
            case VoxelGraphType.CSM:
                graphView.LoadVoxelGraph(voxelGraphSOData.csmGraph);
                break;
            case VoxelGraphType.VoxelDetails:
                graphView.LoadVoxelGraph(voxelGraphSOData.voxelDetailsGraph);
                break;
            default:
                break;
        }

        currentGraphView = graphView;
        graphView.StretchToParentSize();
        graphViewsHolder.Add(graphView);
    }

    /// <summary>
    /// Generate the window
    /// </summary>
    private void GenerateWindow()
    {
        //Generate the toolbar
        var toolbar = new Toolbar();
        var saveButton = new Button(() => voxelGraphSOData.SaveVoxelGraph(currentGraphView, currentVoxelGraphType)) { text = "Save Graph" };
        var generateShaderButton = new Button(() => 
        {
            string path = EditorUtility.SaveFilePanel("Generate compute shader", "Assets/", "DefaultComputeShader.compute", "compute");
            CodeConverter.ConvertAndSave(voxelGraphSOData, path);
        }) { text = "Generate Shader" };

        var switchToDensityGraph = new Button(() => SwitchGraphView("Density Graph", VoxelGraphType.Density)) { text = "Switch to Density Graph" };
        var switchToNormalGraph = new Button(() => SwitchGraphView("Normal Graph", VoxelGraphType.CSM)) { text = "Switch to Color/Smoothness and Metallic Graph" };
        var switchToVoxelDetailsGraph = new Button(() => SwitchGraphView("VoxelDetails Graph", VoxelGraphType.VoxelDetails)) { text = "Switch to VoxelDetails Graph" };
        //Add the buttons to the toolbar
        toolbar.Add(saveButton);
        toolbar.Add(generateShaderButton);

        toolbar.Add(switchToDensityGraph);
        toolbar.Add(switchToNormalGraph);
        toolbar.Add(switchToVoxelDetailsGraph);

        //Generate the graphViewsHolder
        graphViewsHolder = new VisualElement();
        graphViewsHolder.StretchToParentSize();        
        rootVisualElement.Add(graphViewsHolder);
        rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// When this window is destroyed, remove the splitViewer (Contains the two voxel graphs)
    /// </summary>
    private void OnDisable()
    {
        rootVisualElement.Remove(graphViewsHolder);
    }
}
