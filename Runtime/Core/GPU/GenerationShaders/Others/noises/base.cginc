
float3 mod289(float3 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float4 mod289(float4 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float4 permute(float4 x) {
    return mod289(((x * 34.0) + 1.0) * x);
}
// Permutation polynomial: (34x^2 + x) mod 289
float3 permute(float3 x) {
    return mod289((34.0 * x + 1.0) * x);
}
// Modulo 7 without a division
float3 mod7(float3 x) {
    return x - floor(x * (1.0 / 7.0)) * 7.0;
}

#include "./GenerationShaders/Others/SDFFunctions.cginc"
#include "./GenerationShaders/Others/noises/cellular3D.cginc"
#include "./GenerationShaders/Others/noises/noise3D.cginc"
#include "./GenerationShaders/Others/noises/erosionnoise.cginc"
#include "./GenerationShaders/Others/noises/fbmnoises.cginc"
#include "./GenerationShaders/Others/noises/hashes.cginc"


//Base values
float3 offset;
float3 scale;
float chunkScaling;
int resolution;
float quality;
float isolevel;

//Data stuff
struct Voxel
{
    float density;
    float3 color;
    float3 normal;
    float2 sm;    
};
struct VoxelDetail
{
    float3 position;
    float3 forward;
    float size;
    int type;
};
struct ColorSmoothnessMetallic
{
    float3 color;
    float2 sm;
};
RWStructuredBuffer<Voxel> voxelsBuffer;
AppendStructuredBuffer<VoxelDetail> detailsBuffer;