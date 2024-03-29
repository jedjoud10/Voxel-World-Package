using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Jedjoud.VoxelWorld;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;

/// <summary>
/// My octree implementation
/// </summary>
namespace Jedjoud.VoxelWorld
{
    public class VoxelOctreeManager : BaseVoxelComponent
    {
        //Main octree vars
        public List<OctreeNode> nodes;
        public List<OctreeNode> toAdd, toRemove;
        private Dictionary<OctreeNode, OctreeNodeChildrenCarrier> nodesChildrenCarrier;
        [Range(0, 24)]
        public int maxHierarchyIndex;
        private struct OctreeNodeChildrenCarrier
        {
            public int[] children;
        }

        /// <summary>
        /// Initialize this octree manager
        /// </summary>
        public override void Setup(VoxelWorld voxelWorld)
        {
            base.Setup(voxelWorld);
            nodes = new List<OctreeNode>();
            toAdd = new List<OctreeNode>();
            toRemove = new List<OctreeNode>();
            nodesChildrenCarrier = new Dictionary<OctreeNode, OctreeNodeChildrenCarrier>();
        }

        /// <summary>
        /// Create a new octree, and find the differences between this octree and last's frame octree
        /// </summary>
        /// <param name="cameraData">CameraData that you pass to the octree. Ex: Position and ForwardVector</param>
        public void UpdateOctree(CameraData cameraData)
        {
            //CreateOctreeThreaded(new object[] { cameraData });
            ThreadPool.QueueUserWorkItem(CreateOctreeThreaded, new object[] { cameraData });
        }

        /// <summary>
        /// Create the octree and all in another thread
        /// </summary>
        /// <param name="state">State passed from main thread</param>
        public void CreateOctreeThreaded(object state)
        {
            Dictionary<OctreeNode, int> localNodeIndexPointers = new Dictionary<OctreeNode, int>();
            List<OctreeNode> nodesToProcess = new List<OctreeNode>();
            List<OctreeNode> newNodes = new List<OctreeNode>();
            Dictionary<OctreeNode, OctreeNodeChildrenCarrier> localChildrenCarriers = new Dictionary<OctreeNode, OctreeNodeChildrenCarrier>();
            CameraData cameraData = (CameraData)(((object[])state)[0]);
            //Setup root octree
            float reducingFactor = ((float)(VoxelWorld.resolution - 3) / (float)(VoxelWorld.resolution));
            OctreeNode rootOctree = new OctreeNode();
            rootOctree.hierarchyIndex = 0;
            rootOctree.size = Mathf.RoundToInt(Mathf.Pow(2, maxHierarchyIndex)) * VoxelWorld.resolution;
            rootOctree.position = new Vector3Int(-rootOctree.size, -rootOctree.size, -rootOctree.size) / 2;
            rootOctree.chunkSize = rootOctree.size * reducingFactor;
            rootOctree.chunkPosition = new Vector3(rootOctree.position.x, rootOctree.position.y, rootOctree.position.z) * reducingFactor;
            //Main iterations
            localNodeIndexPointers.Add(rootOctree, 0);
            nodesToProcess.Add(rootOctree);
            newNodes.Add(rootOctree);
            //int3 gridSize = new int3(maxSize, maxSize, maxSize);
            //Create the octree

            for (int i = 0; i < nodesToProcess.Count; i++)
            {
                OctreeNode octreeParentNode = nodesToProcess[i];
                //Create octree children 

                //Create children
                int childrenIndex = 0;
                int[] childrenPointers = new int[8];
                if (Vector3.Distance(cameraData.position, octreeParentNode.chunkPosition + new Vector3(octreeParentNode.chunkSize / 2f, octreeParentNode.chunkSize / 2f, octreeParentNode.chunkSize / 2f)) < (octreeParentNode.chunkSize) && octreeParentNode.hierarchyIndex < maxHierarchyIndex)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            for (int z = 0; z < 2; z++)
                            {
                                //Setup Octree child
                                OctreeNode octreeChild = new OctreeNode();
                                octreeChild.size = octreeParentNode.size / 2;
                                octreeChild.position = (octreeChild.size * new Vector3Int(x, y, z)) + octreeParentNode.position;
                                //Chunk sizes
                                //Took me literally 2 months to realize that I could've done this
                                octreeChild.chunkSize = octreeChild.size * reducingFactor;
                                octreeChild.chunkPosition = new Vector3(octreeChild.position.x, octreeChild.position.y, octreeChild.position.z) * reducingFactor;
                                octreeChild.chunkCenter = octreeChild.chunkPosition + new Vector3(octreeChild.chunkSize / 2f, octreeChild.chunkSize / 2f, octreeChild.chunkSize / 2f);

                                octreeChild.hierarchyIndex = octreeParentNode.hierarchyIndex + 1;
                                octreeChild.isLeaf = true;
                                childrenPointers[childrenIndex] = newNodes.Count;

                                //Should we even add this node?
                                if (BoundCheckOptimization.CheckNode(octreeChild, cameraData))
                                {
                                    localNodeIndexPointers.Add(octreeChild, newNodes.Count);
                                    nodesToProcess.Add(octreeChild);
                                    newNodes.Add(octreeChild);
                                }
                                childrenIndex++;
                            }
                        }
                    }
                    int index = localNodeIndexPointers[octreeParentNode];
                    newNodes.RemoveAt(index);
                    octreeParentNode.isLeaf = false;
                    newNodes.Insert(index, octreeParentNode);

                    //newNodes.Add(octreeParentNode);
                }
                localChildrenCarriers.Add(octreeParentNode, new OctreeNodeChildrenCarrier { children = childrenPointers });
            }

            //Find what nodes we added/removed from the octree
            HashSet<OctreeNode> addedOctreeNodesHashset = new HashSet<OctreeNode>(newNodes);
            HashSet<OctreeNode> removedOctreeNodesHashset = new HashSet<OctreeNode>(nodes);

            HashSet<OctreeNode> difference = new HashSet<OctreeNode>(newNodes);
            difference.SymmetricExceptWith(nodes);
            addedOctreeNodesHashset.IntersectWith(difference);
            removedOctreeNodesHashset.IntersectWith(difference);

            //Gotta make this override the current chunk requuests later on
            if (toAdd.Count == 0 && toRemove.Count == 0)
            {
                toRemove.Clear();
                toAdd.Clear();
                List<OctreeNode> toAddLocal = new List<OctreeNode>(addedOctreeNodesHashset);
                toAdd = toAddLocal.OrderByDescending(x => (Vector3.Dot((x.chunkPosition - cameraData.position).normalized, cameraData.forwardVector)) * 30 + x.hierarchyIndex).ToList();
                toRemove.AddRange(removedOctreeNodesHashset);
                nodesChildrenCarrier = localChildrenCarriers;
                nodes = newNodes;//Update octree
            }
        }
    }
}