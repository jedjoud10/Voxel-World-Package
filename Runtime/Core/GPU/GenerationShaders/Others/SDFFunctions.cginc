//SDF Function from Inigo Quilez https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
//SDF Functions
float sdBox(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}
float sdSphere(float3 p, float r) { return length(p) - r; }
float sdCappedCylinder(float3 p, float h, float r)
{
    float2 d = abs(float2(length(p.xz), p.y)) - float2(h, r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}
float sdCircle(in float2 p, in float r)
{
    return length(p) - r;
}
float sdTriPrism(float3 p, float2 h)
{
    float3 q = abs(p);
    return max(q.z - h.y, max(q.x * 0.866025 + p.y * 0.5, -p.y) - h.x * 0.5);
}
float ndot(float2 a, float2 b) { return a.x * b.x - a.y * b.y; }

float sdRhombus(in float2 p, in float2 b)
{
    float2 q = abs(p);

    float h = clamp((-2.0 * ndot(q, b) + ndot(b, b)) / dot(b, b), -1.0, 1.0);
    float d = length(q - 0.5 * b * float2(1.0 - h, 1.0 + h));
    d *= sign(q.x * b.y + q.y * b.x - b.x * b.y);

    return d;
}

// sca is the sin/cos of the orientation
// scb is the sin/cos of the aperture
float sdArc(in float2 p, in float2 sca, in float2 scb, in float ra, in float rb)
{
    p = mul(p, float2x2(sca.x, sca.y, -sca.y, sca.x));
    p.x = abs(p.x);
    float k = (scb.y * p.x > scb.x * p.y) ? dot(p.xy, scb) : length(p.xy);
    return sqrt(dot(p, p) + ra * ra - 2.0 * ra * k) - rb;
}

#define PI 3.1415926
float sdHeart(in float2 p, in float radius) {
    float offset = 3.0 - 2.0 * sqrt(2.0);
    float extra = 0.05;
    float2 center = float2(0.0, offset + extra);
    float r = 1.0 - center.y;

    // Construct the heart in normalized coordinates where radius of inner circle is 1.0
    float2 _p = (p / radius) * r + center;

    float br = sqrt(2.0) / 2.0;
    float d = sdRhombus(_p, 1.0);
    float dc1 = sdCircle(_p - float2(0.5, 0.5), br);
    float dc2 = sdCircle(_p - float2(-0.5, 0.5), br);

    /*
        if(dc1 < 0.0 && d < 0.0)
            d = min(d, -sdArc(_p-vec2(0.5, 0.5), vec2(sin(3.0*PI/4.0), cos(3.0*PI/4.0)), vec2(sin(PI/2.0), cos(PI/2.0)), br, 0.0));
        else
            d = min(d, dc1);

        if(dc2 < 0.0 && d < 0.0)
            d = min(d, -sdArc(_p-vec2(-0.5, 0.5), vec2(sin(PI/4.0), cos(PI/4.0)), vec2(sin(PI/2.0), cos(PI/2.0)), br, 0.0));
        else
            d = min(d, dc2);
    */

    d = min(min(d, dc1), dc2);
    if (_p.y < 0.0) d += 1.5 * abs(_p.x) * abs(_p.y) * abs(_p.y) * r * r
        ; // pull the sides of the heart inward

    // Fix scaling
    return d * radius / r;
}

float opSmoothUnion(float d1, float d2, float k) {
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) - k * h * (1.0 - h);
}

float opSmoothSubtraction(float d1, float d2, float k) {
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    return lerp(d2, -d1, h) + k * h * (1.0 - h);
}

float opSmoothIntersection(float d1, float d2, float k) {
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) + k * h * (1.0 - h);
}

float opUnion(float d1, float d2) 
{
    return min(d1, d2);
}

float opSubtraction(float d1, float d2)
{
    return max(d1, -d2);
}

float opIntersection(float d1, float d2)
{
    return max(d1, d2);
}