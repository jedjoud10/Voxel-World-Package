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
    private ConcurrentDictionary<Chunk, ChunkThreadedMesh> threadedMeshes;
    public Dictionary<OctreeNode, ChunkUpdateRequest> chunkUpdateRequests;
    public HashSet<Chunk> chunksUpdating;
    public Dictionary<OctreeNode, Chunk> chunks;
    public Octree octree;
    public VoxelEditsManager voxelEditsManager;
    public const float reducingFactor = ((float)(VoxelWorld.resolution - 3) / (float)(VoxelWorld.resolution));

    //GPU-CPU Stuff
    private ComputeBuffer buffer;
    private ComputeBuffer buffer1;
    private Voxel[] voxels = new Voxel[resolution * resolution * resolution];
    private NativeArray<Voxel> nativeVoxels;
    private NativeList<MeshTriangle> mcTriangles;
    private NativeList<int> triangles;
    private NativeList<float3> vertices, normals;
    private NativeList<float4> colors;
    private NativeList<float2> uvs;

    //Constant settings
    public const float voxelSize = 1f;//The voxel size in meters (Ex. 0.001 voxelSize is one centimeter voxel size)
    public const int resolution = 32;//The resolution of each chunk> Can either be 8-16-32-64

    //Job system stuff
    private JobHandle vertexMergingHandle;
    private Chunk currentChunk;
    private bool completed = true;

    //Marching squares edge and corner tables
    private readonly Vector3[,] edgesY = new Vector3[,]
    {
        { new Vector3(0, 0, 0), new Vector3(1, 0, 0) },
        { new Vector3(1, 0, 0), new Vector3(1, 0, 1) },
        { new Vector3(1, 0, 1), new Vector3(0, 0, 1) },
        { new Vector3(0, 0, 1), new Vector3(0, 0, 0) },
    };
    private readonly int[,] edgesCornersY = new int[,]
    {
        { 0, 1 },
        { 1, resolution * resolution + 1 },
        { resolution * resolution + 1, resolution * resolution },
        { resolution * resolution, 0 },
    };

    private readonly Vector3[,] edgesZ = new Vector3[,]
    {
        { new Vector3(0, 0, 0), new Vector3(0, 1, 0) },
        { new Vector3(0, 1, 0), new Vector3(1, 1, 0) },
        { new Vector3(1, 1, 0), new Vector3(1, 0, 0) },
        { new Vector3(1, 0, 0), new Vector3(0, 0, 0) },
    };
    private readonly int[,] edgesCornersZ = new int[,]
    {
        { 0, resolution },
        { resolution, resolution + 1 },
        { resolution + 1, 1 },
        { 1, 0 },
    };
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        //Initialize everything
        octree = new Octree(this);
        voxelEditsManager = new VoxelEditsManager(this);
        chunks = new Dictionary<OctreeNode, Chunk>();
        threadedMeshes = new ConcurrentDictionary<Chunk, ChunkThreadedMesh>();
        chunkUpdateRequests = new Dictionary<OctreeNode, ChunkUpdateRequest>();
        chunksUpdating = new HashSet<Chunk>();

        //Setup first time compute shader stuff
        RenderTexture rt = new RenderTexture(2048, 1024, 0);
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
    private void OnDestroy()
    {
        //Release everything
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
        //Generate a mesh for the new chunks
        //Generate the chunks from the requests
        if (chunkUpdateRequests.Count > 0 && Time.frameCount % 3 == 0 && completed)
        {
            KeyValuePair<OctreeNode, ChunkUpdateRequest> request = chunkUpdateRequests.First();
            GenerateMesh(request.Value.chunk, request.Key);
            chunkUpdateRequests.Remove(request.Key);
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
                if (octree.toRemove.Count > 0 && chunkUpdateRequests.Count == 0 && chunksUpdating.Count == 0 && threadedMeshes.Count == 0)
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

        if (!completed && Time.frameCount % 3 == 2)
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
            ChunkThreadedMesh r;
            threadedMeshes.TryRemove(currentChunk, out r);
            chunksUpdating.Remove(currentChunk);
            if (vertices.Length == 0)
            {
                Destroy(currentChunk.chunkGameObject);
            }
            currentChunk.chunkGameObject.GetComponent<MeshRenderer>().material = lowpoly ? lowpolyMaterial : normalMaterial;
            currentChunk.chunkGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            currentChunk.chunkGameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
    //When we create a new chunk
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
    //Create the mesh of the chunk in another thread
    private void MarchingCubesWithSkirtsMultithreaded(object state)
    {
        //2. Generate the mesh from the data in another thread
        Voxel[] localVoxels = (Voxel[])((object[])state)[0];
        OctreeNode node = (OctreeNode)((object[])state)[1];
        Chunk chunk = (Chunk)((object[])state)[2];
        List<int> triangles = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<Vector3> normals = new List<Vector3>();
        //UVs: X = Smoothness, Y = Metallic
        List<Vector2> uvs = new List<Vector2>();
        Vector2 currentUV;

        List<int> map = new List<int>();
        Dictionary<Vector3, int> hashmap = new Dictionary<Vector3, int>();
        int vertexCount = 0;
        for (int z = 0, i; z < resolution - 3; z++)
        {
            for (int y = 0; y < resolution - 3; y++)
            {
                for (int x = 0; x < resolution - 3; x++)
                {
                    
                }
            }
        }
        //Skirts 

        //X Axis
        float reductionFactorChunkScaled = (node.chunkSize / (float)(resolution - 3));
        /*
        CreateSkirtX(0, true);
        CreateSkirtX(resolution - 3, false);
        //Y Axis
        CreateSkirtY(0, false);
        CreateSkirtY(resolution - 3, true);
        //Z Axis
        CreateSkirtZ(0, false);
        CreateSkirtZ(resolution - 3, true);
        */
        /*
        void CreateSkirtX(int slicePoint, bool flip)
        {

        }
        void CreateSkirtY(int slicePoint, bool flip)
        {
            for (int z = 0; z < resolution - 3; z++)
            {
                for (int x = 0; x < resolution - 3; x++)
                {
                    int i = TerrainUtility.FlattenIndex(new Vector3Int(x+1, slicePoint+1, z+1), resolution);
                    //Indexing
                    int msCase = 0;
                    if (localVoxels[i].density < 0) msCase |= 1;
                    if (localVoxels[i + resolution * resolution].density < 0) msCase |= 2;
                    if (localVoxels[i + resolution * resolution + 1].density < 0) msCase |= 4;
                    if (localVoxels[i + 1].density < 0) msCase |= 8;
                    //Get the corners
                    SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
                    cornerVoxels[0] = new SkirtVoxel(localVoxels[i], new Vector3(x, slicePoint, z) * reductionFactorChunkScaled);
                    cornerVoxels[1] = new SkirtVoxel(localVoxels[i + resolution * resolution], new Vector3(x + 1, slicePoint, z) * reductionFactorChunkScaled);
                    cornerVoxels[2] = new SkirtVoxel(localVoxels[i + resolution * resolution + 1], new Vector3(x + 1, slicePoint, z + 1) * reductionFactorChunkScaled);
                    cornerVoxels[3] = new SkirtVoxel(localVoxels[i + 1], new Vector3(x, slicePoint, z + 1) * reductionFactorChunkScaled);
                    //Get each edge's skirtVoxel
                    SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
                    //Run on each edge
                    for (int e = 0; e < 4; e++)
                    {
                        Voxel a = localVoxels[i + edgesCornersY[e, 0]];
                        Voxel b = localVoxels[i + edgesCornersY[e, 1]];
                        float lerpValue = Mathf.InverseLerp(a.density, b.density, isolevel);
                        edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (Vector3.Lerp(edgesY[e, 0], edgesY[e, 1], lerpValue) + new Vector3(x, slicePoint, z)) * reductionFactorChunkScaled);
                    }
                    SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
                }
            }
        }
        void CreateSkirtZ(int slicePoint, bool flip)
        {
            for (int y = 0; y < resolution - 3; y++)
            {
                for (int x = 0; x < resolution - 3; x++)
                {
                    int i = TerrainUtility.FlattenIndex(new Vector3Int(x+1, y+1, slicePoint+1), resolution);
                    //Indexing
                    int msCase = 0;
                    if (localVoxels[i].density < 0) msCase |= 1;
                    if (localVoxels[i + 1].density < 0) msCase |= 2;
                    if (localVoxels[i + resolution + 1].density < 0) msCase |= 4;
                    if (localVoxels[i + resolution].density < 0) msCase |= 8;
                    //Get the corners
                    SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
                    cornerVoxels[0] = new SkirtVoxel(localVoxels[i], new Vector3(x, y, slicePoint) * reductionFactorChunkScaled);
                    cornerVoxels[1] = new SkirtVoxel(localVoxels[i + resolution], new Vector3(x, y + 1, slicePoint) * reductionFactorChunkScaled);
                    cornerVoxels[2] = new SkirtVoxel(localVoxels[i + resolution + 1], new Vector3(x + 1, y + 1, slicePoint) * reductionFactorChunkScaled);
                    cornerVoxels[3] = new SkirtVoxel(localVoxels[i + 1], new Vector3(x + 1, y, slicePoint) * reductionFactorChunkScaled);
                    //Get each edge's skirtVoxel
                    SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
                    //Run on each edge
                    for (int e = 0; e < 4; e++)
                    {
                        Voxel a = localVoxels[i + edgesCornersZ[e, 0]];
                        Voxel b = localVoxels[i + edgesCornersZ[e, 1]];
                        float lerpValue = Mathf.InverseLerp(a.density, b.density, isolevel);
                        edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (Vector3.Lerp(edgesZ[e, 0], edgesZ[e, 1], lerpValue) + new Vector3(x, y, slicePoint)) * reductionFactorChunkScaled);
                    }
                    SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
                }
            }
        }
        
        //Add a single trianle to the mesh (Used for skirts)
        void AddTriangle(SkirtVoxel a, SkirtVoxel b, SkirtVoxel c, bool flip)
        {
            TryAddSkirtVertex(flip ? b : a);
            TryAddSkirtVertex(flip ? a : b);
            TryAddSkirtVertex(c);
            vertexCount += 3;
            triangles.Add(map[vertexCount - 3]);
            triangles.Add(map[vertexCount - 2]);
            triangles.Add(map[vertexCount - 1]);
        }
        //Add a single vertex to the mesh (Also used for skirts)
        void TryAddSkirtVertex(SkirtVoxel skirtVoxel)
        {
            if (!hashmap.ContainsKey(skirtVoxel.pos))
            {
                //First time we generate this vertex
                hashmap.Add(skirtVoxel.pos, vertices.Count);
                map.Add(vertices.Count);
                vertices.Add(skirtVoxel.pos);
                normals.Add(skirtVoxel.normal);
                colors.Add(new Color(skirtVoxel.color.x, skirtVoxel.color.y, skirtVoxel.color.z));
                currentUV.x = skirtVoxel.smoothness;
                currentUV.y = skirtVoxel.metallic;
                uvs.Add(currentUV);
            }
            else
            {
                //Reuse the vertex
                map.Add(hashmap[skirtVoxel.pos]);
            }
        }
    */
    }
    //Generate the mesh for a specific chunk    
    public void GenerateMesh(Chunk chunk, OctreeNode node)
    {
        //1. Generate the data (density, rgb color, roughness, metallic) for the meshing        
        Vector3 chunkOffset = new Vector3(node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3));
        generationShader.SetVector("offset", offset + node.chunkPosition - chunkOffset);
        generationShader.SetFloat("chunkScaling", node.chunkSize / (float)(resolution - 3));
        generationShader.SetFloat("quality", Mathf.Pow((float)node.hierarchyIndex / (float)maxHierarchyIndex, 0.5f));
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
            triangles = mcTriangles.AsParallelWriter(),
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
        vertexMergingHandle = vmJob.Schedule(marchingCubesHandle);
        currentChunk = chunk;
        completed = false;       
        //JobHandle meshingHandle = mJob.Schedule(mcTriangles.Length, 64);
    }
    //Show some debug info
    private void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Nodes in total: " + octree.nodes.Count);
        GUILayout.Label("Nodes to add: " + octree.toAdd.Count);
        GUILayout.Label("Chunks generating: " + chunksUpdating.Count);
        GUILayout.Label("Meshes in other threads: " + threadedMeshes.Count);
        GUILayout.Label("Chunk update requests: " + chunkUpdateRequests.Count);
        GUILayout.Label("Voxel edit requests: " + voxelEditsManager.voxelEditRequestBatches.Count);
        GUILayout.EndVertical();
    }
}