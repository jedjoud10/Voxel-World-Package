using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using static TerrainUtility;
using Unity.Mathematics;
/// <summary>
/// The whole class handling the creation/generation/removal of chunks
/// </summary>
public class VoxelWorld : MonoBehaviour
{
    //Main settings
    [Header("Main Settings")]
    public GameObject chunkPrefab;
    public bool lowpoly;
    public Material normalMaterial;
    public Material lowpolyMaterial;
    [Range(0, 5)]
    public int targetFrameDelay = 5;

    [Header("Octree options")]
    [Range(1, 24)]
    public int maxHierarchyIndex;
    public float LODBias;

    [Header("Generation Settings")]
    public ComputeShader generationShader;
    public Texture2D texture;
    public float isolevel;
    public Vector3 offset, scale;

    //Other stuff
    #region Some hellish fire bellow
    public Dictionary<OctreeNode, ChunkUpdateRequest> chunkUpdateRequests;
    public HashSet<Chunk> chunksUpdating;
    public Dictionary<OctreeNode, Chunk> chunks;
    public Octree octree;
    public VoxelEditsManager voxelEditsManager;
    [HideInInspector]
    public bool generating;

    //GPU-CPU Stuff
    private ComputeBuffer buffer;
    private Voxel[] voxels = new Voxel[resolution * resolution * resolution];
    private NativeArray<Voxel> nativeVoxels;
    private NativeList<MeshTriangle> mcTriangles;
    private NativeList<int> triangles;
    private NativeList<float3> vertices, normals;
    private NativeList<float4> colors;
    private NativeList<float2> uvs;

    //Constant settings
    public const float voxelSize = 1f;//The voxel size in meters (Ex. 0.001 voxelSize is one centimeter voxel size)
    public const int resolution = 64;//The resolution of each chunk> Can either be 8-16-32-64
    public const float reducingFactor = ((float)(VoxelWorld.resolution - 3) / (float)(VoxelWorld.resolution));


    //Job system stuff
    private JobHandle vertexMergingHandle;
    private Chunk currentChunk;
    private bool completed = true;
    private int frameCountSinceLast;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        //Initialize everything
        octree = new Octree(this);
        voxelEditsManager = new VoxelEditsManager(this);
        chunks = new Dictionary<OctreeNode, Chunk>();
        chunkUpdateRequests = new Dictionary<OctreeNode, ChunkUpdateRequest>();
        chunksUpdating = new HashSet<Chunk>();

        //Setup first time compute shader stuff
        RenderTexture rt = new RenderTexture(texture.width, texture.height, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;
        rt.wrapMode = TextureWrapMode.Clamp;
        Graphics.Blit(texture, rt);
        generationShader.SetTexture(0, "animeTexture", rt);
        generationShader.SetTexture(1, "animeTexture", rt);

        generationShader.SetInt("resolution", resolution);
        generationShader.SetVector("scale", scale);

        buffer = new ComputeBuffer((resolution) * (resolution) * (resolution), sizeof(float) * 9);

        generationShader.SetBuffer(0, "buffer", buffer);
        generationShader.SetBuffer(1, "buffer", buffer);

        //Cpu Job system allocations
        nativeVoxels = new NativeArray<Voxel>(resolution * resolution * resolution, Allocator.Persistent);
        mcTriangles = new NativeList<MeshTriangle>((resolution - 3) * (resolution - 3) * (resolution - 3) * 5 + (6 * resolution * resolution * 2), Allocator.Persistent);
        vertices = new NativeList<float3>(mcTriangles.Capacity * 3, Allocator.Persistent);
        normals = new NativeList<float3>(mcTriangles.Capacity * 3, Allocator.Persistent);
        colors = new NativeList<float4>(mcTriangles.Capacity * 3, Allocator.Persistent);
        uvs = new NativeList<float2>(mcTriangles.Capacity * 3, Allocator.Persistent);
        triangles = new NativeList<int>(mcTriangles.Capacity * 3, Allocator.Persistent);
    }
    // Called when this gameObject gets destroyed
    private void OnDestroy()
    {
        //Release everything
        vertexMergingHandle.Complete();
        buffer.Release();
        nativeVoxels.Dispose();
        mcTriangles.Dispose();
        triangles.Dispose();
        vertices.Dispose();
        colors.Dispose();
        uvs.Dispose();
        normals.Dispose();
    }
    // Update is called once per frame
    void Update()
    {
        generating = chunkUpdateRequests.Count > 0 || octree.toAdd.Count > 0 || octree.toRemove.Count > 0 || chunksUpdating.Count > 0;
        //Generate a single mesh
        if (chunkUpdateRequests.Count > 0 && completed)
        {
            KeyValuePair<OctreeNode, ChunkUpdateRequest> request = chunkUpdateRequests.First();
            GenerateMesh(request.Value.chunk, request.Key);
            chunkUpdateRequests.Remove(request.Key);
            frameCountSinceLast = 0;
        }
        
        //Create the chunks
        if (octree.toAdd.Count > 0)
        {
            for (int i = 0; i < 16; i++)
            {
                if (octree.toAdd.Count > 0)
                {
                    if (!chunks.ContainsKey(octree.toAdd[0]))
                    {
                        if (octree.toAdd[0].isLeaf)
                        {
                            Chunk chunk = CreateNewChunk(octree.toAdd[0]);
                            chunks.Add(octree.toAdd[0], chunk);
                            chunksUpdating.Add(chunk);
                        }
                    }
                    octree.toAdd.RemoveAt(0);
                }
            }
        }
        //Remove the chunks
        if (octree.toRemove.Count > 0)
        {
            for (int i = 0; i < 128; i++)
            {
                if (octree.toRemove.Count > 0 && chunkUpdateRequests.Count == 0 && chunksUpdating.Count == 0)
                {
                    OctreeNode nodeToRemove = octree.toRemove[octree.toRemove.Count - 1];
                    if (chunks.ContainsKey(nodeToRemove))
                    {
                        if (chunks[nodeToRemove].chunkGameObject != null) Destroy(chunks[nodeToRemove].chunkGameObject);
                        //RemoveChunkRequest(octree.toRemove[0]);
                        //Remove from the chunks array
                        chunks.Remove(nodeToRemove);
                        //Dequeue from the octree list
                    }
                    octree.toRemove.RemoveAt(octree.toRemove.Count - 1);
                }
            }
        }

        //Complete the job after a small delay, or no delay at all
        if (!completed && (vertexMergingHandle.IsCompleted && frameCountSinceLast > targetFrameDelay))
        {
            vertexMergingHandle.Complete();
            completed = true;

            //Create the mesh and update it
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices.AsArray());
            mesh.SetNormals(normals.AsArray());
            mesh.SetUVs(0, uvs.AsArray());
            mesh.SetColors(colors.AsArray());
            mesh.SetIndices(triangles.AsArray(), MeshTopology.Triangles, 0);
            chunksUpdating.Remove(currentChunk);
            if (vertices.Length == 0)
            {
                Destroy(currentChunk.chunkGameObject);
            }
            currentChunk.chunkGameObject.GetComponent<MeshRenderer>().material = lowpoly ? lowpolyMaterial : normalMaterial;
            currentChunk.chunkGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            currentChunk.chunkGameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        frameCountSinceLast++;
    }
    /// <summary>
    /// Remap value from one range to another. https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    /// </summary>
    /// <param name="value">Value to remap</param>
    /// <param name="from1">Start 1</param>
    /// <param name="to1">End 1</param>
    /// <param name="from2">Start 2</param>
    /// <param name="to2">End 2</param>
    /// <returns></returns>
    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    /// <summary>
    /// We want to create a new chunk
    /// </summary>
    public Chunk CreateNewChunk(OctreeNode octreeNode)
    {
        //return;return newChunk;
        //Don't create a new chunk request when it already exists    
        if (!chunkUpdateRequests.ContainsKey(octreeNode))
        {
            Chunk newChunk;

            GameObject chunkGameObject = Instantiate(chunkPrefab, octreeNode.chunkPosition, Quaternion.identity);
            chunkGameObject.transform.parent = transform;

            newChunk.chunkGameObject = chunkGameObject;
            newChunk.octreeNodeSize = octreeNode.size;
            newChunk.position = octreeNode.chunkPosition;
            chunkUpdateRequests.Add(octreeNode, new ChunkUpdateRequest() { chunk = newChunk, priority = octreeNode.hierarchyIndex });
            return newChunk;
        }
        else
        {
            Chunk newChunk = chunkUpdateRequests[octreeNode].chunk;
            newChunk.octreeNodeSize = octreeNode.size;
            newChunk.position = octreeNode.chunkPosition;
            chunkUpdateRequests.Remove(octreeNode);
            chunkUpdateRequests.Add(octreeNode, new ChunkUpdateRequest() { chunk = newChunk, priority = octreeNode.hierarchyIndex });
            return newChunk;
        }
    }    
    
    /// <summary>
    /// Generate the mesh for a specific chunk
    /// </summary>
    public void GenerateMesh(Chunk chunk, OctreeNode node)
    {
        //1. Generate the data (density, rgb color, roughness, metallic) for the meshing        
        Vector3 chunkOffset = new Vector3(node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3));
        generationShader.SetVector("offset", offset + node.chunkPosition - chunkOffset);
        generationShader.SetFloat("chunkScaling", node.chunkSize / (float)(resolution - 3));
        generationShader.SetFloat("quality", Mathf.Pow((float)node.hierarchyIndex / (float)maxHierarchyIndex, 0.4f));
        generationShader.Dispatch(0, resolution / 8, resolution / 8, resolution / 8);
        generationShader.Dispatch(1, resolution / 8, resolution / 8, resolution / 8);
        buffer.GetData(voxels);
        
        nativeVoxels.CopyFrom(voxels);
        triangles.Clear();
        mcTriangles.Clear();
        vertices.Clear();
        normals.Clear();
        colors.Clear();
        uvs.Clear();

        //Job system
        MarchingCubesJob mcjob = new MarchingCubesJob()
        {
            chunkSize = node.chunkSize,
            isolevel = isolevel,
            mcTriangles = mcTriangles.AsParallelWriter(),
            voxels = nativeVoxels
        };
        //Skirts
        SkirtsJob skirtsJob = new SkirtsJob()
        {
            chunkSize = node.chunkSize,
            isolevel = isolevel,
            reductionFactorChunkScaled = (node.chunkSize / (float)(resolution - 3)),
            mcTriangles = mcTriangles.AsParallelWriter(),
            voxels = nativeVoxels
        };

        VertexMergingJob vmJob = new VertexMergingJob()
        {
            lowpoly = lowpoly,
            mcTriangles = mcTriangles,
            vertices = vertices,
            normals = normals,
            colors = colors,
            uvs = uvs,
            triangles = triangles,
        };

        JobHandle marchingCubesHandle = mcjob.Schedule((resolution - 3) * (resolution - 3) * (resolution - 3), 128);
        //marchingCubesHandle.Complete();

        JobHandle skirtsHandle = skirtsJob.Schedule((resolution - 3) * (resolution - 3) * 6, 128, marchingCubesHandle);

        vertexMergingHandle = vmJob.Schedule(JobHandle.CombineDependencies(skirtsHandle, marchingCubesHandle));
        currentChunk = chunk;
        completed = false;       
    }
    /// <summary>
    /// Draw some gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (octree != null)
        {
            Gizmos.color = Color.green;
            Gizmos.color = Color.red;
            foreach (var node in octree.nodes)
            {
                Gizmos.DrawWireCube(node.chunkPosition + new Vector3(node.chunkSize / 2f, node.chunkSize / 2f, node.chunkSize / 2f), new Vector3(node.chunkSize, node.chunkSize, node.chunkSize));
            }
            if (chunkUpdateRequests.Count > 0)
            {
                Gizmos.DrawWireCube(chunkUpdateRequests.First().Key.chunkPosition + new Vector3(chunkUpdateRequests.First().Key.chunkSize / 2f, chunkUpdateRequests.First().Key.chunkSize / 2f, chunkUpdateRequests.First().Key.chunkSize / 2f), new Vector3(chunkUpdateRequests.First().Key.chunkSize, chunkUpdateRequests.First().Key.chunkSize, chunkUpdateRequests.First().Key.chunkSize));
            }
            //Gizmos.DrawWireCube(octree.toRemove[index].center, new Vector3(octree.toRemove[index].size, octree.toRemove[index].size, octree.toRemove[index].size));
        }
    }
    //Show some debug info
    private void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Nodes in total: " + octree.nodes.Count);
        GUILayout.Label("Nodes to add: " + octree.toAdd.Count);
        GUILayout.Label("Chunks generating: " + chunksUpdating.Count);
        GUILayout.Label("Chunk update requests: " + chunkUpdateRequests.Count);
        GUILayout.Label("Voxel edit requests: " + voxelEditsManager.voxelEditRequestBatches.Count);
        GUILayout.EndVertical();
    }
}