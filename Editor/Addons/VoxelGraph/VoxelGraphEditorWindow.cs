using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static VoxelGraphUtility;
using static SavedVoxelGraphUtility;
/// <summary>
/// Actual editor window that handles the editor stuff
/// </summary>
public class VoxelGraphEditorWindow : EditorWindow
{
    //Main variables
    private VoxelGraphView currentGraphView;
    private VoxelGraphType currentVoxelGraphType;
    private VoxelGraphSerializer serializer;
    private string path;
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
        GenerateToolbar();
        if (currentGraphView != null)
        {
            SavedLocalVoxelGraph newSavedVoxelGraph = currentGraphView.SaveLocalVoxelGraph();
            if (IsGraphDifferent(newSavedVoxelGraph, currentGraphView.currentSavedVoxelGraph))
            {
                if (EditorUtility.DisplayDialog("Switch without saving?", "Are you sure you want to switch without saving?", "Save", "Don't Save"))
                {
                    SaveCurrentGraphView();
                }
            }
        }
        if (path != "")
        {
            serializer.LoadGlobalGraph(path);
            SwitchGraphView(VoxelGraphType.Density, loadingGraph: true);
        }
    }

    public void OnDisable()
    {
        SaveCurrentGraphView();
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void SwitchGraphView(VoxelGraphType voxelGraphType, bool creatingNewOne = false, bool loadingGraph = false)
    {
        Vector3 viewPosition = Vector3.zero;
        //Save the old voxel graph
        if (!loadingGraph)
        {
            if (EditorUtility.DisplayDialog("Switch without saving?", "Are you sure you want to switch without saving?", "Save", "Don't Save"))
            {
                serializer.SaveLocalGraph(currentGraphView.SaveLocalVoxelGraph(), currentVoxelGraphType);
                serializer.SaveGlobalGraph(path);
            }            
        }
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

    private void SaveCurrentGraphView() 
    {
        if(string.IsNullOrEmpty(path)) path = EditorUtility.SaveFilePanel("Save VoxelGraph:", "Assets/", "NewVoxelGraph", "voxelgraph");
        serializer.SaveLocalGraph(currentGraphView.SaveLocalVoxelGraph(), currentVoxelGraphType);
        serializer.SaveGlobalGraph(path);
    }

    /// <summary>
    /// Generate the window
    /// </summary>
    private void GenerateToolbar()
    {
        //Generate the toolbars
        Toolbar toolbar = new Toolbar();
        //Save button
        var saveButton = new Button(() => 
        {
            serializer.SaveLocalGraph(currentGraphView.SaveLocalVoxelGraph(), currentVoxelGraphType);
            serializer.SaveGlobalGraph(path);
        }) { text = "Save Graph" };

        //Save as button
        var saveAsButton = new Button(() => 
        {
            path = EditorUtility.SaveFilePanel("Save VoxelGraph:", "Assets/", "NewVoxelGraph", "voxelgraph");
            serializer.SaveLocalGraph(currentGraphView.SaveLocalVoxelGraph(), currentVoxelGraphType);
            serializer.SaveGlobalGraph(path);
        }) { text = "Save Graph As" };

        //Generate shader button
        var generateShaderButton = new Button(() =>
        {
            GenerateShaderButton();
        }) { text = "Generate Shader" };

        var switchToDensityGraph = new Button(() => SwitchGraphView(VoxelGraphType.Density)) { text = "Switch to Density Graph" };
        var switchToNormalGraph = new Button(() => SwitchGraphView(VoxelGraphType.CSM)) { text = "Switch to Color/Smoothness and Metallic Graph" };
        var switchToVoxelDetailsGraph = new Button(() => SwitchGraphView(VoxelGraphType.VoxelDetails)) { text = "Switch to VoxelDetails Graph" };
        //Loading an already existing voxel graph
        var loadButton = new Button(() =>
        {
            LoadButtonHandling();
        }) { text = "Load VoxelGraph" };

        //Creating a new voxel graph by loading in the default one
        var createButton = new Button(() =>
        {
            CreateButtonHandling();

        }) { text = "Create VoxelGraph" };

        //Add the buttons to the toolbar
        toolbar.Add(loadButton);
        toolbar.Add(createButton);
        toolbar.Add(saveButton);
        toolbar.Add(saveAsButton);
        toolbar.Add(generateShaderButton);

        toolbar.Add(switchToDensityGraph);
        toolbar.Add(switchToNormalGraph);
        toolbar.Add(switchToVoxelDetailsGraph);

        //Generate the graphViewsHolder
        rootVisualElement.Add(toolbar);
    }

    private void GenerateShaderButton()
    {
        string path = EditorUtility.SaveFilePanel("Generate compute shader", "Assets/", "DefaultComputeShader.compute", "compute");
        if (!string.IsNullOrEmpty(path))
        {
            serializer.SaveLocalGraph(currentGraphView.SaveLocalVoxelGraph(), currentVoxelGraphType);
            serializer.SaveGlobalGraph(path);
            CodeConverter.ConvertAndSave(serializer, "Assets/DefaultComputeShader.compute");
        }
    }

    private void CreateButtonHandling()
    {
        //Make sure the user saved their graph
        if (currentGraphView != null)
        {
            if (EditorUtility.DisplayDialog("Switch without saving?", "Are you sure you want to switch without saving?", "Save", "Don't Save"))
            {
                SaveCurrentGraphView();
            }
        }
        //serializer.globalGraph.SetDefaultVars();
        serializer.LoadDefaultVoxelGraph();
        SwitchGraphView(VoxelGraphType.Density, loadingGraph: true);
    }

    private void LoadButtonHandling()
    {
        if (currentGraphView != null)
        {
            SavedLocalVoxelGraph newSavedVoxelGraph = currentGraphView.SaveLocalVoxelGraph();
            if (IsGraphDifferent(newSavedVoxelGraph, currentGraphView.currentSavedVoxelGraph))
            {
                if (EditorUtility.DisplayDialog("Switch without saving?", "Are you sure you want to switch without saving?", "Save", "Don't Save"))
                {
                    SaveCurrentGraphView();
                }
            }
        }

        string loadPath = EditorUtility.OpenFilePanel("Load VoxelGraph", "Assets/", "voxelgraph");
        if (loadPath != "")
        {
            serializer.LoadGlobalGraph(loadPath);
            SwitchGraphView(VoxelGraphType.Density, loadingGraph: true);
        }
        path = loadPath;
    }
}
