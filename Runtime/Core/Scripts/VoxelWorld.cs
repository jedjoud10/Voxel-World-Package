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
using static Unity.Mathematics.math;
/// <summary>
/// The whole class handling the creation/generation/removal of chunks
/// </summary>
namespace Jedjoud.VoxelWorld
{
    public class VoxelWorld : MonoBehaviour
    {
        //Main settings
        [Header("Main Settings")]
        public bool debug;
        public GameObject chunkPrefab;

        //Other stuff
        #region Some hellish fire bellow    
        public VoxelOctreeManager octreeManager;
        public VoxelChunkManager chunkManager;
        public VoxelDetailsManager detailsManager;
        public VoxelEditsManager editsManager;
        public VoxelWorldPreviewer previewer;
        public CameraData camData, lastFrameCamData;

        //Constant settings
        public const float voxelSize = 1f;//The voxel size in meters (Ex. 0.001 voxelSize is one centimeter voxel size)
        public const int resolution = 32;//The resolution of each chunk> Can either be 8-16-32-64
        public const float reducingFactor = ((float)(VoxelWorld.resolution - 3) / (float)(VoxelWorld.resolution));
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            SetupReferences();
            chunkManager.OnGenerateNewChunk += OnGenerateNewChunk;
            chunkManager.OnFinishedGeneration += OnFinishedGeneration;
        }

        /// <summary>
        /// Setup the references for all the managers that are connected to this voxel world
        /// </summary>
        public void SetupReferences()
        {
            //Initialize everything
            previewer.Setup(this);
            octreeManager.Setup(this);
            editsManager.Setup(this);
            chunkManager.Setup(this);
            detailsManager.Setup(this);
        }

        // Update is called once per frame
        void Update()
        {
            //Update the ChunkManager
            chunkManager.UpdateChunkManager();

            //Update the octree
            if (!chunkManager.generating && Time.frameCount % 20 == 0 || !camData.Equals(lastFrameCamData)) octreeManager.UpdateOctree(camData);
        }

        //----Callbacks----\\
        private void OnGenerateNewChunk(Chunk chunk, OctreeNode node)
        {
        }
        private void OnFinishedGeneration()
        {
        }

        /// <summary>
        /// We want to unsubscribe from the callbacks
        /// </summary>
        void OnDestroy()
        {
            //Release everything
            previewer.Release();
            octreeManager.Release();
            editsManager.Release();
            chunkManager.Release();
            detailsManager.Release();

            chunkManager.OnGenerateNewChunk -= OnGenerateNewChunk;
            chunkManager.OnFinishedGeneration -= OnFinishedGeneration;
        }

        /// <summary>
        /// Draw some gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!debug) return;
            if (octreeManager != null)
            {
                Gizmos.color = Color.green;
                Gizmos.color = Color.red;
                foreach (var node in chunkManager.chunkUpdateRequests)
                {
                    Gizmos.DrawWireCube(node.Key.chunkPosition + new Vector3(node.Key.chunkSize / 2f, node.Key.chunkSize / 2f, node.Key.chunkSize / 2f), new Vector3(node.Key.chunkSize, node.Key.chunkSize, node.Key.chunkSize));
                }
                //Gizmos.DrawWireCube(octree.toRemove[index].center, new Vector3(octree.toRemove[index].size, octree.toRemove[index].size, octree.toRemove[index].size));
            }
        }

        /// <summary>
        /// Some debugging
        /// </summary>
        private void OnGUI()
        {
            if (!debug) return;
            GUILayout.Label("Nodes in total: " + octreeManager.nodes.Count);
            GUILayout.Label("Chunk update requests: " + chunkManager.chunkUpdateRequests.Count);
        }
    }
}