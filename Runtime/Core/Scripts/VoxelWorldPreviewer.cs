using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Jedjoud.VoxelWorld.VoxelUtility;
using static Jedjoud.VoxelWorld.VoxelWorld;
using static Unity.Mathematics.math;

namespace Jedjoud.VoxelWorld 
{
    /// <summary>
    /// A previewer that shows a texture of the density map with a specific offset
    /// </summary>
    public class VoxelWorldPreviewer : MonoBehaviour
    {
        public Vector3 previewOffset;
        public Vector3 previewScale = Vector3.one; 
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
