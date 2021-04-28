using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static VoxelGraphUtility;
using static VoxelSavedGraphUtility;
/// <summary>
/// Actual editor window that handles the editor stuff
/// </summary>
public class VoxelGraphEditorWindow : EditorWindow
{
    //Main variables
    private VoxelGraphView currentGraphView;
    private VoxelGraphType currentVoxelGraphType;
    private VoxelGraphSerializer serializer;
    /// <summary>
    /// Actually creates the graph window
    /// </summary>
    [MenuItem("Voxel World/ Voxel Graph")]
    public static void OpenGraphWindow() 
    {
        var window = GetWindow<VoxelGraphEditorWindow>();
        //window.TryClear();
        window.titleContent = new GUIContent("Generation Graph");        
    }

    /// <summary>
    /// When the window is active
    /// </summary>
    public void OnEnable()
    {
        serializer = new VoxelGraphSerializer()
        {
            globalGraph = new SavedGlobalVoxelGraph()
        };

        serializer.globalGraph.SetDefaultVars();
        GenerateToolbar();
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void SwitchGraphView(VoxelGraphType voxelGraphType, bool creatingNewOne = false, bool loadingGraph = false) 
    {
        Vector3 viewPosition = Vector3.zero;
        //Save the old graph view
        if (!loadingGraph) serializer.SaveLocalGraphAskUser(currentGraphView, currentVoxelGraphType, "Are you sure you want to switch without saving?");

        if (currentGraphView != null) rootVisualElement.Remove(currentGraphView);
        currentVoxelGraphType = voxelGraphType;

        if (currentGraphView != null) viewPosition = currentGraphView.viewTransform.position;
        if (creatingNewOne) viewPosition = Vector3.zero;

        var graphView = new VoxelGraphView(voxelGraphType, viewPosition) { name = name, };

        switch (voxelGraphType)
        {
            case VoxelGraphType.Density:
                graphView.LoadLocalVoxelGraph(serializer.globalGraph.densityGraph);
                break;
            case VoxelGraphType.CSM:
                graphView.LoadLocalVoxelGraph(serializer.globalGraph.csmGraph);
                break;
            case VoxelGraphType.VoxelDetails:
                graphView.LoadLocalVoxelGraph(serializer.globalGraph.voxelDetailsGraph);
                break;
            default:
                break;
        }

        currentGraphView = graphView;
        graphView.StretchToParentSize();
        rootVisualElement.Insert(0, graphView);
    }

    /// <summary>
    /// Generate the window
    /// </summary>
    private void GenerateToolbar()
    {
        //Generate the toolbars
        Toolbar toolbar = new Toolbar();
        var saveButton = new Button(() => serializer.SaveLocalGraph(currentGraphView, currentVoxelGraphType)) { text = "Save Graph" };
        var generateShaderButton = new Button(() => 
        {
            //string path = EditorUtility.SaveFilePanel("Generate compute shader", "Assets/", "DefaultComputeShader.compute", "compute");
            serializer.SaveLocalGraph(currentGraphView, currentVoxelGraphType);
            CodeConverter.ConvertAndSave(serializer, "Assets/DefaultComputeShader.compute");
        }) { text = "Generate Shader" };

        var switchToDensityGraph = new Button(() => SwitchGraphView(VoxelGraphType.Density)) { text = "Switch to Density Graph" };
        var switchToNormalGraph = new Button(() => SwitchGraphView(VoxelGraphType.CSM)) { text = "Switch to Color/Smoothness and Metallic Graph" };
        var switchToVoxelDetailsGraph = new Button(() => SwitchGraphView(VoxelGraphType.VoxelDetails)) { text = "Switch to VoxelDetails Graph" };

        var loadButton = new Button(() =>
        {
            serializer.SaveLocalGraphAskUser(currentGraphView, currentVoxelGraphType, "Are you sure you want to load a new VoxelGraph without saving?");
            string savePath = EditorUtility.OpenFilePanel("Load VoxelGraph", "Assets/", "voxelgraph");
            if (savePath != "")
            {
                serializer.LoadGlobalGraph(savePath);
                SwitchGraphView(VoxelGraphType.Density, loadingGraph: true);
            }
        }) { text = "Load VoxelGraph" };

        var createButton = new Button(() =>
        {
            serializer.LoadDefaultVoxelGraph();
            SwitchGraphView(VoxelGraphType.Density, creatingNewOne: true);
        }) { text = "Create VoxelGraph" };

        //Add the buttons to the toolbar
        toolbar.Add(loadButton);
        toolbar.Add(createButton);
        toolbar.Add(saveButton);
        toolbar.Add(generateShaderButton);

        toolbar.Add(switchToDensityGraph);
        toolbar.Add(switchToNormalGraph);
        toolbar.Add(switchToVoxelDetailsGraph);

        //Generate the graphViewsHolder
        rootVisualElement.Add(toolbar);
    }
}
