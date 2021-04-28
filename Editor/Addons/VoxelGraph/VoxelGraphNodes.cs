using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using static VoxelUtility;
using static VoxelGraphUtility;
using static VoxelSavedGraphUtility;
using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Runtime.Serialization;

/// <summary>
/// Utility class for everything related to the generation graph
/// </summary>
public static partial class VoxelGraphUtility
{
    /// <summary>
    /// Main class for every type of node in the graph
    /// </summary>
    [System.Serializable]
    public abstract class VoxelNode
    {
        //Main abstract stuff
        /// <summary>
        /// Serializable variables
        /// </summary>
        abstract public string name { get; }
        public string nodeGuid;
        public List<GraphViewPortData> savedPorts;
        public bool loadSavedPorts = false;

        /// <summary>
        /// Non serializable variables
        /// </summary>
        [NonSerialized] protected List<VisualElement> inputVisualElements, outputVisualElements;
        [NonSerialized] public Dictionary<string, Port> ports;
        [NonSerialized] protected Node node;
        [NonSerialized] public int portCount = 0;

        /// <summary>
        /// Setup this node with a guid and a saved state
        /// </summary>
        public virtual VoxelNode Setup(string nodeGuid, bool loadSavedPorts)
        {
            this.nodeGuid = nodeGuid;
            this.loadSavedPorts = loadSavedPorts;
            if (this.inputVisualElements == null) this.inputVisualElements = new List<VisualElement>(); this.outputVisualElements = new List<VisualElement>();
            if (this.ports == null) this.ports = new Dictionary<string, Port>();
            if (this.savedPorts == null) this.savedPorts = new List<GraphViewPortData>();
            Debug.Log(savedPorts.Count);
            this.node = null;
            return this;
        }

        /// <summary>
        /// Generate a port for a specific node
        /// </summary>
        public virtual Port CreatePort(Direction portDirection, Type type, string name, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);

            GraphViewPortData graphViewPortData;

            if (loadSavedPorts) graphViewPortData = savedPorts[portCount];
            else 
            {
                graphViewPortData = new GraphViewPortData()
                {
                    localPortIndex = portCount,
                    portGuid = Guid.NewGuid().ToString()
                };
                savedPorts.Add(graphViewPortData);
            }
            //Set variables
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
            ports.Add(graphViewPortData.portGuid, port);

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

        /// <summary>
        /// Get the code representation of a specific node
        /// </summary>
        public virtual string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Position node
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return "p";
        }
    }

    /// <summary>
    /// Normal node
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return "n";
        }
    }

    /// <summary>
    /// Node that tells us the zero-crossing point
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return "sp";
        }
    }

    /// <summary>
    /// Node that tells us the zero-crossing point
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return "sn";
        }
    }

    /// <summary>
    /// Normal node
    /// </summary>
    [System.Serializable]
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
    }

    /// <summary>
    /// Result node
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float density = {CodeConverter.EvaluatePort(savedVoxelGraph, portguid, 0f)};";
        }
    }

    /// <summary>
    /// CSM Result node
    /// </summary>
    [System.Serializable]
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
    }

    /// <summary>
    /// Voxel Details Result node
    /// </summary>
    [System.Serializable]
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
    }

    /// <summary>
    /// Type of VoxelGraphEditorWindow
    /// </summary>
    public enum VoxelGraphType { Density, CSM, VoxelDetails }

    #region Constant Node Type

    /// <summary>
    /// Constant voxel nodes
    /// </summary>
    [System.Serializable]
    public abstract class VNConstants : VoxelNode
    {
        public object objValue;
    }

    /// <summary>
    /// Constant float class
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float {nodeGuid} = {(float)objValue};";
        }
    }

    /// <summary>
    /// Constant Vector2 class
    /// </summary>
    [System.Serializable]
    public class VNConstantVec2 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Vector2";
        private SavableVec2 val;

        /// <summary>
        /// Creates all the ports and float fields for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            //Reading
            objValue = objValue == null ? new SavableVec2 { x = 0, y = 0 } : objValue;
            var floatField0 = new FloatField();
            var floatField1 = new FloatField();
            //Base setting
            SavableVec2 castedValue = ((SavableVec2)objValue);
            floatField0.value = castedValue.x;
            floatField1.value = castedValue.y;
            //Port creation
            Port port0 = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            //Port list add
            port0.Add(floatField0);
            port1.Add(floatField1);
            //Visual list add
            inputVisualElements.Add(port0);
            inputVisualElements.Add(port1);
            //Register callbacks
            floatField0.RegisterValueChangedCallback(x => { val.x = x.newValue; objValue = val; });
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float2 {nodeGuid} = float2({CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector2)objValue).x)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector2)objValue).y)});";
        }
    }

    /// <summary>
    /// Constant Vector3 class
    /// </summary>
    [System.Serializable]
    public class VNConstantVec3 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Vector3";
        private SavableVec3 val;

        /// <summary>
        /// Creates all the ports and float fields for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            //Reading
            objValue = objValue == null ? new SavableVec3 { x = 0, y = 0, z = 0 } : objValue;
            var floatField0 = new FloatField();
            var floatField1 = new FloatField();
            var floatField2 = new FloatField();
            //Base setting
            SavableVec3 castedValue = ((SavableVec3)objValue);
            floatField0.value = castedValue.x;
            floatField1.value = castedValue.y;
            floatField2.value = castedValue.z;
            //Port creation
            Port port0 = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            Port port2 = CreatePort(Direction.Input, typeof(float), "Input Z", Port.Capacity.Single);
            //Port list add
            port0.Add(floatField0);
            port1.Add(floatField1);
            port2.Add(floatField2);
            //Visual list add
            inputVisualElements.Add(port0);
            inputVisualElements.Add(port1);
            inputVisualElements.Add(port2);
            //Register callbacks
            floatField0.RegisterValueChangedCallback(x => { val.x = x.newValue; objValue = val; });
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float3 {nodeGuid} = float3(" +
                $"{CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector3)objValue).x)}, " +
                $"{CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector3)objValue).y)}, " +
                $"{CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(2).Key, ((Vector3)objValue).z)});";
        }
    }

    /// <summary>
    /// Constant Vector4 class
    /// </summary>
    [System.Serializable]
    public class VNConstantVec4 : VNConstants
    {
        //Main voxel node variables
        public override string name => "Constants/Vector4";
        private SavableVec4 val;
        /// <summary>
        /// Creates all the ports and float fields for this node
        /// </summary>
        private void CreateInputConstantPort()
        {
            //Reading
            objValue = objValue == null ? new SavableVec4() { x = 0, y = 0, z = 0, w = 0 } : objValue;
            var floatField0 = new FloatField();
            var floatField1 = new FloatField();
            var floatField2 = new FloatField();
            var floatField3 = new FloatField();
            //Base setting
            SavableVec4 castedValue = ((SavableVec4)objValue);
            floatField0.value = castedValue.x;
            floatField1.value = castedValue.y;
            floatField2.value = castedValue.z;
            floatField3.value = castedValue.w;
            //Port creation
            Port port0 = CreatePort(Direction.Input, typeof(float), "Input X", Port.Capacity.Single);
            Port port1 = CreatePort(Direction.Input, typeof(float), "Input Y", Port.Capacity.Single);
            Port port2 = CreatePort(Direction.Input, typeof(float), "Input Z", Port.Capacity.Single);
            Port port3 = CreatePort(Direction.Input, typeof(float), "Input W", Port.Capacity.Single);
            //Port list add
            port0.Add(floatField0);
            port1.Add(floatField1);
            port2.Add(floatField2);
            port3.Add(floatField3);
            //Visual list add
            inputVisualElements.Add(port0);
            inputVisualElements.Add(port1);
            inputVisualElements.Add(port2);
            inputVisualElements.Add(port3);
            //Register callbacks
            floatField0.RegisterValueChangedCallback(x => { val.x = x.newValue; objValue = val; });
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float4 {nodeGuid} = float4({CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(0).Key, ((Vector4)objValue).x)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(1).Key, ((Vector4)objValue).y)}, {CodeConverter.EvaluatePort(savedVoxelGraph, ports.ElementAt(2).Key, ((Vector4)objValue).z)});";
        }
    }

    /// <summary>
    /// Constant Color class
    /// </summary>
    [System.Serializable]
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

            //Reading
            objValue = objValue == null ? new SavableVec4() { x = 0, y = 0, z = 0, w = 0 } : objValue;
            //Base setting
            var colorField = new ColorField();
            SavableVec4 castedValue = ((SavableVec4)objValue);
            colorField.value = new Color()
            {
                r = castedValue.x,
                g = castedValue.y,
                b = castedValue.z,
                a = castedValue.w,
            };
            //Port creation
            CreatePort(Direction.Output, typeof(Vector3), "Result", Port.Capacity.Multi);
            //Register callbacks
            colorField.RegisterValueChangedCallback(x => objValue = new SavableVec4() { x = x.newValue.r, y = x.newValue.g, z = x.newValue.b, w = x.newValue.a });

            return (name, inputVisualElements, outputVisualElements);
        }

        /// <summary>
        /// Return the code representation for this node
        /// </summary>
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"float3 {nodeGuid} = float3({((Vector3)objValue).x}, {((Vector3)objValue).y}, {((Vector3)objValue).z});";
        }
    }

    /// <summary>
    /// Constant int class
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"int {nodeGuid} = {(int)objValue};";
        }
    }

    /// <summary>
    /// Constant bool class
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            return $"bool {nodeGuid} = {(bool)objValue};";
        }
    }

    #endregion

    #region Splitter Node Type

    /// <summary>
    /// Splitter voxel nodes
    /// </summary>
    [System.Serializable]
    public abstract class VNSplitters : VoxelNode { }

    /// <summary>
    /// Splitter Vector2 class
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            switch (((GraphViewPortData)ports[portguid].userData).localPortIndex)
            {
                case 1:
                    return $"float {nodeGuid}_x = {((Vector2)objValue).x};";
                case 2:
                    return $"float {nodeGuid}_y = {((Vector2)objValue).y};";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Splitter Vector3 class
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            switch (((GraphViewPortData)ports[portguid].userData).localPortIndex)
            {
                case 1:
                    return $"float {nodeGuid}_x = {((Vector3)objValue).x};";
                case 2:
                    return $"float {nodeGuid}_y = {((Vector3)objValue).y};";
                case 3:
                    return $"float {nodeGuid}_z = {((Vector3)objValue).z};";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Splitter Vector4 class
    /// </summary>
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            switch (((GraphViewPortData)ports[portguid].userData).localPortIndex)
            {
                case 1:
                    return $"float {nodeGuid}_x = {((Vector4)objValue).x};";
                case 2:
                    return $"float {nodeGuid}_y = {((Vector4)objValue).y};";
                case 3:
                    return $"float {nodeGuid}_z = {((Vector4)objValue).z};";
                case 4:
                    return $"float {nodeGuid}_w = {((Vector4)objValue).w};";
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
    [System.Serializable]
    public abstract class VNShape : VoxelNode { }

    /// <summary>
    /// Sphere adas
    /// </summary>
    [System.Serializable]
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
    }

    /// <summary>
    /// Cube
    /// </summary>
    [System.Serializable]
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
    }

    #endregion

    #region CSG Operation Node Type
    /// <summary>
    /// Constructive-Solid-Geometry operations
    /// </summary>
    [System.Serializable]
    public abstract class VNCSGOperation : VoxelNode { }

    /// <summary>
    /// Mathematical operations
    /// </summary>
    [System.Serializable]
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
    }

    #endregion

    #region Arithmetic Operation Node Type

    /// <summary>
    /// Arithmetic operations
    /// </summary>
    //Addition
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }
    //Subtraction
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }
    //Multiplication
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }
    //Division
    [System.Serializable]
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
        public override string CodeRepresentationPort(SavedLocalVoxelGraph savedVoxelGraph, string portguid)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}