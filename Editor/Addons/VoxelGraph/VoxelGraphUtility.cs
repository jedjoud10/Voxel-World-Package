using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using static VoxelUtility;
using static VoxelGraphUtility;
using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// Utility class for everything related to the generation graph
/// </summary>
public static class VoxelGraphUtility
{
    #region Base
    /// <summary>
    /// Main class for every type of node in the graph
    /// </summary>
    public abstract class VoxelNode
    {
        //Main abstract stuff
        abstract public string name { get; }
        abstract public string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid);
        public string nodeguid;
        public int type;
        protected List<VisualElement> inputVisualElements = new List<VisualElement>(), outputVisualElements = new List<VisualElement>();
        public Dictionary<string, Port> ports = new Dictionary<string, Port>();
        public List<string> savedPorts;
        public bool saveLoaded;
        public int portCount;
        protected Node node;

        /// <summary>
        /// Setup this node with a guid and a saved state
        /// </summary>
        public virtual void Setup(string nodeguid, List<string> savedPorts) 
        {
            this.nodeguid = nodeguid;
            saveLoaded = savedPorts != null;
            this.savedPorts = saveLoaded ? savedPorts : new List<string>();
        }

        /// <summary>
        /// Generate a port for a specific node
        /// </summary>
        public virtual Port CreatePort(Direction portDirection, Type type, string name, Port.Capacity capacity = Port.Capacity.Single)
        {            
            Port port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
            GraphViewPortData graphViewPortData;

            //Load from saved ports if we are allowed to
            if (saveLoaded) graphViewPortData = new GraphViewPortData { portguid = savedPorts[portCount] };
            else graphViewPortData = new GraphViewPortData { portguid = Guid.NewGuid().ToString() };       
            
            //Set variables
            graphViewPortData.localPortIndex = portCount;
            port.userData = graphViewPortData;
            port.portName = name;

            switch (portDirection)
            {
                case Direction.Input:
                    inputVisualElements.Add(port);
                    break;
                case Direction.Output:
                    outputVisualElements.Add(port);
                    break;
                default:
                    break;
            }

            //Save the ports
            ports.Add(graphViewPortData.portguid, port);
            if (!saveLoaded) savedPorts.Add(graphViewPortData.portguid);

            portCount++;
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
    /// An interface used
    /// </summary>
    public interface IBoundCheckOptimizator
    {
        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public VoxelAABBBound GetAABB();
    }

    /// <summary>
    /// Gets all of the VoxelGenerationObject classes
    /// </summary>
    /// <returns></returns>
    public static List<VoxelNode> GetAllVoxelNodeTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(VoxelNode)) && !type.IsAbstract)
            .Select(type => Activator.CreateInstance(type) as VoxelNode)
            .ToList();
    }

    /// <summary>
    /// Position node
    /// </summary>
    public class VNPosition : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Input/Voxel Position";
        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(Vector3), "Position", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return "p";
        }
    }

    /// <summary>
    /// Normal node
    /// </summary>
    public class VNNormal : VoxelNode
    {

        //Main voxel node variables
        public override string name => "Input/Voxel Normal";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(Vector3), "Normal", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return "n";
        }
    }

    /// <summary>
    /// Node that tells us the zero-crossing point
    /// </summary>
    public class VNSurfacePosition : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Input/Surface Intersect Position";
        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(Vector3), "Surface Intersect Position", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return "sp";
        }
    }

    /// <summary>
    /// Node that tells us the zero-crossing point
    /// </summary>
    public class VNSurfaceNormal : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Input/Surface Intersect Normal";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(Vector3), "Surface Intersect Normal", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return "sn";
        }
    }

    /// <summary>
    /// Normal node
    /// </summary>
    public class VNDensity : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Input/Voxel Density";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(float), "Density", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Result node
    /// </summary>
    public class VNResult : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Voxel Result";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(float), "Density", Port.Capacity.Single);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// CSM Result node
    /// </summary>
    public class VNCSMResult : VoxelNode
    {
        //Main voxel node variables
        public override string name => "CSM Result";

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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Voxel Details Result node
    /// </summary>
    public class VNVoxelDetailsResult : VoxelNode
    {
        //Main voxel node variables
        public override string name => "VoxelDetails Result";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(bool), "Generate", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(int), "Type", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Position", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Rotation", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Scale", Port.Capacity.Single);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Type of VoxelGraphEditorWindow
    /// </summary>
    public enum VoxelGraphType { Density, CSM, VoxelDetails }
    #endregion

    #region Constant Node Type

    /// <summary>
    /// Constant voxel nodes
    /// </summary>
    public abstract class VNConstants : VoxelNode 
    {
        public object objValue;
    }

    /// <summary>
    /// Constant float class
    /// </summary>
    public class VNConstantFloat : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Float";
        private FloatField floatField;

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);

            floatField = new FloatField();
            objValue = objValue == null ? 0f : objValue;
            floatField.value = (float)objValue;
            floatField.RegisterValueChangedCallback(x => { objValue = x.newValue; });
            inputVisualElements.Add(floatField);

            CreatePort(Direction.Output, typeof(float), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float {nodeguid} = {(float)objValue};";
        }
    }

    /// <summary>
    /// Constant Vector2 class
    /// </summary>
    public class VNConstantVec2 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Vector2";
        private Vector2 val;

        /// <summary>
        /// Creates all the ports and float fields for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            objValue = objValue == null ? Vector2.zero : objValue;
            var floatField = new FloatField();
            var floatField1 = new FloatField();
            floatField.value = ((Vector2)objValue).x;
            floatField1.value = ((Vector2)objValue).y;
            Port port = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            port.Add(floatField);
            port1.Add(floatField1);
            inputVisualElements.Add(port);
            inputVisualElements.Add(port1);
            floatField.RegisterValueChangedCallback(x => { val.x = x.newValue; objValue = val; });
            floatField1.RegisterValueChangedCallback(y => { val.y = y.newValue; objValue = val; });
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float2 {nodeguid} = float2({CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector2)objValue).x)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector2)objValue).y)});";
        }
    }

    /// <summary>
    /// Constant Vector3 class
    /// </summary>
    public class VNConstantVec3 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Vector3";
        private Vector3 val;

        /// <summary>
        /// Creates all the ports and float fields for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            objValue = objValue == null ? Vector3.zero : objValue;
            var floatField = new FloatField();
            var floatField1 = new FloatField();
            var floatField2 = new FloatField();
            floatField.value = ((Vector3)objValue).x;
            floatField1.value = ((Vector3)objValue).y;
            floatField2.value = ((Vector3)objValue).z;
            Port port = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            Port port2 = CreatePort(Direction.Input, typeof(float), "Input Z", Port.Capacity.Single);
            port.Add(floatField);
            port1.Add(floatField1);
            port2.Add(floatField2);
            inputVisualElements.Add(port);
            inputVisualElements.Add(port1);
            inputVisualElements.Add(port2);
            floatField.RegisterValueChangedCallback(x => { val.x = x.newValue; objValue = val; });
            floatField1.RegisterValueChangedCallback(y => { val.y = y.newValue; objValue = val; });
            floatField2.RegisterValueChangedCallback(z => { val.z = z.newValue; objValue = val; });
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float3 {nodeguid} = float3(" +
                $"{CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector3)objValue).x)}, " +
                $"{CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector3)objValue).y)}, " +
                $"{CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(2).Key, ((Vector3)objValue).z)});";
        }
    }

    /// <summary>
    /// Constant Vector4 class
    /// </summary>
    public class VNConstantVec4 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Vector4";
        private Vector4 val;
        /// <summary>
        /// Creates all the ports and float fields for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            objValue = objValue == null ? Vector4.zero : objValue;
            var floatField = new FloatField();
            var floatField1 = new FloatField();
            var floatField2 = new FloatField();
            var floatField3 = new FloatField();
            floatField.value = ((Vector4)objValue).x;
            floatField1.value = ((Vector4)objValue).y;
            floatField2.value = ((Vector4)objValue).z;
            floatField3.value = ((Vector4)objValue).w;
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
            floatField.RegisterValueChangedCallback(x => { val.x = x.newValue; objValue = val; });
            floatField1.RegisterValueChangedCallback(y => { val.y = y.newValue; objValue = val; });
            floatField2.RegisterValueChangedCallback(z => { val.z = z.newValue; objValue = val; });
            floatField3.RegisterValueChangedCallback(w => { val.w = w.newValue; objValue = val; });
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float4 {nodeguid} = float4({CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector4)objValue).x)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector4)objValue).y)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(2).Key, ((Vector4)objValue).z)});";
        }
    }

    /// <summary>
    /// Constant Color class
    /// </summary>
    public class VNConstantColor : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Color";
        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            objValue = objValue == null ? Vector3.one : objValue;
            var colorField = new ColorField();
            colorField.value = new Color(((Vector3)objValue).x, ((Vector3)objValue).y, ((Vector3)objValue).z);
            colorField.RegisterValueChangedCallback(x => { objValue = new Vector3(x.newValue.r, x.newValue.g, x.newValue.b); });
            inputVisualElements.Add(colorField);

            CreatePort(Direction.Output, typeof(Vector3), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float3 {nodeguid} = float3({CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector3)objValue).x)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector3)objValue).y)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(2).Key, ((Vector3)objValue).z)});";
        }
    }

    /// <summary>
    /// Constant int class
    /// </summary>
    public class VNConstantInt : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Int";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            objValue = objValue == null ? 0 : objValue;
            var intField = new IntegerField();
            intField.value = (int)objValue;
            intField.RegisterValueChangedCallback(x => { objValue = x.newValue; });
            inputVisualElements.Add(intField);

            CreatePort(Direction.Output, typeof(int), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"int {nodeguid} = {(int)objValue};";
        }
    }

    /// <summary>
    /// Constant bool class
    /// </summary>
    public class VNConstantBool : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Bool";

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<VisualElement>, List<VisualElement>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            objValue = objValue == null ? false : objValue;
            var toggle = new Toggle();
            toggle.value = (bool)objValue;
            toggle.RegisterValueChangedCallback(x => { objValue = x.newValue; });
            inputVisualElements.Add(toggle);

            CreatePort(Direction.Output, typeof(bool), "Result", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"bool {nodeguid} = {(bool)objValue};";
        }
    }

    #endregion

    #region Splitter Node Type

    /// <summary>
    /// Splitter voxel nodes
    /// </summary>
    public abstract class VNSplitters : VoxelNode { }

    /// <summary>
    /// Splitter Vector2 class
    /// </summary>
    public class VNSplitterVec2 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Splitters/Vector2";

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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            switch (((GraphViewPortData)ports[portguid].userData).localPortIndex)
            {
                case 1:
                    return $"float {nodeguid}_x = {((Vector2)objValue).x};";
                case 2:
                    return $"float {nodeguid}_y = {((Vector2)objValue).y};";
                default:
                    return "";
            }            
        }
    }

    /// <summary>
    /// Splitter Vector3 class
    /// </summary>
    public class VNSplitterVec3 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Splitters/Vector3";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            switch (((GraphViewPortData)ports[portguid].userData).localPortIndex)
            {
                case 1:
                    return $"float {nodeguid}_x = {((Vector3)objValue).x};";
                case 2:
                    return $"float {nodeguid}_y = {((Vector3)objValue).y};";
                case 3:
                    return $"float {nodeguid}_z = {((Vector3)objValue).z};";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Splitter Vector4 class
    /// </summary>
    public class VNSplittersVec4 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Splitters/Vector4";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            switch (((GraphViewPortData)ports[portguid].userData).localPortIndex)
            {
                case 1:
                    return $"float {nodeguid}_x = {((Vector4)objValue).x};";
                case 2:
                    return $"float {nodeguid}_y = {((Vector4)objValue).y};";
                case 3:
                    return $"float {nodeguid}_z = {((Vector4)objValue).z};";
                case 4:
                    return $"float {nodeguid}_w = {((Vector4)objValue).w};";
                default:
                    return "";
            }
        }
    }

    #endregion

    #region Shape Node Type
    /// <summary>
    /// Shapes
    /// </summary>
    public abstract class VNShape : VoxelNode
    {
    }

    /// <summary>
    /// Sphere adas
    /// </summary>
    public class VNSphere : VNShape, IBoundCheckOptimizator
    {
        //Main voxel node variables
        public override string name => "SDF Shapes/Sphere";
        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public VoxelAABBBound GetAABB()
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
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Cube
    /// </summary>
    public class VNCube : VNShape
    {
        //Main voxel node variables
        public override string name => "SDF Shapes/Cube";
        /// <summary>
        /// Get the AABB bound for this object
        /// </summary>
        public VoxelAABBBound GetAABB()
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

            CreatePort(Direction.Output, typeof(float), "Density", Port.Capacity.Multi);
            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region CSG Operation Node Type
    /// <summary>
    /// Constructive-Solid-Geometry operations
    /// </summary>
    public abstract class VNCSGOperation : VoxelNode
    {
    }

    /// <summary>
    /// Mathematical operations
    /// </summary>
    public class VNCSGUnion : VoxelNode
    {
        //Main voxel node variables
        public override string name => "CSG/Union";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Arithmetic Operation Node Type

    /// <summary>
    /// Arithmetic operations
    /// </summary>
    //Addition
    public class VNMathAddition : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Math/Arithmetic/Addition";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }
    //Subtraction
    public class VNMathSubtraction : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Math/Arithmetic/Subtraction";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }
    //Multiplication
    public class VNMathMultiplication : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Math/Arithmetic/Multiplication";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }
    //Division
    public class VNMathDivision : VoxelNode
    {
        //Main voxel node variables
        public override string name => "Math/Arithmetic/Division";
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

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}