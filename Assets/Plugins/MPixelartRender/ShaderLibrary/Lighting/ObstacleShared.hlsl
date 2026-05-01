#ifndef OBSTACLE_SHARED_INCLUDED
#define OBSTACLE_SHARED_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_ObstacleSDF_0); SAMPLER(sampler_ObstacleSDF_0);
TEXTURE2D(_ObstacleSDF_1); SAMPLER(sampler_ObstacleSDF_1);
TEXTURE2D(_ObstacleSDF_2); SAMPLER(sampler_ObstacleSDF_2);
TEXTURE2D(_ObstacleSDF_3); SAMPLER(sampler_ObstacleSDF_3);
TEXTURE2D(_ObstacleSDF_4); SAMPLER(sampler_ObstacleSDF_4);
TEXTURE2D(_ObstacleSDF_5); SAMPLER(sampler_ObstacleSDF_5);
TEXTURE2D(_ObstacleSDF_6); SAMPLER(sampler_ObstacleSDF_6);
TEXTURE2D(_ObstacleSDF_7); SAMPLER(sampler_ObstacleSDF_7);
TEXTURE2D(_ObstacleSDF_8); SAMPLER(sampler_ObstacleSDF_8);

TEXTURE2D(_ObstacleMask); SAMPLER(sampler_ObstacleMask);

float4 _ObstacleParams;

float2 WorldToUV(float2 posWS)
{
    float4 posCS = TransformWorldToHClip(float3(posWS, 0.0));
    float4 scrPos = ComputeScreenPos(posCS);
    float2 uv = scrPos.xy / scrPos.w;

    return uv;
}

float2 UVToWorld(float2 uv)
{
    float2 ndc = uv * 2.0 - 1.0;
    
    float2 worldPosXY = _WorldSpaceCameraPos.xy + (ndc * unity_OrthoParams.xy);

    return worldPosXY;
}

inline int GetChunkIndex(int2 positionIndex)
{
    return positionIndex.y * 3 + positionIndex.x;
}

//
inline float2 GetChunkCenterScreenUV()
{
    int2 chunkIndex = _ObstacleParams.yz;
    float2 positionWS = chunkIndex * 16;
    return WorldToUV(positionWS);
}

inline float2 GetChunkScreenUVSize()
{
    return float2(256, 256) / _ScreenParams.xy;
}

float SampleObstacleSDF(int index, float2 uv)
{
    if (index == 0)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_0, sampler_ObstacleSDF_0, uv).r;
    if (index == 1)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_1, sampler_ObstacleSDF_1, uv).r;
    if (index == 2)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_2, sampler_ObstacleSDF_2, uv).r;
    if (index == 3)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_3, sampler_ObstacleSDF_3, uv).r;
    if (index == 4)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_4, sampler_ObstacleSDF_4, uv).r;
    if (index == 5)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_5, sampler_ObstacleSDF_5, uv).r;
    if (index == 6)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_6, sampler_ObstacleSDF_6, uv).r;
    if (index == 7)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_7, sampler_ObstacleSDF_7, uv).r;
    if (index == 8)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_8, sampler_ObstacleSDF_8, uv).r;
    return 0;
}

float GetObstacleSDF(float2 screenUV)
{
    float2 centerUV = GetChunkCenterScreenUV();
    float2 chunkSize = GetChunkScreenUVSize();
    
    float2 offset = screenUV - centerUV;
    int2 offsetIndex = int2(floor(offset.x / chunkSize.x), floor(offset.y / chunkSize.y));

    float2 sampleUV = (offset - offsetIndex * chunkSize) / chunkSize;

    offsetIndex += int2(1, 1);
    return SampleObstacleSDF(GetChunkIndex(offsetIndex), sampleUV);
}

float GetObstacleSDF_RawCamera(float2 uv)
{
    /*
    uv = clamp(uv - float2(0.125, 0.125), 0.0, 0.75) * 4.0 / 3.0;
    int2 offsetIndex = int2(floor(uv.x * 3), floor(uv.y * 3));
    float2 sampleUV = (uv - offsetIndex * float2(0.333333, 0.333333)) / float2(0.333333, 0.333333);
    */
    int2 offsetIndex = int2(floor(uv.x * 3), floor(uv.y * 3));
    float2 sampleUV = (uv - offsetIndex * float2(0.333333, 0.333333)) / float2(0.333333, 0.333333);

    //offsetIndex += int2(1, 1);
    return SampleObstacleSDF(GetChunkIndex(offsetIndex), sampleUV);
}

float2 UnpackSDFToScreen(float rawSDF)
{
    // 按256x256单元 ~ 480x270屏幕大小计算
    return rawSDF * float2(0.75424, 1.34088); 
}

float UnpackSDFToRaw(float rawSDF)
{
    // TODO 按256单元 ~ 256 * 3换算
    return rawSDF * 0.35355;
}

float GetObstacleMask(float2 screenUV)
{
    float2 centerUV = GetChunkCenterScreenUV();
    float2 chunkSize = GetChunkScreenUVSize();
    float2 totalChunkSize = chunkSize * 4;

    float2 offset = screenUV - centerUV;
    offset += chunkSize * 1.5;

    float2 sampleUV = offset / totalChunkSize;
    return SAMPLE_TEXTURE2D(_ObstacleMask, sampler_ObstacleMask, sampleUV).r;
}

float GetObstacleMask_RawCamera(float2 uv)
{
    uv = uv * 3 / 4 + 0.125;
    return SAMPLE_TEXTURE2D(_ObstacleMask, sampler_ObstacleMask, uv).r;
}


#endif