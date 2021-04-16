#include "./Others/SDFFunctions.cginc"
#include "./Others/noises/base.cginc"
#include "./Others/noises/cellular3D.cginc"
#include "./Others/noises/noise3D.cginc"
//Example generator
float Density(float3 p, float3 lp)
{
    float noiseDensity = 0;
    float maxNoiseDensity = 0;
    for (int i = 0; i < 3; i++)
    {
        noiseDensity += abs(snoise((p * 0.002 + 5) * pow(2, i) * float3(1, 0, 1) ) * pow(0.5, i));
        maxNoiseDensity += pow(0.5, i);
    }
    noiseDensity /= maxNoiseDensity;
    return p.y + noiseDensity * 100;
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