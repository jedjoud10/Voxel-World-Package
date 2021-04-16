using UnityEngine;
using Unity.Mathematics;
using static TerrainUtility;
/// <summary>
/// Utility class for terrain
/// </summary>
public static class TerrainUtility
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
        public float smoothness, metallic;
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
            this.smoothnessMetallicDensity = math.float3(a.smoothness, a.metallic, a.density);
        }
        public SkirtVoxel(Voxel a, Voxel b, float t, float3 pos)
        {
            this.pos = pos;
            this.color = Vector3.Lerp(a.color, b.color, t);
            this.normal = Vector3.Lerp(a.normal, b.normal, t);
            this.smoothnessMetallicDensity = math.lerp(math.float3(a.smoothness, a.metallic, a.density), math.float3(b.smoothness, b.metallic, b.density), t);
        }
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
}
/// <summary>
/// All the possible objects you can create in the GPU density functions
/// </summary>
public abstract class DensityObject
{
    //Abstract variables
    public float3 pos;
    public abstract bool CheckBounds(OctreeNode node);
}