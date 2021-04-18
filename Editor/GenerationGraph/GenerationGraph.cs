using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// Actual editor window that handles the editor stuff
/// </summary>
public class GenerationGraph : EditorWindow
{
    //Main variables
    private GenerationGraphView graphView;

    /// <summary>
    /// Actually creates the graph window
    /// </summary>
    [MenuItem("VoxelWorld/Generation Graph")]
    public static void OpenGraphWindow() 
    {
        var window = GetWindow<GenerationGraph>();
        window.titleContent = new GUIContent("Generation Graph");
    }

    /// <summary>
    /// Generates the graphview
    /// </summary>
    private void ConstructGraphView() 
    {
        graphView = new GenerationGraphView
        {
            name = "Generation Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    /// <summary>
    /// Generates a toolbar with custom actions like creating nodes
    /// </summary>
    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();
        graphView.Add(toolbar);
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }
    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
}
