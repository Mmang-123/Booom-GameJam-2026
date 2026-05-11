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
TEXTURE2D(_ObstacleSDF_9); SAMPLER(sampler_ObstacleSDF_9);

#ifdef CHUNKRANGE_15
TEXTURE2D(_ObstacleSDF_10); SAMPLER(sampler_ObstacleSDF_10);
TEXTURE2D(_ObstacleSDF_11); SAMPLER(sampler_ObstacleSDF_11);
TEXTURE2D(_ObstacleSDF_12); SAMPLER(sampler_ObstacleSDF_12);
TEXTURE2D(_ObstacleSDF_13); SAMPLER(sampler_ObstacleSDF_13);
TEXTURE2D(_ObstacleSDF_14); SAMPLER(sampler_ObstacleSDF_14);
TEXTURE2D(_ObstacleSDF_15); SAMPLER(sampler_ObstacleSDF_15);
#endif

TEXTURE2D(_ObstacleMask); SAMPLER(sampler_ObstacleMask);

#ifndef M_OBSTACLE_PARAMS_INCLUDED
#define M_OBSTACLE_PARAMS_INCLUDED
float4 _ObstacleParams;
float4 _ObstacleChunkParams;
float2 _ChunkRange;
#endif

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
    return positionIndex.y * _ChunkRange.x + positionIndex.x;
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

    #ifdef CHUNKRANGE_15
    if (index == 9)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_4, sampler_ObstacleSDF_9, uv).r;
    if (index == 10)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_5, sampler_ObstacleSDF_10, uv).r;
    if (index == 11)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_6, sampler_ObstacleSDF_11, uv).r;
    if (index == 12)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_7, sampler_ObstacleSDF_12, uv).r;
    if (index == 13)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_8, sampler_ObstacleSDF_13, uv).r;
    if (index == 14)
        return SAMPLE_TEXTURE2D(_ObstacleSDF_8, sampler_ObstacleSDF_14, uv).r;
    #endif
    return 0;
}

float GetObstacleSDF(float2 screenUV)
{
    float2 centerUV = GetChunkCenterScreenUV();
    float2 chunkSize = GetChunkScreenUVSize();
    
    float2 offset = screenUV - centerUV;
    int2 offsetIndex = int2(floor(offset.x / chunkSize.x), floor(offset.y / chunkSize.y));

    float2 sampleUV = (offset - offsetIndex * chunkSize) / chunkSize;

    offsetIndex += int2((_ChunkRange.x - 1) / 2, (_ChunkRange.y - 1) / 2);
    return SampleObstacleSDF(GetChunkIndex(offsetIndex), sampleUV);
}

float GetObstacleSDF_RawCamera(float2 uv)
{
    /*
    uv = clamp(uv - float2(0.125, 0.125), 0.0, 0.75) * 4.0 / 3.0;
    int2 offsetIndex = int2(floor(uv.x * 3), floor(uv.y * 3));
    float2 sampleUV = (uv - offsetIndex * float2(0.333333, 0.333333)) / float2(0.333333, 0.333333);
    */

    if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0)
        return 0.0;

    // 3x3
    // int2 offsetIndex = int2(floor(uv.x * 3), floor(uv.y * 3));
    // float2 sampleUV = (uv - offsetIndex * float2(0.333333, 0.333333)) / float2(0.333333, 0.333333);

    float xRange = _ChunkRange.x;
    int2 offsetIndex = int2(floor(uv.x * _ChunkRange.x), floor(uv.y * _ChunkRange.y));
    float2 sampleUV = (uv - offsetIndex / _ChunkRange) * _ChunkRange;

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
    // 按256单元 ~ 256 * 3换算
    //return rawSDF * 0.35355; // 这里好像算错了？？？

    //
    return rawSDF * 1.414213 / _ChunkRange.x;
}

float GetObstacleMask(float2 screenUV)
{
    float2 centerUV = GetChunkCenterScreenUV();
    float2 chunkSize = GetChunkScreenUVSize();
    float2 totalChunkSize = chunkSize * (_ChunkRange + float2(1, 1));

    float2 offset = screenUV - centerUV;
    offset += chunkSize * float2((_ChunkRange.x - 1.0) / 2.0 + 0.5, (_ChunkRange.y - 1.0) / 2.0 + 0.5);

    float2 sampleUV = offset / totalChunkSize;
    return SAMPLE_TEXTURE2D(_ObstacleMask, sampler_ObstacleMask, sampleUV).r;
}

float GetObstacleMask_RawCamera(float2 uv)
{
    //uv = uv * 3 / 4 + 0.125;
    
    uv = (uv - 0.5) * _ChunkRange / (_ChunkRange + 1.0) + 0.5;
    
    return SAMPLE_TEXTURE2D(_ObstacleMask, sampler_ObstacleMask, uv).r;
}


#endif