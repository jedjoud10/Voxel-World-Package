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
//The terrain generator handling the generation of chunks and saving/loading of the terrain
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

    //GPU Stuff
    private ComputeBuffer buffer;

    //Constant settings
    public const float voxelSize = 1f;//The voxel size in meters (Ex. 0.001 voxelSize is one centimeter voxel size)
    public const int resolution = 32;//The resolution of each chunk> Can either be 8-16-32- or 64
    private readonly int[,] mcTable = new int[256, 16]{
    {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
    { 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1 },
    { 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1 },
    { 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
    { 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1 },
    { 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1 },
    { 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1 },
    { 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
    { 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1 },
    { 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1 },
    { 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
    { 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1 },
    { 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1 },
    { 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1 },
    { 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1 },
    { 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1 },
    { 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1 },
    { 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
    { 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1 },
    { 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
    { 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1 },
    { 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1 },
    { 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1 },
    { 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
    { 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1 },
    { 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1 },
    { 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
    { 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1 },
    { 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1 },
    { 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1 },
    { 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
    { 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1 },
    { 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1 },
    { 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
    { 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1 },
    { 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1 },
    { 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1 },
    { 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1 },
    { 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1 },
    { 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
    { 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1 },
    { 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
    { 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1 },
    { 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1 },
    { 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1 },
    { 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1 },
    { 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1 },
    { 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1 },
    { 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1 },
    { 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1 },
    { 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1 },
    { 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1 },
    { 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1 },
    { 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1 },
    { 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1 },
    { 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1 },
    { 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1 },
    { 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1 },
    { 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1 },
    { 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1 },
    { 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1 },
    { 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1 },
    { 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1 },
    { 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1 },
    { 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1 },
    { 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1 },
    { 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1 },
    { 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1 },
    { 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1 },
    { 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1 },
    { 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1 },
    { 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1 },
    { 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1 },
    { 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1 },
    { 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1 },
    { 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1 },
    { 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1 },
    { 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1 },
    { 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1 },
    { 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1 },
    { 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1 },
    { 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1 },
    { 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1 },
    { 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1 },
    { 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1 },
    { 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1 },
    { 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1 },
    { 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1 },
    { 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1 },
    { 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1 },
    { 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1 },
    { 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1 },
    { 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1 },
    { 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1 },
    { 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1 },
    { 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1 },
    { 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1 },
    { 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1 },
    { 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1 },
    { 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1 },
    { 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1 },
    { 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1 },
    { 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1 },
    { 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1 },
    { 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1 },
    { 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1 },
    { 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1 },
    { 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1 },
    { 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
    };//Triangulation table from https://paulbourke.net/geometry/polygonise/

    //Marching cubes edge tables
    private readonly Vector3Int[] edgesToCorners = new Vector3Int[12]
    {
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(1, 0, 1),
    };
    private readonly Vector3Int[] edgeToCorners2 = new Vector3Int[12]
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 0),
    };
    private readonly int[] edgesToCornerIndices = new int[12]
    {
        resolution,
        resolution + 1,
        1,
        0,
        resolution * resolution + resolution,
        resolution * resolution + resolution + 1,
        resolution * resolution + 1,
        resolution * resolution,
        resolution * resolution,
        resolution * resolution + resolution,
        resolution * resolution + resolution + 1,
        resolution * resolution + 1,
    };
    private readonly int[] edgesToCornerIndices2 = new int[12]
    {
        0,
        resolution,
        resolution + 1,
        1,
        resolution * resolution,
        resolution * resolution + resolution,
        resolution * resolution + resolution + 1,
        resolution * resolution + 1,
        0,
        resolution,
        resolution + 1,
        1,
    };

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

    private static int FlattenIndex(Vector3Int position)
    {
        return (position.z * resolution * resolution) + (position.y * resolution) + position.x;
    }
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
        RenderTexture rt = new RenderTexture(3915, 3072, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;
        rt.wrapMode = TextureWrapMode.Clamp;
        Graphics.Blit(texture, rt);
        generationShader.SetTexture(0, "animeTexture", rt);
        buffer = new ComputeBuffer((resolution) * (resolution) * (resolution), sizeof(float) * 9);
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
    //Turn an index into a 3d position
    public static Vector3 UnflattenIndex(int index)
    {
        int z = index / ((resolution) * (resolution));
        index -= (z * (resolution) * (resolution));
        int y = index / (resolution);
        int x = index % (resolution);
        return new Vector3(x, y, z);
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
        for (int z = 0, i; z < resolution - 1; z++)
        {
            for (int y = 0; y < resolution - 1; y++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    i = FlattenIndex(new Vector3Int(x, y, z));
                    //Indexing
                    int mcCase = 0;
                    if (localVoxels[i + 0].density < isolevel) mcCase |= 1;
                    if (localVoxels[i + resolution].density < isolevel) mcCase |= 2;
                    if (localVoxels[i + resolution + 1].density < isolevel) mcCase |= 4;
                    if (localVoxels[i + 1].density < isolevel) mcCase |= 8;
                    if (localVoxels[i + resolution * resolution].density < isolevel) mcCase |= 16;
                    if (localVoxels[i + resolution * resolution + resolution].density < isolevel) mcCase |= 32;
                    if (localVoxels[i + resolution * resolution + resolution + 1].density < isolevel) mcCase |= 64;
                    if (localVoxels[i + resolution * resolution + 1].density < isolevel) mcCase |= 128;

                    //Every triangle in this marching cubes case
                    for (int t = 0; t < 15; t++)
                    {
                        int tri = mcTable[mcCase, t];
                        if (tri != -1)
                        {
                            //Find the zero-crossing point
                            float lerpValue = Mathf.InverseLerp(localVoxels[i + edgesToCornerIndices[tri]].density, localVoxels[i + edgesToCornerIndices2[tri]].density, isolevel);
                            Vector3 vertex = (Vector3.Lerp(edgesToCorners[tri], edgeToCorners2[tri], lerpValue) + pos) * (node.chunkSize / (float)(resolution - 1));
                            if (!hashmap.ContainsKey(vertex))
                            {
                                //First time we generate this vertex
                                hashmap.Add(vertex, vertices.Count);
                                map.Add(vertices.Count);
                                vertices.Add(vertex);
                                Voxel a = localVoxels[i + edgesToCornerIndices[tri]];
                                Voxel b = localVoxels[i + edgesToCornerIndices2[tri]];
                                normals.Add(lowpoly ? Vector3.one : Vector3.Lerp(a.normal, b.normal, lerpValue));
                                Vector3 color = Vector3.Lerp(a.color, b.color, lerpValue);
                                colors.Add(new Color(color.x, color.y, color.z, 1.0f));
                                currentUV.x = Mathf.Lerp(a.smoothness, b.smoothness, lerpValue);
                                currentUV.y = Mathf.Lerp(a.metallic, b.metallic, lerpValue);
                                uvs.Add(currentUV);
                            }
                            else
                            {
                                //Reuse the vertex
                                map.Add(hashmap[vertex]);
                            }
                            triangles.Add(map[triangles.Count]);
                            vertexCount++;
                        }
                    }
                }
            }
        }
        //Skirts 
        //X Axis        
        CreateSkirtX(0, true);
        CreateSkirtX(resolution - 1, false);
        //Y Axis
        CreateSkirtY(0, false);
        CreateSkirtY(resolution - 1, true);
        //Z Axis
        CreateSkirtZ(0, false);
        CreateSkirtZ(resolution - 1, true);

        void CreateSkirtX(int slicePoint, bool flip)
        {
            for (int y = 0; y < resolution - 1; y++)
            {
                for (int z = 0; z < resolution - 1; z++)
                {
                    int i = FlattenIndex(new Vector3Int(slicePoint, y, z));
                    //Indexing
                    int msCase = 0;
                    if (localVoxels[i].density < 0) msCase |= 1;
                    if (localVoxels[i + resolution * resolution].density < 0) msCase |= 2;
                    if (localVoxels[i + resolution + resolution * resolution].density < 0) msCase |= 4;
                    if (localVoxels[i + resolution].density < 0) msCase |= 8;
                    //Get the corners
                    SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
                    cornerVoxels[0] = new SkirtVoxel(localVoxels[i], new Vector3(slicePoint, y, z) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[1] = new SkirtVoxel(localVoxels[i + resolution], new Vector3(slicePoint, y + 1, z) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[2] = new SkirtVoxel(localVoxels[i + resolution + resolution * resolution], new Vector3(slicePoint, y + 1, z + 1) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[3] = new SkirtVoxel(localVoxels[i + resolution * resolution], new Vector3(slicePoint, y, z + 1) * (node.chunkSize / (float)(resolution - 1)));
                    //Get each edge's skirtVoxel
                    SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
                    for (int e = 0; e < 4; e++)
                    {
                        Voxel a = localVoxels[i + edgesCornersX[e, 0]];
                        Voxel b = localVoxels[i + edgesCornersX[e, 1]];
                        float lerpValue = Mathf.InverseLerp(a.density, b.density, isolevel);
                        edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (Vector3.Lerp(edgesX[e, 0], edgesX[e, 1], lerpValue) + new Vector3(slicePoint, y, z)) * (node.chunkSize / (float)(resolution - 1)));
                    }
                    SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
                }
            }
        }
        void CreateSkirtY(int slicePoint, bool flip)
        {
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int i = FlattenIndex(new Vector3Int(x, slicePoint, z));
                    //Indexing
                    int msCase = 0;
                    if (localVoxels[i].density < 0) msCase |= 1;
                    if (localVoxels[i + resolution * resolution].density < 0) msCase |= 2;
                    if (localVoxels[i + resolution * resolution + 1].density < 0) msCase |= 4;
                    if (localVoxels[i + 1].density < 0) msCase |= 8;
                    //Get the corners
                    SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
                    cornerVoxels[0] = new SkirtVoxel(localVoxels[i], new Vector3(x, slicePoint, z) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[1] = new SkirtVoxel(localVoxels[i + resolution * resolution], new Vector3(x + 1, slicePoint, z) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[2] = new SkirtVoxel(localVoxels[i + resolution * resolution + 1], new Vector3(x + 1, slicePoint, z + 1) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[3] = new SkirtVoxel(localVoxels[i + 1], new Vector3(x, slicePoint, z + 1) * (node.chunkSize / (float)(resolution - 1)));
                    //Get each edge's skirtVoxel
                    SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
                    //Run on each edge
                    for (int e = 0; e < 4; e++)
                    {
                        Voxel a = localVoxels[i + edgesCornersY[e, 0]];
                        Voxel b = localVoxels[i + edgesCornersY[e, 1]];
                        float lerpValue = Mathf.InverseLerp(a.density, b.density, isolevel);
                        //lerpValue = 0.5f;
                        edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (Vector3.Lerp(edgesY[e, 0], edgesY[e, 1], lerpValue) + new Vector3(x, slicePoint, z)) * (node.chunkSize / (float)(resolution - 1)));
                    }
                    SolveMarchingSquareCase(msCase, cornerVoxels, edgeMiddleVoxels, flip);
                }
            }
        }
        void CreateSkirtZ(int slicePoint, bool flip)
        {
            for (int y = 0; y < resolution - 1; y++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int i = FlattenIndex(new Vector3Int(x, y, slicePoint));
                    //Indexing
                    int msCase = 0;
                    if (localVoxels[i].density < 0) msCase |= 1;
                    if (localVoxels[i + 1].density < 0) msCase |= 2;
                    if (localVoxels[i + resolution + 1].density < 0) msCase |= 4;
                    if (localVoxels[i + resolution].density < 0) msCase |= 8;
                    //Get the corners
                    SkirtVoxel[] cornerVoxels = new SkirtVoxel[4];
                    cornerVoxels[0] = new SkirtVoxel(localVoxels[i], new Vector3(x, y, slicePoint) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[1] = new SkirtVoxel(localVoxels[i + resolution], new Vector3(x, y + 1, slicePoint) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[2] = new SkirtVoxel(localVoxels[i + resolution + 1], new Vector3(x + 1, y + 1, slicePoint) * (node.chunkSize / (float)(resolution - 1)));
                    cornerVoxels[3] = new SkirtVoxel(localVoxels[i + 1], new Vector3(x + 1, y, slicePoint) * (node.chunkSize / (float)(resolution - 1)));
                    //Get each edge's skirtVoxel
                    SkirtVoxel[] edgeMiddleVoxels = new SkirtVoxel[4];
                    //Run on each edge
                    for (int e = 0; e < 4; e++)
                    {
                        Voxel a = localVoxels[i + edgesCornersZ[e, 0]];
                        Voxel b = localVoxels[i + edgesCornersZ[e, 1]];
                        float lerpValue = Mathf.InverseLerp(a.density, b.density, isolevel);
                        edgeMiddleVoxels[e] = new SkirtVoxel(a, b, lerpValue, (Vector3.Lerp(edgesZ[e, 0], edgesZ[e, 1], lerpValue) + new Vector3(x, y, slicePoint)) * (node.chunkSize / (float)(resolution - 1)));
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
                    AddTriangle(edgeMiddleVoxels[0], edgeMiddleVoxels[3], edgeMiddleVoxels[2], flip);
                    AddTriangle(edgeMiddleVoxels[1], edgeMiddleVoxels[2], edgeMiddleVoxels[0], flip);
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
                    AddTriangle(cornerVoxels[1], edgeMiddleVoxels[0], edgeMiddleVoxels[1], flip);
                    AddTriangle(cornerVoxels[3], edgeMiddleVoxels[2], edgeMiddleVoxels[3], flip);
                    AddTriangle(edgeMiddleVoxels[0], edgeMiddleVoxels[1], edgeMiddleVoxels[3], flip);
                    AddTriangle(edgeMiddleVoxels[3], edgeMiddleVoxels[1], edgeMiddleVoxels[2], flip);
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
                    if ((cornerVoxels[0].density + cornerVoxels[1].density + cornerVoxels[2].density + cornerVoxels[3].density) / 4 > -10)
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
        //End
        AddThreadedMesh(chunk, new ChunkThreadedMesh() { vertices = vertices.ToArray(), normals = normals.ToArray(), colors = colors.ToArray(), uvs = uvs.ToArray(), triangles = triangles.ToArray() });
    }
    //Generate the mesh for a specific chunk    
    public void GenerateMesh(Chunk chunk, OctreeNode node)
    {
        //1. Generate the data (density, rgb color, roughness, metallic) for the meshing        
        generationShader.SetVector("offset", offset + node.chunkPosition);
        generationShader.SetVector("scale", scale);
        generationShader.SetFloat("chunkScaling", node.chunkSize / (float)(resolution - 1));
        generationShader.SetBuffer(0, "buffer", buffer);
        generationShader.SetInt("resolution", resolution);
        generationShader.SetFloat("quality", Mathf.Pow((float)node.hierarchyIndex / (float)maxHierarchyIndex, 0.5f));
        generationShader.Dispatch(0, resolution / 8, resolution / 8, resolution / 8);
        Voxel[] voxels = new Voxel[(resolution) * (resolution) * (resolution)];
        buffer.GetData(voxels);
        ThreadPool.QueueUserWorkItem(MarchingCubesWithSkirtsMultithreaded, new object[3] { voxels, node, chunk });
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
//A chunk that stores data about the world
public struct Chunk 
{
    public Vector3 position;
    public GameObject chunkGameObject;
    public int octreeNodeSize;
}
//A chunk update request that will be used to update / generate the mesh for a chunk
public struct ChunkUpdateRequest 
{
    public Chunk chunk;
    public int priority;
}
//A chunk mesh from another thread
public struct ChunkThreadedMesh 
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public Color[] colors;
    public Vector2[] uvs;
    public int[] triangles;
}
//A single voxel
public struct Voxel 
{
    public float density;
    public Vector3 color;
    public Vector3 normal;
    public float smoothness, metallic;
}
//A skirt voxel
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