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

    //GPU Stuff
    private ComputeBuffer buffer;    

    //Constant settings
    public const float voxelSize = 1f;//The voxel size in meters (Ex. 0.001 voxelSize is one centimeter voxel size)
    public const int resolution = 64;//The resolution of each chunk> Can either be 8-16-32- or 64

    //Marching squares edge and corner tables
    private readonly Vector3[,] edgesX = new Vector3[,]
    {
            { new Vector3(0, 0, 0), new Vector3(0, 1, 0) },
            { new Vector3(0, 1, 0), new Vector3(0, 1, 1) },
            { new Vector3(0, 1, 1), new Vector3(0, 0, 1) },
            { new Vector3(0, 0, 1), new Vector3(0, 0, 0) },
    };
    private readonly int[,] edgesCornersX = new int[,]
    {
        { 0, resolution },
        { resolution, resolution + resolution * resolution },
        { resolution + resolution * resolution, resolution * resolution },
        { resolution * resolution, 0 },
    };

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
        RenderTexture rt = new RenderTexture(1024, 1024, 0);
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
    }
    private void OnDestroy()
    {
        //Release everything
        buffer.Release();
    }
    // Update is called once per frame
    void Update()
    {
        //Generate a mesh for the new chunks
        for (int i = 0; i < 1; i++)
        {
            //Generate the chunks from the requests
            if (chunkUpdateRequests.Count > 0 && Time.frameCount % 2 == 0)
            {
                KeyValuePair<OctreeNode, ChunkUpdateRequest> request = chunkUpdateRequests.First();
                GenerateMesh(request.Value.chunk, request.Key);
                chunkUpdateRequests.Remove(request.Key);
            }
        }

        //Create the chunks
        for (int i = 0; i < 64; i++)
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

        //Remove the chunks
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

        //Get the mesh data from the other threads and make a unity mesh out of them
        while (threadedMeshes.Count > 0)
        {
            KeyValuePair<Chunk, ChunkThreadedMesh> threadedMeshPair = threadedMeshes.First();
            if (threadedMeshPair.Key.chunkGameObject != null)
            {
                Mesh mesh = new Mesh
                {
                    vertices = threadedMeshPair.Value.vertices,
                    normals = threadedMeshPair.Value.normals,
                    uv = threadedMeshPair.Value.uvs,
                    colors = threadedMeshPair.Value.colors,
                    triangles = threadedMeshPair.Value.triangles,
                };
                //mesh.RecalculateNormals();
                ChunkThreadedMesh r;
                threadedMeshes.TryRemove(threadedMeshPair.Key, out r);
                chunksUpdating.Remove(threadedMeshPair.Key);
                if (threadedMeshPair.Value.vertices.Length == 0)
                {
                    Destroy(threadedMeshPair.Key.chunkGameObject);
                    continue;
                }
                threadedMeshPair.Key.chunkGameObject.GetComponent<MeshRenderer>().material = lowpoly ? lowpolyMaterial : normalMaterial;
                threadedMeshPair.Key.chunkGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                threadedMeshPair.Key.chunkGameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
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
            for (int y = 0; y < resolution - 3; y++)
            {
                for (int z = 0; z < resolution - 3; z++)
                {
                    int i = TerrainUtility.FlattenIndex(new Vector3Int(slicePoint+1, y+1, z+1), resolution);
                    //Indexing
                    int msCase = 0;
                    if (localVoxels[i].density < 0) msCase |= 1;
                    if (localVoxels[i + resolution * resolution].density < 0) msCase |= 2;
                    if (localVoxels[i + resolution + resolution * resolution].density < 0) msCase |= 4;
                    if (localVoxels[i + resolution].density < 0) msCase |= 8;
                    //Get the corners
                    SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
                    cornerVoxels[0] = new SkirtVoxel(localVoxels[i], new Vector3(slicePoint, y, z) * reductionFactorChunkScaled);
                    cornerVoxels[1] = new SkirtVoxel(localVoxels[i + resolution], new Vector3(slicePoint, y + 1, z) * reductionFactorChunkScaled);
                    cornerVoxels[2] = new SkirtVoxel(localVoxels[i + resolution + resolution * resolution], new Vector3(slicePoint, y + 1, z + 1) * reductionFactorChunkScaled);
                    cornerVoxels[3] = new SkirtVoxel(localVoxels[i + resolution * resolution], new Vector3(slicePoint, y, z + 1) * reductionFactorChunkScaled);
                    //Get each edge's skirtVoxel
                    SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
                    for (int e = 0; e < 4; e++)
                    {
                        Voxel a = localVoxels[i + edgesCornersX[e, 0]];
                        Voxel b = localVoxels[i + edgesCornersX[e, 1]];
                        float lerpValue = Mathf.InverseLerp(a.density, b.density, isolevel);
                        edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (Vector3.Lerp(edgesX[e, 0], edgesX[e, 1], lerpValue) + new Vector3(slicePoint, y, z)) * (reductionFactorChunkScaled));
                    }
                    SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
                }
            }
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
        //Triangulate the marching squares case
        void SolveMarchingSquareCase(int msCase, SkirtVoxel[] cornerVoxels, SkirtVoxel[] edgeMiddleVoxels, bool flip)
        {
            //Please, someone help me, how can I do something other than this!?!?!
            switch (msCase)
            {
                case 0:
                    break;
                case 1:
                    AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], edgeMiddleVoxels[3], flip);
                    break;
                case 2:
                    AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[2], cornerVoxels[3], flip);
                    break;
                case 3:
                    AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], edgeMiddleVoxels[2], flip);
                    AddTriangle(edgeMiddleVoxels[2], cornerVoxels[3], cornerVoxels[0], flip);
                    break;
                case 4:
                    AddTriangle(edgeMiddleVoxels[2], edgeMiddleVoxels[1], cornerVoxels[2], flip);
                    break;
                case 5:
                    AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], edgeMiddleVoxels[3], flip);
                    AddTriangle(cornerVoxels[2], edgeMiddleVoxels[1], edgeMiddleVoxels[2], flip);
                    break;
                case 6:
                    AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[1], cornerVoxels[2], flip);
                    AddTriangle(cornerVoxels[2], cornerVoxels[3], edgeMiddleVoxels[3], flip);
                    break;
                case 7:
                    AddTriangle(cornerVoxels[0], edgeMiddleVoxels[0], cornerVoxels[3], flip);
                    AddTriangle(cornerVoxels[3], edgeMiddleVoxels[1], cornerVoxels[2], flip);
                    AddTriangle(edgeMiddleVoxels[0], edgeMiddleVoxels[1], cornerVoxels[3], flip);
                    break;
                case 8:
                    AddTriangle(edgeMiddleVoxels[1], edgeMiddleVoxels[0], cornerVoxels[1], flip);
                    break;
                case 9:
                    AddTriangle(cornerVoxels[1], edgeMiddleVoxels[1], edgeMiddleVoxels[3], flip);
                    AddTriangle(edgeMiddleVoxels[3], cornerVoxels[0], cornerVoxels[1], flip);
                    break;
                case 10:
                    AddTriangle(cornerVoxels[1], edgeMiddleVoxels[1], edgeMiddleVoxels[0], flip);
                    AddTriangle(cornerVoxels[3], edgeMiddleVoxels[3], edgeMiddleVoxels[2], flip);
                    break;
                case 11:
                    AddTriangle(cornerVoxels[1], edgeMiddleVoxels[1], cornerVoxels[0], flip);
                    AddTriangle(cornerVoxels[0], edgeMiddleVoxels[2], cornerVoxels[3], flip);
                    AddTriangle(edgeMiddleVoxels[1], edgeMiddleVoxels[2], cornerVoxels[0], flip);
                    break;
                case 12:
                    AddTriangle(edgeMiddleVoxels[2], edgeMiddleVoxels[0], cornerVoxels[1], flip);
                    AddTriangle(cornerVoxels[1], cornerVoxels[2], edgeMiddleVoxels[2], flip);
                    break;
                case 13:
                    AddTriangle(cornerVoxels[1], edgeMiddleVoxels[3], cornerVoxels[0], flip);
                    AddTriangle(cornerVoxels[2], edgeMiddleVoxels[2], cornerVoxels[1], flip);
                    AddTriangle(cornerVoxels[1], edgeMiddleVoxels[2], edgeMiddleVoxels[3], flip);
                    break;
                case 14:
                    AddTriangle(cornerVoxels[2], edgeMiddleVoxels[0], cornerVoxels[1], flip);
                    AddTriangle(cornerVoxels[3], edgeMiddleVoxels[3], cornerVoxels[2], flip);
                    AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[0], cornerVoxels[2], flip);
                    break;
                case 15:
                    if ((cornerVoxels[0].density + cornerVoxels[1].density + cornerVoxels[2].density + cornerVoxels[3].density) / 4 > -chunk.octreeNodeSize / 15)
                    {
                        AddTriangle(cornerVoxels[0], cornerVoxels[1], cornerVoxels[2], flip);
                        AddTriangle(cornerVoxels[2], cornerVoxels[3], cornerVoxels[0], flip);
                    }
                    break;
                default:
                    break;
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
        //End
        AddThreadedMesh(chunk, new ChunkThreadedMesh() { vertices = vertices.ToArray(), normals = normals.ToArray(), colors = colors.ToArray(), uvs = uvs.ToArray(), triangles = triangles.ToArray() });
    }
    //Generate the mesh for a specific chunk    
    public void GenerateMesh(Chunk chunk, OctreeNode node)
    {
        //1. Generate the data (density, rgb color, roughness, metallic) for the meshing        
        Vector3 chunkOffset = new Vector3(node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3));
        generationShader.SetVector("offset", offset + node.chunkPosition - chunkOffset);
        generationShader.SetFloat("chunkScaling", node.chunkSize / (float)(resolution - 3));
        generationShader.SetFloat("quality", Mathf.Pow((float)node.hierarchyIndex / (float)maxHierarchyIndex, 0.2f));
        generationShader.Dispatch(0, resolution / 8, resolution / 8, resolution / 8);
        generationShader.Dispatch(1, resolution / 8, resolution / 8, resolution / 8);
        //Voxel[] voxels = new Voxel[(resolution) * (resolution) * (resolution)];
        //ThreadPool.QueueUserWorkItem(MarchingCubesWithSkirtsMultithreaded, new object[3] { voxels, node, chunk });
    }
    //If we already find a threaded mesh in the threadedMesh list, overwrite it
    private void AddThreadedMesh(Chunk chunk, ChunkThreadedMesh threadedMesh)
    {
        if (!threadedMeshes.ContainsKey(chunk))
        {
            threadedMeshes.TryAdd(chunk, threadedMesh);
        }
        else
        {
            ChunkThreadedMesh r;
            threadedMeshes.TryGetValue(chunk, out r);
            threadedMeshes.TryUpdate(chunk, threadedMesh, r);
        }
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