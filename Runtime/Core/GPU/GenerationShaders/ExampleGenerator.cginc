#include "./Others/SDFFunctions.cginc"
#include "./Others/noises/base.cginc"
#include "./Others/noises/cellular3D.cginc"
#include "./Others/noises/noise3D.cginc"
//Example generator
float Density(float3 p, float3 lp)
{
    return p.y + snoise(p * 0.01) * 50;
}
//The color function
float3 Color(float3 p, float3 lp, float3 n)
{
    float3 color = n;
    return color;
}
//The metallic function
float Metallic(float3 p, float3 lp, float3 n)
{
    return 0;
}
//The Smoothness function
float Smoothness(float3 p, float3 lp, float3 n)
{
    return 0;
}