//FbM functions
float fbmSnoise(float3 p, float pe, float la, float o)
{
    float noiseDensity = 0;
    float maxNoiseDensity = 0;
    for (int i = 0; i < o; i++)
    {
        noiseDensity += (snoise((p)*pow(la, i)) * pow(pe, i));
        maxNoiseDensity += pow(pe, i);
    }
    return noiseDensity / maxNoiseDensity;
}

