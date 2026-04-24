#ifndef PGENERIC_LIGHTING_INCLUDED
#define PGENERIC_LIGHTING_INCLUDED

#define ADDITIONAL_LIGHT_CALCULATE_SHADOWS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


Light GetAdditionalLightWithShadowAttenuation(uint i, float3 positionWS)
{
    half4 shadowMask = half4(1, 1, 1, 1);
    int lightIndex = i;
    Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    half4 occlusionProbeChannels = _AdditionalLightsBuffer[lightIndex].occlusionProbeChannels;
#else
    half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[lightIndex];
#endif
    //light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, positionWS, light.direction);

    light.shadowAttenuation = AdditionalLightShadow(lightIndex, positionWS, light.direction, shadowMask, occlusionProbeChannels);
#if defined(_LIGHT_COOKIES)
    real3 cookieColor = SampleAdditionalLightCookie(lightIndex, positionWS);
    light.color *= cookieColor;
#endif

    return light;
}

#endif // PGENERIC_LIGHTING_INCLUDED