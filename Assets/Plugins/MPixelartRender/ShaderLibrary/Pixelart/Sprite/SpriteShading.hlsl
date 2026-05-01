#ifndef SPRITE_SHADING_INCLUDED
#define SPRITE_SHADING_INCLUDED

inline float3 LightenBlend(float3 albedo, float3 lightColor, float factor = 0.24)
{
    // lightColor = SRGBToLinear(lightColor);
    // albedo = SRGBToLinear(albedo);
    // return albedo;
    return lerp(albedo, max(albedo, lightColor), factor);
}

#endif