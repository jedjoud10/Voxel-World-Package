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
    private GenerationGraphView graphView, graphView1;
    private VisualElement visualElement;//Thank you Unity for having this! :)

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
        visualElement = new VisualElement();
        float boxSize = 0.5f;
        //Bottom graph
        graphView = new GenerationGraphView(true)
        {
            name = "Normal Generation Graph",
        };
        graphView.StretchToParentWidth();
        graphView.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
        graphView.style.top = new StyleLength(new Length(50 + boxSize/2f, LengthUnit.Percent));

        //Top graph
        graphView1 = new GenerationGraphView(false)
        {
            name = "Density Generation Graph",            
        };
        graphView1.StretchToParentWidth();
        //graphView1.style.height = new StyleLength(new Length(50, LengthUnit.Percent));
        graphView1.style.bottom = new StyleLength(new Length(50 + boxSize / 2f, LengthUnit.Percent));
        graphView1.style.top = new StyleLength(new Length(0, LengthUnit.Percent));

        //Separator box
        VisualElement box = new VisualElement();
        box.style.backgroundColor = new StyleColor(Color.black);
        box.StretchToParentWidth();
        box.style.bottom = new StyleLength(new Length(50 - boxSize/2f, LengthUnit.Percent));
        box.style.top = new StyleLength(new Length(50 - boxSize/2f, LengthUnit.Percent));

        visualElement.Add(graphView);
        visualElement.Add(box);
        visualElement.Add(graphView1);
        visualElement.StretchToParentSize();
        rootVisualElement.Add(visualElement);
    }

    /// <summary>
    /// Generates a toolbar with custom actions like creating nodes
    /// </summary>
    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();
        Button saveButton = new Button(() => {  });
        Button loadButton = new Button(() => {  });
        Button generateShaderButton = new Button(() => {  });
        //Add the buttons to the toolbar
        saveButton.text = "Save Graph";
        toolbar.Add(saveButton);
        loadButton.text = "Load Graph";
        toolbar.Add(loadButton);
        generateShaderButton.text = "Generate Shader";
        toolbar.Add(generateShaderButton);
        rootVisualElement.Insert(0, toolbar);
        toolbar.BringToFront();
    }
    /// <summary>
    /// When this window gets created, generate the voxel graphs
    /// </summary>
    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    /// <summary>
    /// When this window is destroyed, remove the splitViewer (Contains the two voxel graphs)
    /// </summary>
    private void OnDisable()
    {
        rootVisualElement.Remove(visualElement);
    }
}
