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
    public class VoxelWorldPreviewer : BaseVoxelComponent
    {
        //Main preview vars
        public Vector3 previewOffset;
        public Vector3 previewScale = Vector3.one;
        public float previewIsolevel;
        public RenderTexture previewTexture;

        /// <summary>
        /// Initialize this voxel previewer
        /// </summary>
        public override void Setup(VoxelWorld voxelWorld)
        {
            base.Setup(voxelWorld);
            previewTexture = new RenderTexture(VoxelWorld.resolution, VoxelWorld.resolution, 8);
            previewTexture.enableRandomWrite = true;
            previewTexture.Create();
            //Gotta do this to avoid errors
            voxelWorld.chunkManager.generationShader.SetTexture(0, "previewTexture", previewTexture);
            voxelWorld.chunkManager.generationShader.SetTexture(1, "previewTexture", previewTexture);

            voxelWorld.chunkManager.generationShader.SetTexture(2, "previewTexture", previewTexture);
            voxelWorld.chunkManager.generationShader.SetInt("resolution", resolution);
        }

        /// <summary>
        /// Create the texture using the current generation compute shader
        /// </summary>
        public void UpdateTexture() 
        {
            voxelWorld.chunkManager.generationShader.SetVector("scale", previewScale);
            voxelWorld.chunkManager.generationShader.SetVector("generationOffset", previewOffset);
            voxelWorld.chunkManager.generationShader.SetFloat("isolevel", previewIsolevel);
            voxelWorld.chunkManager.generationShader.Dispatch(2, resolution / 8, 1, resolution / 8);
        }

        /// <summary>
        /// Release the preview texture
        /// </summary>
        public override void Release() 
        {
            previewTexture.Release();
        }
    }
}
