#ifndef CLOUD_TEXTURE_INCLUDED
#define CLOUD_TEXTURE_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_CloudTexture);
SAMPLER(sampler_CloudTexture);

float3 _CenterFocusPosition;
float _CloudSize;

float GetRawCloudShadow(float3 lightDirection, float3 positionWS)
{
    float3 projectionPosition = dot(float3(0, 0, 0) - positionWS, float3(0, 1, 0)) / dot(lightDirection, float3(0, 1, 0)) * lightDirection + positionWS;
    float2 pos = projectionPosition.xz;
    pos.x -= _CenterFocusPosition.x;
    pos.y -= _CenterFocusPosition.z;
    pos /= _CloudSize;
    pos += float2(0.5, 0.5);

    float cloudShadow = SAMPLE_TEXTURE2D(_CloudTexture, sampler_CloudTexture, pos).x;
    return cloudShadow;
}

float GetToonCloudShadow(float3 lightDirection, float3 positionWS)
{
    float rawShadow = GetRawCloudShadow(lightDirection, positionWS);
    float toonShadow = lerp(0.25, lerp(0.5, 1, step(0.7, rawShadow)), step(0.5, rawShadow));
    return toonShadow;
}

Light ApplyRawCloudShadow(Light light, float3 positionWS)
{
    float cloudShadow = GetRawCloudShadow(light.direction, positionWS);
    light.shadowAttenuation *= cloudShadow;
    return light;
}

Light ApplyToonCloudShadow(Light light, float3 positionWS)
{
    float cloudShadow = GetToonCloudShadow(light.direction, positionWS);
    light.shadowAttenuation *= cloudShadow;
    return light;
}

#endif // CLOUD_TEXTURE_INCLUDED