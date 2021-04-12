
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