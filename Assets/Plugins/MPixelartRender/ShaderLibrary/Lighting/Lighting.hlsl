#ifndef M_LIGHTING_INCLUDED
#define M_LIGHTING_INCLUDED

TEXTURE2D(_MLightingTexture);
SAMPLER(sampler_MLightingTexture);

float4 _ObstacleParams;

float3 SampleLight(float2 screenUV)
{
    int2 chunkIndex = _ObstacleParams.yz - int2(1, 1);
    float2 positionWS = chunkIndex * 16;
    float4 posCS = TransformWorldToHClip(float3(positionWS, 0.0));
    float4 scrPos = ComputeScreenPos(posCS);
    float2 originUV = scrPos.xy / scrPos.w;

    float2 chunkSize = float2(256, 256) * 3 / _ScreenParams.xy;
    float2 sampleUV = (screenUV - originUV) / chunkSize;

    //return float3(sampleUV, 0);
    return SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV).rgb;
}

#endif
