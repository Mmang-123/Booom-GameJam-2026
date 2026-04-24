#ifndef PIXELART_BUFFER_INCLUDED
#define PIXELART_BUFFER_INCLUDED
#include "PixelartShared.hlsl"

TEXTURE2D(_DepthBuffer);
TEXTURE2D(_DepthNormalBuffer);
TEXTURE2D(_NormalBuffer);
TEXTURE2D(_OriginUVBuffer);
TEXTURE2D(_AlbedoBuffer);
TEXTURE2D(_SmoothnessMetallicBuffer);
TEXTURE2D(_PropertiesBuffer);
SAMPLER(sampler_DepthBuffer);
SAMPLER(sampler_DepthNormalBuffer);
SAMPLER(sampler_NormalBuffer);
SAMPLER(sampler_OriginUVBuffer);
SAMPLER(sampler_AlbedoBuffer);
SAMPLER(sampler_SmoothnessMetallicBuffer);
SAMPLER(sampler_PropertiesBuffer);

inline float PB_GetDepth(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_DepthBuffer, sampler_DepthBuffer, screenUV).r;
}

inline float PB_GetDepth_NoReversed(float2 screenUV)
{
    float raw = SAMPLE_TEXTURE2D(_DepthBuffer, sampler_DepthBuffer, screenUV).r;
#ifdef UNITY_REVERSED_Z
    return 1.0 - raw;
#else
    return raw;
#endif
}

inline half4 PB_GetDepthNormal(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_DepthNormalBuffer, sampler_DepthNormalBuffer, screenUV).rgba;
}

inline float PB_GetDepth2(float2 screenUV)
{
    half4 depthNormal = PB_GetDepthNormal(screenUV);
    return DecodeDepth(depthNormal);
}

inline float PB_GetDepth2_NoReversed(float2 screenUV)
{
    half4 depthNormal = PB_GetDepthNormal(screenUV);
    float raw = DecodeDepth(depthNormal);
#ifdef UNITY_REVERSED_Z
    return 1.0 - raw;
#else
    return raw;
#endif
}

inline float3 PB_GetNormal2(float2 screenUV)
{
    half4 depthNormal = PB_GetDepthNormal(screenUV);
    return DecodeNormal(depthNormal);
}

inline float3 PB_GetAlbedo(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_AlbedoBuffer, sampler_AlbedoBuffer, screenUV).rgb;
}

inline float2 PB_GetSmoothnessMetallic(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_SmoothnessMetallicBuffer, sampler_SmoothnessMetallicBuffer, screenUV).rg;
}

inline float3 PB_GetNormal(float2 screenUV)
{
    float3 __raw_normal = SAMPLE_TEXTURE2D(_NormalBuffer, sampler_NormalBuffer, screenUV).rgb;
    return (__raw_normal - 0.5) * 2.0;
}

inline float2 PB_GetOriginUV(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_OriginUVBuffer, sampler_OriginUVBuffer, screenUV).rg;
}

inline half4 PB_GetProperties(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_PropertiesBuffer, sampler_PropertiesBuffer, screenUV).rgba;
}

inline uint PB_GetLUTIndex(float2 screenUV)
{
    float property = PB_GetProperties(screenUV).x;
    return DecodeLUTIndex(property);
}

inline int PB_GetOutline(float2 screenUV)
{
    half propertyG = PB_GetProperties(screenUV).y;
    PixelartPropertiesG g = DecodePropertiesG(propertyG);
    return g.outline;
}

inline int PB_GetShadowType(float2 screenUV)
{
    half propertyG = PB_GetProperties(screenUV).y;
    PixelartPropertiesG g = DecodePropertiesG(propertyG);
    return g.shadowType;
}


#define GET_DEPTH(screenUV, outDepth) \
float outDepth = PB_GetDepth(screenUV); \

#define GET_DEPTH2(screenUV, outDepth) \
float outDepth = PB_GetDepth2(screenUV); \

#define GET_ALBEDO(screenUV, outAlbedo) \
float3 outAlbedo = PB_GetAlbedo(screenUV); \

#define GET_SMOOTHNESS_METALLIC(screenUV, outSmoothness, outMetallic) \
float2 __raw_SmoothnessMetallic = PB_GetSmoothnessMetallic(screenUV); \
float outSmoothness = __raw_SmoothnessMetallic.x; \
float outMetallic = __raw_SmoothnessMetallic.y; \

#define GET_NORMAL(screenUV, outNormal) \
float3 outNormal = PB_GetNormal(screenUV); \

#define GET_NORMAL2(screenUV, outNormal) \
float3 outNormal = PB_GetNormal2(screenUV); \

#define GET_ORIGIN_UV(screenUV, outOriginUV) \
float2 outOriginUV = PB_GetOriginUV(screenUV); \

#define GET_PROPERTIES(screenUV, outIndex, outOutline, outShadowType) \
half4 __raw_properties = PB_GetProperties(screenUV); \
uint outIndex = DecodeLUTIndex(__raw_properties.x); \
PixelartPropertiesG __raw_propertiesG = DecodePropertiesG(__raw_properties.y); \
int outOutline = __raw_propertiesG.outline; \
int outShadowType = __raw_propertiesG.shadowType; \

#define GET_LUTINDEX(screenUV, outIndex) \
uint outIndex = PB_GetLUTIndex(screenUV); \

#define GET_OUTLINE(screenUV, outOutline) \
int outOutline = PB_GetOutline(screenUV); \

#define GET_SHADOW_TYPE(screenUV, outShadowType) \
int outShadowType = PB_GetShadowType(screenUV); \

#define GET_POSITION(screenUV, rawDepth, outPositionWS, outPositionCS) \
float3 outPositionWS = GetWorldPositionWithRawDepth(screenUV, rawDepth); \
float4 outPositionCS = mul(PIXELART_CAMERA_MATRIX_VP, float4(outPositionWS, 1.0)); \

//
#endif // PIXELART_BUFFER_INCLUDED