using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Concurrent;
using Jedjoud.VoxelWorld;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
using Unity.Mathematics;

namespace Jedjoud.VoxelWorld
{
    /// <summary>
    /// Chunk manager class
    /// </summary>
    public class VoxelChunkManager : BaseVoxelComponent
    {
        //Unity Inspector vars
        [Range(0, 5)]
        public int targetFrameDelay;
        public Material material;
        public ComputeShader generationShader;
        public float isolevel;
        public Vector3 offset;
        public Vector3 scale = Vector3.one;

        //Main chunk manager vars
        public Dictionary<OctreeNode, ChunkUpdateRequest> chunkUpdateRequests = new Dictionary<OctreeNode, ChunkUpdateRequest>();
        public HashSet<Chunk> chunksUpdating = new HashSet<Chunk>();
        public Dictionary<OctreeNode, Chunk> chunks = new Dictionary<OctreeNode, Chunk>();

        //GPU-CPU Stuff
        private ComputeBuffer voxelsBuffer;
        private Voxel[] voxels = new Voxel[resolution * resolution * resolution];
        private NativeArray<Voxel> nativeVoxels;
        private NativeList<MeshTriangle> mcTriangles;
        private NativeList<int> triangles;
        private NativeList<float3> vertices, normals;
        private NativeList<float4> colors;
        private NativeList<float2> uvs;

        //Job system stuff
        private JobHandle vertexMergingHandle;
        private Chunk currentChunk;
        private bool completed = true;
        private int frameCountSinceLast;

        //Detect when we finish generating the terrain
        [HideInInspector]
        public bool generating;
        private bool finishedStartGeneration = false, tempStartGeneration = false;

        //Callbacks
        public event Action OnFinishedGeneration;
        public event Action<Chunk, OctreeNode> OnGenerateNewChunk;

        /// <summary>
        /// Initialize this chunk manager
        /// </summary>
        public override void Setup(VoxelWorld voxelWorld)
        {
            base.Setup(voxelWorld);
            //Setup first time compute shader stuff
            voxelsBuffer = new ComputeBuffer((resolution) * (resolution) * (resolution), sizeof(float) * 9);
            chunks = new Dictionary<OctreeNode, Chunk>();
            chunkUpdateRequests = new Dictionary<OctreeNode, ChunkUpdateRequest>();
            chunksUpdating = new HashSet<Chunk>();

            generationShader.SetInt("resolution", resolution);
            generationShader.SetVector("scale", scale);
            generationShader.SetVector("generationOffset", offset);
            generationShader.SetFloat("isolevel", isolevel);
            generationShader.SetBuffer(0, "voxelsBuffer", voxelsBuffer);
            generationShader.SetBuffer(1, "voxelsBuffer", voxelsBuffer);
            //CPU Job system allocations
            nativeVoxels = new NativeArray<Voxel>(resolution * resolution * resolution, Allocator.Persistent);
            mcTriangles = new NativeList<MeshTriangle>((resolution - 3) * (resolution - 3) * (resolution - 3) * 5 + (6 * resolution * resolution * 2), Allocator.Persistent);
            vertices = new NativeList<float3>(mcTriangles.Capacity * 3, Allocator.Persistent);
            normals = new NativeList<float3>(mcTriangles.Capacity * 3, Allocator.Persistent);
            colors = new NativeList<float4>(mcTriangles.Capacity * 3, Allocator.Persistent);
            uvs = new NativeList<float2>(mcTriangles.Capacity * 3, Allocator.Persistent);
            triangles = new NativeList<int>(mcTriangles.Capacity * 3, Allocator.Persistent);
        }

        /// <summary>
        /// Update this chunk manager
        /// </summary>
        public void UpdateChunkManager()
        {
            //Some temp stuff
            generating = chunkUpdateRequests.Count > 0 || voxelWorld.octreeManager.toAdd.Count > 0 || voxelWorld.octreeManager.toRemove.Count > 0 || chunksUpdating.Count > 0;

            //Generate a single mesh
            StartChunkGeneration();
            //Create the chunks
            CreateChunks();
            //Remove the chunks
            DeleteChunks();
            //Complete the job after a small delay, or no delay at all
            CompleteChunkGeneration();

            //When the terrain finishes generation for the first time
            if (tempStartGeneration && voxelWorld.octreeManager.toAdd.Count == 0 && chunksUpdating.Count == 0 && chunkUpdateRequests.Count == 0 && !finishedStartGeneration)
            {
                finishedStartGeneration = true;
                OnFinishedGeneration?.Invoke();
            }

            frameCountSinceLast++;
        }

        /// <summary>
        /// Delete all the pending deletion chunks, gotta make this better
        /// </summary>
        private void DeleteChunks()
        {
            if (voxelWorld.octreeManager.toRemove.Count > 0)
            {
                for (int i = 0; i < 128; i++)
                {
                    if (voxelWorld.octreeManager.toRemove.Count > 0 && chunkUpdateRequests.Count == 0 && chunksUpdating.Count == 0)
                    {
                        OctreeNode nodeToRemove = voxelWorld.octreeManager.toRemove[voxelWorld.octreeManager.toRemove.Count - 1];
                        if (chunks.ContainsKey(nodeToRemove))
                        {
                            if (chunks[nodeToRemove].chunkGameObject != null) Destroy(chunks[nodeToRemove].chunkGameObject);
                            //RemoveChunkRequest(octree.toRemove[0]);
                            //Remove from the chunks array
                            chunks.Remove(nodeToRemove);
                            //Dequeue from the octree list
                        }
                        voxelWorld.octreeManager.toRemove.RemoveAt(voxelWorld.octreeManager.toRemove.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Start the mesh generation for the current prioritized chunk
        /// </summary>
        private void StartChunkGeneration()
        {
            if (chunkUpdateRequests.Count > 0 && completed)
            {
                KeyValuePair<OctreeNode, ChunkUpdateRequest> request = chunkUpdateRequests.First();
                GenerateMesh(request.Value.chunk, request.Key);
                chunkUpdateRequests.Remove(request.Key);
                frameCountSinceLast = 0;
            }
        }

        /// <summary>
        /// Make sure the chunk has completed it's mesh generation
        /// </summary>
        private void CompleteChunkGeneration()
        {
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
                voxelWorld.detailsManager.InstantiateVoxelDetails(currentChunk);
                currentChunk.chunkGameObject.GetComponent<MeshRenderer>().material = material;
                currentChunk.chunkGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                currentChunk.chunkGameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }

        /// <summary>
        /// Generate the mesh for a specific chunk
        /// </summary>
        private void GenerateMesh(Chunk chunk, OctreeNode node)
        {
            //1. Generate the data (density, rgb color, roughness, metallic) for the meshing
            OnGenerateNewChunk?.Invoke(chunk, node);
            Vector3 chunkOffset = new Vector3(node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3), node.chunkSize / ((float)resolution - 3));
            voxelWorld.detailsManager.ResetBufferCount();
            generationShader.SetVector("offset", offset + node.chunkPosition - chunkOffset);
            generationShader.SetFloat("chunkScaling", node.chunkSize / (float)(resolution - 3));
            generationShader.SetFloat("quality", Mathf.Pow((float)node.hierarchyIndex / (float)voxelWorld.octreeManager.maxHierarchyIndex, 0.2f));
            generationShader.Dispatch(0, resolution / 8, resolution / 8, resolution / 8);
            generationShader.Dispatch(1, resolution / 8, resolution / 8, resolution / 8);
            voxelsBuffer.GetData(voxels);

            voxelWorld.detailsManager.GetDataFromBuffer(chunk, node);

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
        /// Create all the pending chunks in the octree
        /// </summary>
        private void CreateChunks()
        {
            if (voxelWorld.octreeManager.toAdd.Count > 0)
            {
                tempStartGeneration = true;
                for (int i = 0; i < 16; i++)
                {
                    if (voxelWorld.octreeManager.toAdd.Count > 0)
                    {
                        if (!chunks.ContainsKey(voxelWorld.octreeManager.toAdd[0]))
                        {
                            if (voxelWorld.octreeManager.toAdd[0].isLeaf)
                            {
                                Chunk chunk = CreateChunk(voxelWorld.octreeManager.toAdd[0]);
                                chunks.Add(voxelWorld.octreeManager.toAdd[0], chunk);
                                chunksUpdating.Add(chunk);
                            }
                        }
                        voxelWorld.octreeManager.toAdd.RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// We want to create a new chunk
        /// </summary>
        private Chunk CreateChunk(OctreeNode octreeNode)
        {
            //Don't create a new chunk request when it already exists    
            if (!chunkUpdateRequests.ContainsKey(octreeNode))
            {
                Chunk newChunk;

                GameObject chunkGameObject = Instantiate(voxelWorld.chunkPrefab, octreeNode.chunkPosition, Quaternion.identity);
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
        /// Release all the native containers and the GPU buffer
        /// </summary>
        public override void Release()
        {
            vertexMergingHandle.Complete();
            voxelsBuffer.Release();
            nativeVoxels.Dispose();
            mcTriangles.Dispose();
            triangles.Dispose();
            vertices.Dispose();
            colors.Dispose();
            uvs.Dispose();
            normals.Dispose();
        }

    }
}