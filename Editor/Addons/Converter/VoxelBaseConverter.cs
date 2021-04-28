using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A converter used to turn C# code into the proper terrain compute shader
/// </summary>
public partial class VoxelBaseConverter
{
    private Dictionary<string, ConverterVariable> variables;
    private List<string> lines;
    public virtual void Convert() 
    {
        variables = new Dictionary<string, ConverterVariable>();
        lines = new List<string>();
    }

    /// <summary>
    /// Create a new variable and add it to the variable list
    /// </summary>
    protected ConverterVariable InitVariable(string name, System.Type type, string defaultValue = null) 
    {
        ConverterVariable variable = new ConverterVariable() { name = name, codeRepresentation = $"{type.ToString()} {name} = {defaultValue};", type = type };
        variables.Add(name, variable);
        lines.Add(variable.codeRepresentation);
        return variable;
    }
    protected void InitVariable(ConverterVariable variable)
    {
        variables.Add(variable.name, variable);
    }

    /// <summary>
    /// Debug the current lines that are stored in the converter
    /// </summary>
    protected void DebugLines() { foreach (var line in lines) Debug.Log(line); }

    /// <summary>
    /// Retrieves a single variable by it's name
    /// </summary>
    protected ConverterVariable GetVariable(string name) { return variables[name]; }

    /// <summary>
    /// Do a math operation on two variables
    /// </summary>
    protected ConverterVariable Math(ConverterVariable a, ConverterVariable b, MathOperation operation) 
    {
        if (a.type == b.type)
        {
            ConverterVariable c = new ConverterVariable() { name = $"{ a.name }_{ b.name }_{ operation.ToString() }", type = a.type };
            c.codeRepresentation = $"var {c.name} = {a.name} + {b.name};";
            return c;
        }
        return new ConverterVariable();
    }

    /// <summary>
    /// A math operation we could use on two variables
    /// </summary>
    protected enum MathOperation 
    {
        Addition, Substraction, Multiplication, Division
    }

    /// <summary>
    /// A struct representing a variable in the converter class
    /// </summary>
    protected struct ConverterVariable 
    {
        public string name;
        public System.Type type;
        public string codeRepresentation;
    }
}
