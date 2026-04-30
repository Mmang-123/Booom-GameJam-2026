#ifndef SPRITE_SHADING_INCLUDED
#define SPRITE_SHADING_INCLUDED


inline float3 MixAlbedoAndLightColor_Background(float3 albedo, float3 lightColor)
{
    return albedo + lightColor;
}

#endif