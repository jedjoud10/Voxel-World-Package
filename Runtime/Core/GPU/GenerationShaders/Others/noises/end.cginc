
int flt(uint3 pos) { return (pos.z * resolution * resolution) + (pos.y * resolution) + pos.x; }
[numthreads(8, 8, 8)]
void VoxelMain(uint3 id : SV_DispatchThreadID)
{
    float3 p = (id * chunkScaling + offset) * scale;

    Voxel voxel;
    voxel.density = Density(p, id / (float)resolution);
    voxel.normal = 1;
    voxel.color = 1;
    voxel.sm = 0;
    voxelsBuffer[(id.z * resolution * resolution) + (id.y * resolution) + id.x] = voxel;
}

[numthreads(8, 8, 8)]
void VoxelFinal(uint3 id : SV_DispatchThreadID)
{
    float3 p = (id * chunkScaling + offset) * scale;
    int index = flt(id);
    Voxel voxel = voxelsBuffer[index];
    float3 normal = 0;
    normal.x = voxelsBuffer[flt(uint3(1, 0, 0) + id)].density - voxelsBuffer[flt(uint3(-1, 0, 0) + id)].density;
    normal.y = voxelsBuffer[flt(uint3(0, 1, 0) + id)].density - voxelsBuffer[flt(uint3(0, -1, 0) + id)].density;
    normal.z = voxelsBuffer[flt(uint3(0, 0, 1) + id)].density - voxelsBuffer[flt(uint3(0, 0, -1) + id)].density;
    normal = normalize(normal);

    voxel.normal = normal;
    ColorSmoothnessMetallic csm = GetCSM(p, id / (float)resolution, normal);
    voxel.color = csm.color;
    voxel.sm = saturate(csm.sm);
    if(id.x < resolution - 3 && id.y < resolution - 3 && id.z < resolution - 3 && quality == 1) PlaceVoxelDetail(p, id / (float)resolution, normal, voxel.density);
    voxelsBuffer[index] = voxel;
}