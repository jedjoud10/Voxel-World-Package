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
public class VoxelGraph : EditorWindow
{
    //Main variables
    private VoxelGraphView graphView;
    private VisualElement graphViewsHolder;
    private VoxelGraphSO voxelGraphSOData;

    /// <summary>
    /// Actually creates the graph window
    /// </summary>
    public static void OpenGraphWindow(VoxelGraphSO voxelGraphSOData) 
    {
        var window = GetWindow<VoxelGraph>();
        window.voxelGraphSOData = voxelGraphSOData;
        window.titleContent = new GUIContent("Generation Graph");
        window.GenerateWindow();
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void ConstructGraphView(string name, VoxelGraphType voxelGraphType) 
    {
        if(graphViewsHolder.childCount > 0) graphViewsHolder.Remove(graphView);
        graphView = new VoxelGraphView(voxelGraphType, graphView == null ? Vector3.zero : graphView.viewTransform.position)
        {
            name = name,
        };
        graphView.LoadVoxelGraph(voxelGraphSOData.densityGraph);
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
        Button saveButton = new Button(() => { voxelGraphSOData.SaveVoxelGraph(graphView, graphView.voxelGraphType); }) { text = "Save Graph" };
        Button generateShaderButton = new Button(() => { }) { text = "Generate Shader" };

        Button switchToDensityGraph = new Button(() => { ConstructGraphView("Density Graph", VoxelGraphType.Density); }) { text = "Switch to Density Graph" };
        Button switchToNormalGraph = new Button(() => { ConstructGraphView("Normal Graph", VoxelGraphType.Normal); }) { text = "Switch to Normal Graph" };
        Button switchToVoxelDetailsGraph = new Button(() => { ConstructGraphView("Normal Graph", VoxelGraphType.VoxelDetails); }) { text = "Switch to VoxelDetails Graph" };
        //Add the buttons to the toolbar
        toolbar.Add(saveButton);
        toolbar.Add(generateShaderButton);

        toolbar.Add(switchToDensityGraph);
        toolbar.Add(switchToNormalGraph);
        toolbar.Add(switchToVoxelDetailsGraph);

        //Generate the graphViewsHolder
        graphViewsHolder = new VisualElement();
        graphViewsHolder.StretchToParentSize();
        ConstructGraphView("Density Graph", VoxelGraphType.Density);
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
