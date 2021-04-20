using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// Actual editor window that handles the editor stuff
/// </summary>
public class VoxelGraph : EditorWindow
{
    //Main variables
    private VoxelGraphView graphView;
    private VisualElement graphViewsHolder;

    /// <summary>
    /// Actually creates the graph window
    /// </summary>
    [MenuItem("VoxelWorld/Generation Graph")]
    public static void OpenGraphWindow() 
    {
        var window = GetWindow<VoxelGraph>();
        window.titleContent = new GUIContent("Generation Graph");
        //
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void ConstructGraphView(string name, bool normal) 
    {
        if(graphViewsHolder.childCount > 0) graphViewsHolder.Remove(graphView);
        graphView = new VoxelGraphView(normal, graphView == null ? Vector3.zero : graphView.viewTransform.position)
        {
            name = name,
        };
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
        Button saveButton = new Button(() => {  });
        Button loadButton = new Button(() => {  });
        Button generateShaderButton = new Button(() => {  });

        Button switchToDensityGraph = new Button(() => { ConstructGraphView("Density Graph", false); });
        Button switchToNormalGraph = new Button(() => { ConstructGraphView("Normal Graph", true); });
        //Add the buttons to the toolbar
        saveButton.text = "Save Graph";
        toolbar.Add(saveButton);
        loadButton.text = "Load Graph";
        toolbar.Add(loadButton);
        generateShaderButton.text = "Generate Shader";
        toolbar.Add(generateShaderButton);

        switchToDensityGraph.text = "Switch to Density Graph";
        switchToNormalGraph.text = "Switch to Normal Graph";
        toolbar.Add(switchToDensityGraph);
        toolbar.Add(switchToNormalGraph);

        //Generate the graphViewsHolder
        graphViewsHolder = new VisualElement();
        graphViewsHolder.StretchToParentSize();
        ConstructGraphView("Density Graph", false);
        rootVisualElement.Add(graphViewsHolder);
        rootVisualElement.Add(toolbar);
    }
    /// <summary>
    /// When this window gets created, generate the voxel graphs
    /// </summary>
    private void OnEnable()
    {
        GenerateWindow();        
    }

    /// <summary>
    /// When this window is destroyed, remove the splitViewer (Contains the two voxel graphs)
    /// </summary>
    private void OnDisable()
    {
        rootVisualElement.Remove(graphViewsHolder);
    }
}
