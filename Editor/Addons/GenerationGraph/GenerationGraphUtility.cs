using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using static VoxelUtility;
using static GenerationGraphUtility;
using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// Utility class for everything related to the generation graph
/// </summary>
public static class GenerationGraphUtility
{
    /// <summary>
    /// Main class for "something" (Like noise, a sphere, a cube or anything represented by a density field really)
    /// </summary>
    public abstract class VoxelNodeType
    {
        //Main abstract stuff
        abstract public string name { get; }
        public abstract VoxelAABBBound GetAABB();
        protected List<VisualElement> inputPorts = new List<VisualElement>(), outputPorts = new List<VisualElement>();
        protected Node node;

        /// <summary>
        /// Generate a port for a specific node
        /// </summary>
        public virtual Port CreatePort(Direction portDirection, Type type, string name, Port.Capacity capacity = Port.Capacity.Single)
        {            
            Port port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
            port.userData = new VoxelPortData();
            port.portName = name;
            switch (portDirection)
            {
                case Direction.Input:
                    inputPorts.Add(port);
                    break;
                case Direction.Output:
                    outputPorts.Add(port);
                    break;
                default:
                    break;
            }
            return port;
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public virtual (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            this.node = node;
            return ("None", inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Gets all of the VoxelGenerationObject classes
    /// </summary>
    /// <returns></returns>
    public static List<VoxelNodeType> GetAll()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(VoxelNodeType)) && !type.IsAbstract)
            .Select(type => Activator.CreateInstance(type) as VoxelNodeType)
            .ToList();
    }

    /// <summary>
    /// Input node
    /// </summary>
    public class VNInput : VoxelNodeType
    {
        //Main voxel node variables
        public override string name => "Voxel Input";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a custom port for this VoxelNode Input node
        /// </summary>
        public Port CreateCSMPort(Direction portDirection, Type type, string name, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = base.CreatePort(portDirection, type, name, capacity);
            ((VoxelPortData)port.userData).csmPort = true;
            return port;
        }
        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(Vector3), "Position", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(Vector3), "Local Position", Port.Capacity.Multi);
            CreateCSMPort(Direction.Output, typeof(Vector3), "Normal", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Result node
    /// </summary>
    public class VNResult : VoxelNodeType
    {
        public override string name => "Voxel Result";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary> 
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(float), "Density", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector2), "Smoothness and Metallic", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Color", Port.Capacity.Single);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Constant voxel nodes
    /// </summary>
    public abstract class VNConstants : VoxelNodeType { }

    /// <summary>
    /// Constant float class
    /// </summary>
    public class VNConstantFloat : VNConstants
    {
        public override string name => "Constants/Float";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a new constant port for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            var floatField = new FloatField();
            inputPorts.Add(floatField);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(float), "Result", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Constant Vector2 class
    /// </summary>
    public class VNConstantVec2 : VNConstants
    {
        public override string name => "Constants/Vector2";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a new constant port for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            var vec2Field = new Vector2Field();
            inputPorts.Add(vec2Field);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector2), "Result", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Constant Vector3 class
    /// </summary>
    public class VNConstantVec3 : VNConstants
    {
        public override string name => "Constants/Vector3";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a new constant port for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            var vec3Field = new Vector3Field();
            inputPorts.Add(vec3Field);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector3), "Result", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Constant Vector4 class
    /// </summary>
    public class VNConstantVec4 : VNConstants
    {
        public override string name => "Constants/Vector4";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a new constant port for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            var vec4Field = new Vector4Field();
            inputPorts.Add(vec4Field);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector4), "Result", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }


    /// <summary>
    /// Shapes
    /// </summary>
    public abstract class VNShape : VoxelNodeType
    {
        //Shape variables
        public Vector3 offset;
    }

    /// <summary>
    /// Constructive-Solid-Geometry operations
    /// </summary>
    public abstract class VNCSGOperation : VoxelNodeType
    {
        //CSG Operation variables
        public float smoothness;
    }

    /// <summary>
    /// Mathematical operations
    /// </summary>
    public class VNCSGUnion : VoxelNodeType
    {
        public override string name => "CSG/Union";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(float), "A", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "B", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "Smoothness", Port.Capacity.Single);
            CreatePort(Direction.Output, typeof(float), "Result", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Mathematical operations
    /// </summary>
    public class VNMathAddition : VoxelNodeType
    {
        public override string name => "Math/Addition";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(float), "A", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "B", Port.Capacity.Single);
            CreatePort(Direction.Output, typeof(float), "Result", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }

    /// <summary>
    /// Sphere
    /// </summary>
    public class VNSphere : VNShape
    {
        public override string name => "SDF Shapes/Sphere";

        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public override VoxelAABBBound GetAABB()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(Vector3), "Position", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Offset", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "Radius", Port.Capacity.Single);
            CreatePort(Direction.Output, typeof(float), "Density", Port.Capacity.Multi);
            return (name, inputPorts, outputPorts);
        }
    }
}