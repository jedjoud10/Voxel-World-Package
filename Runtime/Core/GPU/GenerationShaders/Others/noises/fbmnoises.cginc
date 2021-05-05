//FbM functions
float fBmSnoise(float3 p, float pe, float la, float o)
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
float fBmRidge(float3 p, float pe, float la, float o)
{
    float noiseDensity = 0;
    float maxNoiseDensity = 0;
    for (int i = 0; i < o; i++)
    {
        float n = 1 - abs(snoise((p)*pow(la, i)));
        n = pow(n, 1.3);
        noiseDensity += (1 - n) * pow(pe, i);
        maxNoiseDensity += pow(pe, i);
    }
    return noiseDensity / maxNoiseDensity;
}
float fBmCellular(float3 p, float pe, float la, float o)
{
    float noiseDensity = 0;
    float maxNoiseDensity = 0;
    for (int i = 0; i < o; i++)
    {
        noiseDensity += ((1 - cellular(p * pow(la, i))) * pow(pe, i));
        maxNoiseDensity += pow(pe, i);
    }
    return noiseDensity / maxNoiseDensity;
}
