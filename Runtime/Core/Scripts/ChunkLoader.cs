using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ChunkLoader : MonoBehaviour
{
    //How many chunks we should load in the x y z axis
    private VoxelWorld voxelWorld;
    // Start is called before the first frame update
    void Start()
    {
        voxelWorld = FindObjectOfType<VoxelWorld>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 20 == 0 && !voxelWorld.generating)
        {
            voxelWorld.octree.UpdateOctree(new TerrainUtility.CameraData() { position = transform.position, forwardVector = transform.forward });
        }
    }    
}
