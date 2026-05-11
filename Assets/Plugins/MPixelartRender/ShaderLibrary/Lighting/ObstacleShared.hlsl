#ifndef OBSTACLE_SHARED_INCLUDED
#define OBSTACLE_SHARED_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D_ARRAY(_ObstacleSDF); SAMPLER(sampler_ObstacleSDF);

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

// _ObstacleParams.yz = origin in chunk units, may be fractional for even N
// origin = CenterIndex - (N-1)/2.0  (float division)
inline float2 GetChunkCenterScreenUV()
{
    float2 originWS = _ObstacleParams.yz * 16.0;
    return WorldToUV(originWS);
}

inline float2 GetChunkScreenUVSize()
{
    return float2(256, 256) / _ScreenParams.xy;
}

float SampleObstacleSDF(int index, float2 uv)
{
    return SAMPLE_TEXTURE2D_ARRAY(_ObstacleSDF, sampler_ObstacleSDF, uv, (float)index).r;
}

float GetObstacleSDF(float2 screenUV)
{
    float2 originUV = GetChunkCenterScreenUV();
    float2 chunkSize = GetChunkScreenUVSize();
    
    float2 offset = screenUV - originUV;
    int2 offsetIndex = int2(floor(offset.x / chunkSize.x), floor(offset.y / chunkSize.y));

    float2 sampleUV = (offset - offsetIndex * chunkSize) / chunkSize;

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
    float2 originUV = GetChunkCenterScreenUV();
    float2 chunkSize = GetChunkScreenUVSize();
    float2 totalChunkSize = chunkSize * (_ChunkRange + float2(1, 1));

    float2 offset = screenUV - originUV;
    offset += chunkSize * float2(0.5, 0.5);

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