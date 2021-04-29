using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A converter used to turn C# code into the proper terrain compute shader
/// </summary>
public abstract partial class VoxelEditorWindow : EditorWindow
{
    private Dictionary<string, Convar> variables;
    protected List<string> lines;
    public virtual void Convert() 
    {
        variables = new Dictionary<string, Convar>();        
        lines = new List<string>();
    }

    public abstract void GetDensityCode(StringBuilder globalBuilder);

    private Type GuessType(ref string defaultValue) 
    {
        Type type = null;
        switch (defaultValue.Split(',').Length)
        {
            case 1:
                type = typeof(float);
                break;
            case 2:
                type = typeof(Vector2);
                defaultValue = $"float2{defaultValue}";
                break;
            case 3:
                type = typeof(Vector3);
                defaultValue = $"float3{defaultValue}";
                break;
            case 4:
                type = typeof(Vector4);
                defaultValue = $"float4{defaultValue}";
                break;
            default:
                break;
        }
        return type;
    }

    /// <summary>
    /// Create a new variable and add it to the variable list
    /// </summary>
    protected Convar init(string name, string defaultValue) 
    {
        Type type = GuessType(ref defaultValue);
        Convar variable = new Convar() { name = name, codeRepresentation = $"{GetTypeOfVariable(type)} {name} = {defaultValue};", type = type };
        InitVariable(variable);
        return variable;
    }
    protected Convar InitVariable(Convar variable)
    {
        if (variable.name.Contains(".")) throw new System.Exception("NONONON");
        variables.Add(variable.name, variable);
        lines.Add(variable.codeRepresentation);
        if (variable.type == typeof(Vector2))
        {
            init($"{variable.name}_x", $"{variable.name}.x");
            init($"{variable.name}_y", $"{variable.name}.y");
        }
        else if (variable.type == typeof(Vector3))
        {
            init($"{variable.name}_x", $"{variable.name}.x");
            init($"{variable.name}_y", $"{variable.name}.y");
            init($"{variable.name}_z", $"{variable.name}.z");
        }
        else if (variable.type == typeof(Vector4))
        {
            init($"{variable.name}_x", $"{variable.name}.x");
            init($"{variable.name}_y", $"{variable.name}.y");
            init($"{variable.name}_z", $"{variable.name}.z");
            init($"{variable.name}_w", $"{variable.name}.w");
        }

        return variable;
    }

    /// <summary>
    /// Turns a type into a string
    /// </summary>
    private string GetTypeOfVariable(System.Type type) 
    {
        //Can't use switch since it doesn't accept float as a pattern type
        if (type == typeof(float))
        {
            return "float";
        }
        else if (type == typeof(Vector2))
        {
            return "float2";
        }
        else if (type == typeof(Vector3))
        {
            return "float3";
        }
        else if (type == typeof(Vector4))
        {
            return "float4";
        }
        return "";
    }

    /// <summary>
    /// Debug the current lines that are stored in the converter
    /// </summary>
    protected void DebugLines() { foreach (var line in lines) Debug.Log(line); }

    /// <summary>
    /// Retrieves a single variable by it's name
    /// </summary>
    protected Convar get(string name) { return variables[name]; }

    /// <summary>
    /// Sets a single variable by it's name
    /// </summary>
    protected void set(string name, string defaultValue) 
    {
        if (GuessType(ref defaultValue) == variables[name].type)
        {
            Convar convar = variables[name];
            convar.codeRepresentation = $"{name} = {defaultValue};";
            variables[name] = convar;
            Debug.LogWarning("ADFG");
            lines.Add(convar.codeRepresentation);
        }        
    }

    /// <summary>
    /// Do a math operation on two variables
    /// </summary>
    protected Convar math(Convar a, Convar b, mathopr operation) 
    {
        if (a.type == b.type)
        {
            Convar c = new Convar() { name = $"{ a.name }_{ b.name }_{ operation.ToString() }", type = a.type };
            c.codeRepresentation = $"{GetTypeOfVariable(a.type)} {c.name} = {a.name} + {b.name};";
            return InitVariable(c);
        }
        return new Convar();
    }


    /// <summary>
    /// A math operation we could use on two variables
    /// </summary>
    protected enum mathopr 
    {
        add, Substraction, Multiplication, Division
    }

    /// <summary>
    /// A struct representing a variable in the converter class
    /// </summary>
    protected struct Convar 
    {
        public string name;
        public System.Type type;
        public string codeRepresentation;
    }
}
