using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
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
        return new int3(x, y, z);
    }

    /// <summary>
    /// A single "vertex" of Marching Cube data
    /// </summary>
    public struct VertexData 
    {
        public float3 position, color, normal;
        public float2 smoothness, metallic;
    }
    
    /// <summary>
    /// A marching cubes triangle
    /// </summary>
    public struct MeshTriangle 
    {
        VertexData a, b, c;
        //Custom getter setter with index
        public VertexData this[int index]
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
                        return default(VertexData);
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
        public Vector3 color;
        public Vector3 normal;
        public float smoothness, metallic;
    }
    
    /// <summary>
    /// A skirt voxel
    /// </summary>
    public struct SkirtVoxel
    {
        public SkirtVoxel(Voxel a, Vector3 pos)
        {
            this.pos = pos;
            this.color = a.color;
            this.normal = a.normal;
            this.smoothness = a.smoothness;
            this.metallic = a.metallic;
            this.density = a.density;
        }
        public SkirtVoxel(Voxel a, Voxel b, float t, Vector3 pos)
        {
            this.pos = pos;
            this.color = Vector3.Lerp(a.color, b.color, t);
            this.normal = Vector3.Lerp(a.normal, b.normal, t);
            this.smoothness = Mathf.Lerp(a.smoothness, b.smoothness, t);
            this.metallic = Mathf.Lerp(a.metallic, b.metallic, t);
            this.density = Mathf.Lerp(a.density, b.density, t);
        }
        public Vector3 pos;
        public Vector3 color;
        public Vector3 normal;
        public float smoothness, metallic, density;
    }
}