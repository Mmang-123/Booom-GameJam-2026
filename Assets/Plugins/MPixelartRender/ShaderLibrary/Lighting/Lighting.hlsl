#ifndef M_LIGHTING_INCLUDED
#define M_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

struct LightData2D
{
    float4 position; 
    float4 color;
    float4 lightParams1;
    float3 lightParams2;
};

StructuredBuffer<LightData2D> _MLightDataBuffer;
int3 _MLightParams;


TEXTURE2D(_MLightingTexture);
SAMPLER(sampler_MLightingTexture);

#ifndef M_OBSTACLE_PARAMS_INCLUDED
#define M_OBSTACLE_PARAMS_INCLUDED
float4 _ObstacleParams;
float4 _ObstacleChunkParams;
float2 _ChunkRange;
#endif

float4 _Resolution;


float3 SampleLight(float2 screenUV)
{
    // _ObstacleParams.yz 是CenterChunk的左下角
    int2 chunkIndex = _ObstacleParams.yz - ((_ChunkRange - 1) / 2);
    float2 positionWS = chunkIndex * 16;
    float4 posCS = TransformWorldToHClip(float3(positionWS, 0.0));
    float4 scrPos = ComputeScreenPos(posCS);
    float2 originUV = scrPos.xy / scrPos.w;


    float2 chunkSize = _ObstacleChunkParams.xy * _ChunkRange.xy / _Resolution.zw;
    float2 sampleUV = (screenUV - originUV) / chunkSize;

    //return float3(sampleUV, 0);
    return SRGBToLinear(SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV).rgb);
}

void ComputePointLight(int lightIndex, float2 positionWS, out half3 outColor)
{
    outColor = 0.0;
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = light.position.xy;
    float radius = light.position.w;
    
    half3 lightColor = light.color.rgb;
    half intensity = light.color.w;

    // 
    float dist = distance(positionWS.xy, lightPos);
    if (dist > radius)
        return;

    // 衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation;

    // Step
    float3 s = saturate(intensity * distanceAttenuation);
    s = round(s * 2.0) / 2.0;
    s *= s;

    outColor = lightColor * s;
}

void ComputeSpotLight(int lightIndex, float2 positionWS, out half3 outColor)
{
    outColor = 0.0;
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = light.position.xy;
    float radius = light.position.w;
    float cullRadius = light.lightParams2.x;
    
    half3 lightColor = light.color.rgb;
    half intensity = light.color.w;

    float2 lightDir = light.lightParams1.xy;
    float2 scaleOffset = light.lightParams1.zw;

    //
    float height = dot(positionWS - lightPos, lightDir);

    // 
    float dist = distance(positionWS.xy, lightPos);
    if (dist > radius || height < cullRadius)
        return;

    // 距离衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation * (3.0 - 2.0 * distanceAttenuation);

    // 角度衰减
    float2 direction = normalize(positionWS.xy - lightPos.xy);
    float SdotL = dot(lightDir, direction); 
    float angleAttenuation = saturate(SdotL * scaleOffset.x + scaleOffset.y);
    angleAttenuation *= angleAttenuation; // 边缘平滑

    // Step
    float s = angleAttenuation;
    s = round(s * 2.0) / 2.0;

    if (s <= 0)
    {
        return;
    }

    outColor = distanceAttenuation * s * intensity * lightColor;
}

void ComputeAreaLight(int lightIndex, float2 positionWS, out half3 outColor)
{
    outColor = 0.0;
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = light.position.xy;
    float radius = light.position.w;
    
    half3 lightColor = light.color.rgb;
    half intensity = light.color.w;

    float2 lightDir = light.lightParams1.xy;
    float2 point1 = light.lightParams1.zw;
    float2 point2 = light.lightParams2.xy;
    float innerScale = min(0.99, light.lightParams2.z);

    // 
    float dist = distance(positionWS.xy, lightPos);
    if (dist > radius)
        return;

    float2 v = point2 - point1;
    float2 w = positionWS - point1;
    float vSq = dot(v, v);

    // 算出进度比例 t
    float t = dot(w, v) / vSq;

    if (t < 0.0 || t > 1.0 || dot(lightDir, normalize(w)) < 0.0)
    {
        return;
    }

    // 距离衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation * (3.0 - 2.0 * distanceAttenuation);

    // 边缘衰减
    float edgeDis = (abs(t - 0.5) * 2.0);
    float edgeT = saturate(edgeDis - innerScale) / (1 - innerScale);
    float edgeAttenuation = (1.0 - edgeT);

    outColor = distanceAttenuation * edgeAttenuation * intensity * lightColor;
}




#endif
