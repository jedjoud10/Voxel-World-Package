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
        voxelWorld.camData = new VoxelUtility.CameraData() { position = transform.position, forwardVector = transform.forward };
    }    
}
