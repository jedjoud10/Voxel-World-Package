using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

//A whole octree
public class Octree
{
    public List<OctreeNode> nodes;//All the nodes we have in the octree
    public List<OctreeNode> toAdd, toRemove;//The nodes we added/removed from last's frame octree
    private Dictionary<OctreeNode, OctreeNodeChildrenCarrier> nodesChildrenCarrier;//A dictionary containting all the children of specific nodes
    private VoxelWorld voxelWorld;//Reference to voxelWorld
    public int maxHierarchyIndex;//The max hierarchy index allowed in the octree and the resolution of the chunks
    private struct OctreeNodeChildrenCarrier
    {
        public int[] children;
    }
    //Initialize octree
    public Octree(VoxelWorld _world)
    {
        maxHierarchyIndex = _world.maxHierarchyIndex;
        nodes = new List<OctreeNode>();
        toAdd = new List<OctreeNode>();
        toRemove = new List<OctreeNode>();
        nodesChildrenCarrier = new Dictionary<OctreeNode, OctreeNodeChildrenCarrier>();
        voxelWorld = _world;
    }

    //Create a new octree, and find the differences between this octree and last's frame octree
    public void UpdateOctree(Vector3 _cameraPosition)
    {
        //CreateOctreeThreaded(new object[] { _cameraPosition });
        ThreadPool.QueueUserWorkItem(CreateOctreeThreaded, new object[] { _cameraPosition });
    }
    //Checks what nodes we need to edit
    public void CheckNodesToEdit(VoxelEditRequestBatch voxelEditRequestBatch)
    {
        //CheckNodesToEditThreaded(new object[] { voxelEditRequest });
        //CheckNodesToEditThreaded(new object[] { (center - new Vector3(size / 2, size / 2, size / 2)), (center + new Vector3(size / 2, size / 2, size / 2)) });
        ThreadPool.QueueUserWorkItem(CheckNodesToEditThreaded, new object[] { voxelEditRequestBatch });
    }
    //Create the octree and all in another thread
    public void CreateOctreeThreaded(object state)
    {
        Dictionary<OctreeNode, int> localNodeIndexPointers = new Dictionary<OctreeNode, int>();
        List<OctreeNode> nodesToProcess = new List<OctreeNode>();
        List<OctreeNode> newNodes = new List<OctreeNode>();
        Dictionary<OctreeNode, OctreeNodeChildrenCarrier> localChildrenCarriers = new Dictionary<OctreeNode, OctreeNodeChildrenCarrier>();
        Vector3 cameraPos = (Vector3)(((object[])state)[0]);
        //Setup root octree
        float reducingFactor = ((float)(VoxelWorld.resolution - 1) / (float)(VoxelWorld.resolution));
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
            if (Vector3.Distance(cameraPos, octreeParentNode.chunkPosition + new Vector3(octreeParentNode.chunkSize / 2f, octreeParentNode.chunkSize / 2f, octreeParentNode.chunkSize / 2f)) < (octreeParentNode.chunkSize + voxelWorld.LODBias) && octreeParentNode.hierarchyIndex < maxHierarchyIndex)
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

                            octreeChild.hierarchyIndex = octreeParentNode.hierarchyIndex + 1;
                            octreeChild.isLeaf = true;
                            childrenPointers[childrenIndex] = newNodes.Count;
                            localNodeIndexPointers.Add(octreeChild, newNodes.Count);
                            nodesToProcess.Add(octreeChild);
                            newNodes.Add(octreeChild);
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
        HashSet<OctreeNode> addedOctreeNodesHashset = new HashSet<OctreeNode>(newNodes);
        HashSet<OctreeNode> removedOctreeNodesHashset = new HashSet<OctreeNode>(nodes);

        HashSet<OctreeNode> difference = new HashSet<OctreeNode>(newNodes);
        difference.SymmetricExceptWith(nodes);
        addedOctreeNodesHashset.IntersectWith(difference);
        removedOctreeNodesHashset.IntersectWith(difference);

        if (toAdd.Count == 0 && toRemove.Count == 0)
        {
            toRemove.Clear();
            toAdd.Clear();
            toAdd.AddRange(addedOctreeNodesHashset);
            toAdd = toAdd.OrderByDescending(x => x.hierarchyIndex).ToList();
            toRemove.AddRange(removedOctreeNodesHashset);
            nodesChildrenCarrier = localChildrenCarriers;
            nodes = newNodes;//Update octree
        }
    }
    //Request intersection test
    public bool NodeIntersectWithBounds(OctreeNode node, VoxelAABBBound bounds)
    {
        return (node.chunkPosition.x <= bounds.max.x && node.chunkPosition.x + node.chunkSize >= bounds.min.x) &&
               (node.chunkPosition.y <= bounds.max.y && node.chunkPosition.y + node.chunkSize >= bounds.min.y) &&
               (node.chunkPosition.z <= bounds.max.z && node.chunkPosition.z + node.chunkSize >= bounds.min.z);
    }
    //Checks what nodes we need to edit in another thread
    public void CheckNodesToEditThreaded(object state)
    {
        VoxelEditRequestBatch voxelEditRequestBatch = (VoxelEditRequestBatch)((object[])state)[0];
        for (int b = 0; b < voxelEditRequestBatch.voxelEditRequests.Count; b++)
        {
            List<OctreeNode> nodesToProcess = new List<OctreeNode>();
            nodesToProcess.Add(nodes[0]);
            for (int i = 0; i < nodesToProcess.Count; i++)
            {
                OctreeNode octreeParentNode = nodesToProcess[i];
                //Create octree children 

                //Create children
                int childrenIndex = 0;
                if (nodesChildrenCarrier.ContainsKey(octreeParentNode))
                {
                    int[] children = nodesChildrenCarrier[octreeParentNode].children;
                    //Check if the edit is inside/intersecting with the node AABB
                    if (NodeIntersectWithBounds(octreeParentNode, voxelEditRequestBatch.voxelEditRequests[b].bound) && octreeParentNode.hierarchyIndex < maxHierarchyIndex)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                for (int z = 0; z < 2; z++)
                                {
                                    //Setup Octree child
                                    OctreeNode octreeChild = nodes[children[childrenIndex]];
                                    if (NodeIntersectWithBounds(octreeChild, voxelEditRequestBatch.voxelEditRequests[b].bound) && octreeChild.hierarchyIndex > octreeParentNode.hierarchyIndex)
                                    {
                                        nodesToProcess.Add(octreeChild);
                                        if (voxelWorld.chunks.ContainsKey(octreeChild) && octreeChild.isLeaf)
                                        {
                                            Chunk chunk = voxelWorld.chunks[octreeChild];
                                            //Avoid duplicates
                                            if (!voxelWorld.chunkUpdateRequests.ContainsKey(octreeChild))
                                            {
                                                //world.GenerateMesh(chunk.position, chunk, true, resolution, 1);
                                                voxelWorld.chunksUpdating.Add(chunk);
                                                voxelWorld.chunkUpdateRequests.Add(octreeChild, new ChunkUpdateRequest { chunk = chunk, priority = octreeChild.hierarchyIndex });
                                            }
                                        }
                                    }
                                    childrenIndex++;
                                }
                            }
                        }
                    }
                }
                //Just in case
                if (i > 100000)
                {
                    Debug.LogError("Octree just gave up... R.I.P Octree you will be missed");
                    break;
                }
            }
        }
    }
}
//A singular octree node
public struct OctreeNode
{
    public int hierarchyIndex, size;
    public Vector3Int position;
    public Vector3 chunkPosition;
    public float chunkSize;
    public bool isLeaf;
}