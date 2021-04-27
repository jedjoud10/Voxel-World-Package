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
    private VisualElement graphViewsHolder;
    private VisualElement menuElement;
    private VoxelGraphType currentVoxelGraphType;
    private VoxelGraphSerializer serializer;
    private Toolbar toolbar;
    /// <summary>
    /// Actually creates the graph windowasdgsdfgh
    /// </summary>
    [MenuItem("Voxel World/ Voxel Graph")]
    public static void OpenGraphWindow() 
    {
        var window = GetWindow<VoxelGraphEditorWindow>();
        window.titleContent = new GUIContent("Generation Graph");
        /*
        window.serializer = new VoxelGraphSerializer() 
        {
            globalGraph = new SavedGlobalVoxelGraph()
        };
        */
        //window.serializer.globalGraph.SetDefaultVars();        
        window.GenerateMenu();
    }

    /// <summary>
    /// Generate the main menu for the VoxelGraph
    /// </summary>
    private void GenerateMenu()
    {
        if (menuElement == null)
        {
            menuElement = new VisualElement();
            menuElement.Add(new Button(() =>
            {
                string path = EditorUtility.OpenFilePanel("Load VoxelGraph", "Assets/", "voxelgraph");
                if (path != "")
                {
                    serializer.LoadGlobalGraph(path);
                    GenerateGraphView();
                    SwitchGraphView(VoxelGraphType.Density);
                }
            })
            { text = "Load VoxelGraph" });
            menuElement.Add(new Button(() =>
            {
                serializer.LoadDefaultVoxelGraph();
                GenerateGraphView();
                SwitchGraphView(VoxelGraphType.Density);
            })
            { text = "Create VoxelGraph" });
            menuElement.StretchToParentSize();
            rootVisualElement.Add(menuElement);
        }
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void SwitchGraphView(VoxelGraphType voxelGraphType) 
    {
        //Save the old graph view
        serializer.SaveLocalGraphAskUser(currentGraphView, currentVoxelGraphType, "Are you sure you want to save this VoxelGraph?");

        if (graphViewsHolder.childCount > 0) graphViewsHolder.Remove(currentGraphView);
        currentVoxelGraphType = voxelGraphType;

        var graphView = new VoxelGraphView(voxelGraphType, currentGraphView == null ? Vector3.zero : currentGraphView.viewTransform.position) { name = name, };

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
        graphViewsHolder.Add(graphView);
    }

    /// <summary>
    /// Generate the window
    /// </summary>
    private void GenerateGraphView()
    {
        rootVisualElement.Remove(menuElement);
        //Generate the toolbar
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

    private void OnEnable()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    /// <summary>
    /// Editor window reload fix. TODO: Actually do this
    /// </summary>
    private void OnAfterAssemblyReload()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// When this window is destroyed, remove the splitViewer (Contains the two voxel graphs)
    /// </summary>
    private void OnDisable()
    {
        /*
        serializer.SaveLocalGraphAskUser(currentGraphView, currentVoxelGraphType, @"Are you sure you want to save this VoxelGraph?
        PS: The window will still close!");
        */
        if (graphViewsHolder != null) rootVisualElement.Remove(graphViewsHolder);
        if (toolbar != null) rootVisualElement.Remove(toolbar);
    }
}
