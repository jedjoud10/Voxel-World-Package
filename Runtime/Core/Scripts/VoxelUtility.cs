using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using static VoxelUtility;
using static GenerationGraphUtility;
using System;
using System.Linq;
/// <summary>
/// Utility class for terrain
/// </summary>
public static class VoxelUtility
{    
    /// <summary>
    /// Turns a position into an index
    /// </summary>
    /// <param name="position">The position to convert</param>
    /// <returns>The index that was calculated</returns>
    public static int FlattenIndex(int3 position, int size) 
    {
        return (position.z * size * size + (position.y * size) + position.x);
    }

    /// <summary>
    /// Turns an index to a position
    /// </summary>
    /// <param name="index">The index to conver</param>
    /// <returns>The position that was calculated</returns>
    public static int3 UnflattenIndex(int index, int size)
    {
        int z = index / ((size) * (size));
        index -= (z * (size) * (size));
        int y = index / (size);
        int x = index % (size);
        return math.int3(x, y, z);
    }

    /// <summary>
    /// A single "vertex" of Marching Cube data
    /// </summary>
    public struct MeshVertex 
    {
        public float3 position, color, normal;
        public float2 uv;
        //Custom constructor
        public MeshVertex(SkirtVoxel a) 
        {
            this.position = a.pos;
            this.color = a.color;
            this.normal = a.normal;
            this.uv = a.smoothnessMetallicDensity.xy;
        }
    }
    
    /// <summary>
    /// A marching cubes triangle
    /// </summary>
    public struct MeshTriangle 
    {
        public MeshVertex a, b, c;
        //Custom getter setter with index
        public MeshVertex this[int index]
        {
            get 
            {
                switch (index)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    case 2:
                        return c;
                    default:
                        return default(MeshVertex);
                }
            }
            set 
            {
                switch (index)
                {
                    case 0:
                        a = value;
                        break;
                    case 1:
                        b = value;
                        break;
                    case 2:
                        c = value;
                        break;
                    default:
                        break;
                }
            }
        }
        //Custom constructor
        public MeshTriangle(SkirtVoxel a, SkirtVoxel b, SkirtVoxel c) 
        {
            this.a = new MeshVertex(a);
            this.b = new MeshVertex(b);
            this.c = new MeshVertex(c);
        }
    }

    /// <summary>
    /// The camera that the octree could use to create/sort the nodes
    /// </summary>
    public struct CameraData 
    {
        public Vector3 position;
        public Vector3 forwardVector;
    }

    /// <summary>
    /// A chunk that stores data about the world
    /// </summary>
    public struct Chunk
    {
        public Vector3 position;
        public GameObject chunkGameObject;
        public int octreeNodeSize;
    }

    /// <summary>
    /// A singular octree node
    /// </summary>
    public struct OctreeNode
    {
        public int hierarchyIndex, size;
        public Vector3Int position;
        public Vector3 chunkPosition;
        public Vector3 chunkCenter;
        public float chunkSize;
        public bool isLeaf;
    }

    /// <summary>
    /// A chunk update request that will be used to update / generate the mesh for a chunk
    /// </summary>
    public struct ChunkUpdateRequest
    {
        public Chunk chunk;
        public int priority;
    }

    /// <summary>
    /// A chunk mesh from another thread
    /// </summary>
    public struct ChunkThreadedMesh
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Color[] colors;
        public Vector2[] uvs;
        public int[] triangles;
    }

    /// <summary>
    /// A single voxel
    /// </summary>
    public struct Voxel
    {
        public float density;
        public float3 color;
        public float3 normal;
        public float2 sm;
    }
    
    /// <summary>
    /// A skirt voxel
    /// </summary>
    public struct SkirtVoxel
    {
        public float3 pos;
        public float3 color;
        public float3 normal;
        public float3 smoothnessMetallicDensity;
        //Custom constructors
        public SkirtVoxel(Voxel a, float3 pos)
        {
            this.pos = pos;
            this.color = a.color;
            this.normal = a.normal;
            this.smoothnessMetallicDensity = math.float3(a.sm.x, a.sm.y, a.density);
        }
        public SkirtVoxel(Voxel a, Voxel b, float t, float3 pos)
        {
            this.pos = pos;
            this.color = Vector3.Lerp(a.color, b.color, t);
            this.normal = Vector3.Lerp(a.normal, b.normal, t);
            this.smoothnessMetallicDensity = math.lerp(math.float3(a.sm.x, a.sm.y, a.density), math.float3(b.sm.x, b.sm.y, b.density), t);
        }
    }

    /// <summary>
    /// A VoxelDetail that can be spawned on the terrain
    /// </summary>
    public struct VoxelDetail 
    {
        public float3 position;
        public float3 forward;
        public float size;
        public int type;
    }

    /// <summary>
    /// Request intersection test
    /// </summary>
    public static bool NodeIntersectWithBounds(OctreeNode node, VoxelAABBBound bounds)
    {
        return (node.chunkPosition.x <= bounds.max.x && node.chunkPosition.x + node.chunkSize >= bounds.min.x) &&
               (node.chunkPosition.y <= bounds.max.y && node.chunkPosition.y + node.chunkSize >= bounds.min.y) &&
               (node.chunkPosition.z <= bounds.max.z && node.chunkPosition.z + node.chunkSize >= bounds.min.z);
    }

    //Editing parts

    /// <summary>
    /// An AABB Bound
    /// </summary>
    public struct VoxelAABBBound
    {
        //The min and max of the bound
        public Vector3 min, max;
    }

    /// <summary>
    /// A voxel edit requested that is going to be saved / loaded
    /// </summary>
    public struct VoxelEditRequest
    {
        public EditRequest editRequest;
        public VoxelAABBBound bound;
    }

    /// <summary>
    /// A voxel edit request batch, this is used for saving time when doing mass edits
    /// </summary>
    public struct VoxelEditRequestBatch
    {
        public List<VoxelEditRequest> voxelEditRequests;
        public VoxelAABBBound bound;
        //Add a voxel edit request and update the bound if needed
        public void AddVoxelEditRequest(VoxelEditRequest request)
        {
            if (voxelEditRequests != null)
            {
                voxelEditRequests.Add(request);
                bound.max = Vector3.Max(bound.max, request.bound.max);
                bound.min = Vector3.Min(bound.min, request.bound.min);
            }
            else
            {
                voxelEditRequests = new List<VoxelEditRequest>();
            }
        }

        //Create a batch for one voxel editEditRequest
        public VoxelEditRequestBatch(VoxelEditRequest request)
        {
            voxelEditRequests = new List<VoxelEditRequest>();
            voxelEditRequests.Add(request);
            bound = request.bound;
        }
    }
    /// <summary>
    /// A singular edit request that will be passed to the edit compute shader
    /// </summary>
    public struct EditRequest
    {
        public Vector3 center;
        public Vector3 color;
        public float size;
        public int shape;
        public int editType;
    }    
}
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
        protected List<Port> inputPorts = new List<Port>(), outputPorts = new List<Port>();
        protected Node node;

        /// <summary>
        /// Generate a port for a specific node
        /// </summary>
        protected void CreatePort(Direction portDirection, Type type, string name, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
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
        }

        /// <summary>
        /// Get the custom node data for this specific node
        /// </summary>
        public virtual (string, List<Port>, List<Port>) GetCustomNodeData(Node node) 
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
    /// Voxel node
    /// </summary>
    public class VNVoxel : VoxelNodeType
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
        /// Get the custom node data for this specific node
        /// </summary>
        public override (string, List<Port>, List<Port>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Output, typeof(Vector3), "Position", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(Vector3), "Local Position", Port.Capacity.Multi);
            CreatePort(Direction.Output, typeof(Vector3), "Normal", Port.Capacity.Multi);
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
        public override (string, List<Port>, List<Port>) GetCustomNodeData(Node node)
        {
            base.GetCustomNodeData(node);
            CreatePort(Direction.Input, typeof(float), "Density", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector2), "Smoothness and Metallic", Port.Capacity.Single);
            CreatePort(Direction.Input, typeof(Vector3), "Color", Port.Capacity.Single);
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
        public override (string, List<Port>, List<Port>) GetCustomNodeData(Node node)
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
        public override (string, List<Port>, List<Port>) GetCustomNodeData(Node node)
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
        public override (string, List<Port>, List<Port>) GetCustomNodeData(Node node)
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