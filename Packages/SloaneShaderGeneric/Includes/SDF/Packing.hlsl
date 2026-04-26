#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

float UnpackSDF(float3 packedSDF, float factor = 1)
{
    float sign = packedSDF.z > 0.5 ? 1 : -1;
    float magnitude = UnpackFloatFromR8G8(packedSDF.xy);
    return sign * magnitude * factor;
}

float3 PackSDF(float sdf, float factor = 1)
{
    float sign = sdf >= 0 ? 1 : -1;
    float magnitude = abs(sdf) / factor;
    float2 packedMagnitude = PackFloatToR8G8(magnitude);
    return float3(packedMagnitude, sign > 0 ? 1 : 0);
}