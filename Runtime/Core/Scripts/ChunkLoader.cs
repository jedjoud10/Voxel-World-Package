using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ChunkLoader : MonoBehaviour
{
    //How many chunks we should load in the x y z axis
    private VoxelWorld voxelWorld;
    private Vector3 lastPosition = Vector3.positiveInfinity;
    // Start is called before the first frame update
    void Start()
    {
        voxelWorld = FindObjectOfType<VoxelWorld>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 20 == 0 && (Vector3.Distance(lastPosition, transform.position) > 1f || Time.frameCount < 21))
        {
            voxelWorld.octree.UpdateOctree(transform.position);
            lastPosition = transform.position;
        }
    }    
}
