#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../Pixelart/Generic/PixelartShared.hlsl"
#include "../Pixelart/Generic/PixelartBuffer.hlsl"
#include "ObstacleShared.hlsl"

struct LightData2D
{
    float4 position; 
    float4 color;
    float4 lightParams1;
    float3 lightParams2;
};

StructuredBuffer<LightData2D> _MLightDataBuffer;
int3 _MLightParams;

float4 _ObstacleChunkParams;

// 3x3的uv在4x4的位置
inline float2 UV3To4(float2 uv)
{
    return uv * 3 / 4 + 0.125;
}

inline float2 UV4To3(float2 uv)
{
    return clamp(uv - float2(0.125, 0.125), 0.0, 0.75) * 4.0 / 3.0;
}

half GetShadow(float2 screenUV, float2 lightUV)
{
    /*
    screenUV.y *= _ObstacleChunkParams.y / _ObstacleChunkParams.x;
    lightUV.y *= _ObstacleChunkParams.y / _ObstacleChunkParams.x;
    */
    float2 direction = normalize(lightUV - screenUV);

    const int MAX_STEPS = 128;
    
    float2 current = screenUV;
    float unitSize = 1 / _ObstacleChunkParams.z;
    half shadowMask = 1.0;

    [loop]
    for (int i = 0; i <= MAX_STEPS; i++)
    {
        half obstacleMask = GetObstacleMask_RawCamera(current);
        if (obstacleMask > 0.1)
        {
            shadowMask = 0.0;
            break;
        }

        float dist = distance(current, lightUV);
        float sdf = GetObstacleSDF_RawCamera(current);
        float nextStep = UnpackSDFToRaw(sdf) * 0.9;
        if (nextStep <= 4.0 * unitSize)
        {
            nextStep = min(unitSize, nextStep);

            // 采样两个分量
            half obstacleMaskX = GetObstacleMask_RawCamera(current + float2(sign(direction.x) * unitSize, 0.0));
            half obstacleMaskY = GetObstacleMask_RawCamera(current + float2(0.0, sign(direction.y) * unitSize));
            if (obstacleMaskX + obstacleMaskY > 0.1)
            {
                shadowMask = 0.0;
                break;
            }
        }

        if (dist < 0.01 || dist <= nextStep)
        {
            shadowMask = 1.0;
            break;
        }

        current += direction * nextStep;
    }

    return shadowMask;
}

half3 ComputePointLight(int lightIndex, float2 positionWS, float2 uv)
{
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    //float2 lightPos = SnapWorldPosition(float3(light.position.xy, 0));
    float2 lightPos = float3(light.position.xy, 0);
    float radius = light.position.w;
    
    half3 lightColor = light.color.rgb;
    half intensity = light.color.w;

    // 
    float dist = distance(positionWS.xy, lightPos);
    if (dist > radius)
        return 0;

    // 衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation * (3.0 - 2.0 * distanceAttenuation);

    // 阴影
    float2 lightUV = UV4To3(WorldToUV(lightPos));

    float shadow = 1.0;
    if (GetObstacleMask_RawCamera(lightUV) > 0.5)
    {
        shadow = 0.0;
    }
    else
    {
        shadow = GetShadow(uv, lightUV);
    }

    // 累加当前光源的贡献
    return lightColor * intensity * distanceAttenuation * shadow;
    //return shadow;
}

half3 ComputeSpotLight(int lightIndex, float2 positionWS, float2 uv)
{
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    //float2 lightPos = SnapWorldPosition(float3(light.position.xy, 0));
    float2 lightPos = float3(light.position.xy, 0);
    float radius = light.position.w;
    
    half3 lightColor = light.color.rgb;
    half intensity = light.color.w;

    float2 lightDir = light.lightParams1.xy;
    float2 scaleOffset = light.lightParams1.zw;

    // 
    float dist = distance(positionWS.xy, lightPos);
    if (dist > radius)
        return 0;

    // 距离衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation * (3.0 - 2.0 * distanceAttenuation);

    // 角度衰减
    float2 lightUV = UV4To3(WorldToUV(lightPos));
    float2 direction = normalize(uv - lightUV);
    float SdotL = dot(lightDir, direction); 
    float angleAttenuation = saturate(SdotL * scaleOffset.x + scaleOffset.y);
    angleAttenuation *= angleAttenuation; // 边缘平滑

    if (angleAttenuation <= 0)
    {
        return 0.0;
    }

    // 阴影
    float shadow = 1.0;
    if (GetObstacleMask_RawCamera(lightUV) > 0.5)
    {
        shadow = 0.0;
    }
    else
    {
        shadow = GetShadow(uv, lightUV);
    }

    //return half3(lightDir, 0);
    return distanceAttenuation * angleAttenuation * shadow * intensity * lightColor;
}

half3 ComputeAreaLight(int lightIndex, float2 positionWS, float2 uv)
{
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = float3(light.position.xy, 0);
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
        return 0;

    float2 v = point2 - point1;
    float2 w = positionWS - point1;
    float vSq = dot(v, v);

    // 算出进度比例 t
    float t = dot(w, v) / vSq;

    if (t < 0.0 || t > 1.0 || dot(lightDir, normalize(w)) < 0.0)
    {
        return 0;
    }

    // 阴影
    float2 targetPoint = lerp(point1, point2, t);
    float2 lightUV = UV4To3(WorldToUV(targetPoint));
    float shadow = 1.0;
    if (GetObstacleMask_RawCamera(lightUV) > 0.5)
    {
        shadow = 0.0;
    }
    else
    {
        shadow = GetShadow(uv, lightUV);
    }

    // 距离衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation * (3.0 - 2.0 * distanceAttenuation);

    // 边缘衰减
    float edgeDis = (abs(t - 0.5) * 2.0);
    float edgeT = saturate(edgeDis - innerScale) / (1 - innerScale);
    float edgeAttenuation = (1.0 - edgeT);

    return distanceAttenuation * edgeAttenuation * intensity * lightColor * shadow;
}

half4 LightingFrag(Varyings input) : SV_Target
{
    GET_BLIT_UV();
    //float3 originVSOffset = ComputeOriginVSOffset();

    //float3 positionWS = GetWorldPositionWithRawDepth(uv, 0);
    float2 positionWS = UVToWorld(UV3To4(uv));
    //float3 positionWS = SnapWorldPosition(GetWorldPositionWithRawDepth(uv, 0).xyz);
    uv = UV4To3(WorldToUV(positionWS));

    half3 totalLight = half3(0.0, 0.0, 0.0);

    int lightCount = _MLightParams.x;
    int pointLightCount = _MLightParams.y;
    int spotLightCount = _MLightParams.z;

    // Point Light
    int start = 0;
    int end = pointLightCount;
    for(int i = start; i < end; i++)
    {
        totalLight += ComputePointLight(i, positionWS, uv);
    }

    // Spot Light
    start += pointLightCount;
    end += spotLightCount;
    for(int i = start; i < end; i++)
    {
        totalLight += ComputeSpotLight(i, positionWS, uv);
    }

    // Area Light
    start += spotLightCount;
    end = lightCount;
    for(int i = start; i < end; i++)
    {
        totalLight += ComputeAreaLight(i, positionWS, uv);
    }

    return float4(totalLight, 1);
}