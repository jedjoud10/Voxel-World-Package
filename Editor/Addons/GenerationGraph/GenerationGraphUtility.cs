using UnityEngine;
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
    #region Base
    /// <summary>
    /// Main class for "something" (Like noise, a sphere, a cube or anything represented by a density field really)
    /// </summary>
    public abstract class VoxelNodeType
    {
        //Main abstract stuff
        abstract public string name { get; }
        public abstract VoxelAABBBound GetAABB();
        protected List<VisualElement> inputVisualElements = new List<VisualElement>(), outputVisualElements = new List<VisualElement>();
        public List<Port> inputPorts = new List<Port>(), outputPorts = new List<Port>();
        protected Node node;

        /// <summary>
        /// Generate a port for a specific node
        /// </summary>
        public virtual Port CreatePort(Direction portDirection, Type type, string name, Port.Capacity capacity = Port.Capacity.Single)
        {            
            Port port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
            port.portName = name;
            switch (portDirection)
            {
                case Direction.Input:
                    inputVisualElements.Add(port);
                    inputPorts.Add(port);
                    break;
                case Direction.Output:
                    outputVisualElements.Add(port);
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
            return ("None", inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Gets all of the VoxelGenerationObject classes
    /// </summary>
    /// <returns></returns>
    public static List<VoxelNodeType> GetAllVoxelNodeTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(VoxelNodeType)) && !type.IsAbstract)
            .Select(type => Activator.CreateInstance(type) as VoxelNodeType)
            .ToList();
    }

    /// <summary>
    /// Position node
    /// </summary>
    public class VNPosition : VoxelNodeType
    {
        //Main voxel node variables
        public override string name => "Input/Voxel Position";

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
            CreatePort(Direction.Output, typeof(Vector3), "Position", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Normal node
    /// </summary>
    public class VNNormal : VoxelNodeType
    {
        //Main voxel node variables
        public override string name => "Input/Voxel Normal";

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
            CreatePort(Direction.Output, typeof(Vector3), "Normal", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Normal node
    /// </summary>
    public class VNDensity : VoxelNodeType
    {
        //Main voxel node variables
        public override string name => "Input/Voxel Density";

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
            CreatePort(Direction.Output, typeof(float), "Density", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
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
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Normal Result node
    /// </summary>
    public class VNNormalResult : VoxelNodeType
    {
        public override string name => "Normal Voxel Result";

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
            CreatePort(Direction.Input, typeof(Vector2), "Smoothness and Metallic", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Color", Port.Capacity.Single);
            return (name, inputVisualElements, outputVisualElements);
        }
    }


    #endregion

    #region Constant Node Type

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
            inputVisualElements.Add(floatField);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(float), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
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
            var floatField = new FloatField();
            var floatField1 = new FloatField();
            Port port = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            port.Add(floatField);
            port1.Add(floatField1);
            inputVisualElements.Add(port);
            inputVisualElements.Add(port1);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector2), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
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
            var floatField = new FloatField();
            var floatField1 = new FloatField();
            var floatField2 = new FloatField();
            Port port = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            Port port2 = CreatePort(Direction.Input, typeof(float), "Input Z", Port.Capacity.Single);
            port.Add(floatField);
            port1.Add(floatField1);
            port2.Add(floatField2);
            inputVisualElements.Add(port);
            inputVisualElements.Add(port1);
            inputVisualElements.Add(port2);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector3), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
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
            var floatField = new FloatField();
            var floatField1 = new FloatField();
            var floatField2 = new FloatField();
            var floatField3 = new FloatField();
            Port port = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            Port port2 = CreatePort(Direction.Input, typeof(float), "Input Z", Port.Capacity.Single);
            Port port3 = CreatePort(Direction.Input, typeof(float), "Input W", Port.Capacity.Single);
            port.Add(floatField);
            port1.Add(floatField1);
            port2.Add(floatField2);
            port3.Add(floatField3);
            inputVisualElements.Add(port);
            inputVisualElements.Add(port1);
            inputVisualElements.Add(port2);
            inputVisualElements.Add(port3);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector4), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }
    /// <summary>
    /// Constant Color class
    /// </summary>
    public class VNConstantColor : VNConstants
    {
        public override string name => "Constants/Color";

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
            var csharpField = new ColorField();
            csharpField.value = Color.white;
            inputVisualElements.Add(csharpField);
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreateInputConstantPort();
            CreatePort(Direction.Output, typeof(Vector3), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    #endregion

    #region Splitter Node Type

    /// <summary>
    /// Splitter voxel nodes
    /// </summary>
    public abstract class VNSplitters : VoxelNodeType { }

    /// <summary>
    /// Splitter Vector2 class
    /// </summary>
    public class VNSplitterVec2 : VNConstants
    {
        public override string name => "Splitters/Vector2";

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
            CreatePort(Direction.Input, typeof(Vector2), "Input Vector2", Port.Capacity.Single);
            CreatePort(Direction.Output, typeof(float), "Result X", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(float), "Result Y", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Splitter Vector3 class
    /// </summary>
    public class VNSplitterVec3 : VNConstants
    {
        public override string name => "Splitters/Vector3";

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
            CreatePort(Direction.Input, typeof(Vector3), "Input Vector3", Port.Capacity.Single);
            CreatePort(Direction.Output, typeof(float), "Result X", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(float), "Result Y", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(float), "Result Z", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Splitter Vector4 class
    /// </summary>
    public class VNSplittersVec4 : VNConstants
    {
        public override string name => "Splitters/Vector4";

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
            CreatePort(Direction.Input, typeof(Vector4), "Input Vector4", Port.Capacity.Single);
            CreatePort(Direction.Output, typeof(float), "Result X", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(float), "Result Y", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(float), "Result Z", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(float), "Result W", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    #endregion

    #region Shape Node Type
    /// <summary>
    /// Shapes
    /// </summary>
    public abstract class VNShape : VoxelNodeType
    {
        //Shape variables
        public Vector3 offset;
        public Color color;
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
            CreatePort(Direction.Input, typeof(Vector3), "Color", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "Radius", Port.Capacity.Single);

            CreatePort(Direction.Output, typeof(float), "Density", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(Vector3), "Color", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    /// <summary>
    /// Cube
    /// </summary>
    public class VNCube : VNShape
    {
        public override string name => "SDF Shapes/Cube";

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
            CreatePort(Direction.Input, typeof(Vector3), "Bounds", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Color), "Color", Port.Capacity.Single);

            CreatePort(Direction.Output, typeof(float), "Density", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    #endregion

    #region CSG Operation Node Type
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
            CreatePort(Direction.Input, typeof(float), "A Density", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "A Color", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "B Density", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "B Color", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(float), "Smoothness", Port.Capacity.Single);

            CreatePort(Direction.Output, typeof(float), "Result Density", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(Vector3), "Result Color", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    #endregion

    #region Mathematical Operation Node Type

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
            return (name, inputVisualElements, outputVisualElements);
        }
    }

    #endregion
}