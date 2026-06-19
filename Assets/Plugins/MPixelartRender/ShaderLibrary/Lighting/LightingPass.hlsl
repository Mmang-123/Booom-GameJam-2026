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

// 3x3的uv在4x4的位置
inline float2 UV3To4(float2 uv)
{
    //return uv * 3.0 / 4.0 + 0.125;
    return (uv - 0.5) * _ChunkRange / (_ChunkRange + 1.0) + 0.5;
}

inline float2 UV4To3(float2 uv)
{
    //return (uv - 0.5) * 4.0 / 3.0 + 0.5;
    return (uv - 0.5) * (_ChunkRange + 1.0) / _ChunkRange + 0.5;
}

inline float2 SnapLightPoisition(float2 rawPosition)
{
    return rawPosition;
    //const float UNIT_SIZE = 16.0 / 256.0;
    //return round(rawPosition / UNIT_SIZE) * UNIT_SIZE;
}

inline float2 ScaleUV(float2 screenUV)
{
    //return screenUV;
    screenUV.y *= _ChunkRange.y / _ChunkRange.x;
    return screenUV;
}

inline float2 UnscaleUV(float2 screenUV)
{
    //return screenUV;
    screenUV.y *= _ChunkRange.x / _ChunkRange.y;
    return screenUV;
}

half GetShadow(float2 screenUV, float2 lightUV, float innerRadius, float maskThreshold)
{
    float2 direction = normalize(lightUV - screenUV);

    const int MAX_STEPS = 160
    
    float2 current = screenUV;
    //float unitSize = 1.0 / _ObstacleChunkParams.z;
    float unitSize = 1.0 / (_ObstacleChunkParams.x * (_ChunkRange.x + 1));
    half shadowMask = 1.0;

    [loop]
    for (int i = 0; i <= MAX_STEPS; i++)
    {
        float dist = distance(current, lightUV);
        if (dist <= innerRadius)
        {
            shadowMask = 1.0;
            break;
        }

        half obstacleMask = GetObstacleMask_RawCamera(UnscaleUV(current));
        if (obstacleMask > maskThreshold)
        {
            shadowMask = 0.0;
            break;
        }

        float sdf = GetObstacleSDF_RawCamera(UnscaleUV(current));
        float nextStep = obstacleMask < 0.5 ? max(UnpackSDFToRaw(sdf) * 0.75, unitSize * 2.0) : UnpackSDFToRaw(sdf) * 0.9;
        /* if (nextStep <= 4.0 * unitSize)
        {
            // 切换到DDA精确网格遍历
            float2 invAbsDir = 1.0 / max(abs(direction), 1e-6);
            float2 tDelta = unitSize * invAbsDir;
            int2 cellPos = int2(floor(current / unitSize));
            int2 stepDirDDA = int2(sign(direction));
            float2 tMax;
            tMax.x = direction.x >= 0
                ? ((cellPos.x + 1) * unitSize - current.x) * invAbsDir.x
                : (current.x - cellPos.x * unitSize) * invAbsDir.x;
            tMax.y = direction.y >= 0
                ? ((cellPos.y + 1) * unitSize - current.y) * invAbsDir.y
                : (current.y - cellPos.y * unitSize) * invAbsDir.y;

            float ddaMaxDist = dist - innerRadius;
            float cornerEps = min(tDelta.x, tDelta.y) * 0.01;

            // ddaStatus: 0=未决（步数耗尽），1=到达光源，-1=被遮挡
            int ddaStatus = 0;
            float ddaLastT = 0.0;

            [loop]
            for (int j = 0; j < 128; j++)
            {
                float t;
                if (abs(tMax.x - tMax.y) <= cornerEps)
                {
                    // 射线过网格角点：同时步进两轴
                    t = tMax.x;
                    if (t >= ddaMaxDist) { ddaStatus = 1; break; }

                    int2 cellX = int2(cellPos.x + stepDirDDA.x, cellPos.y);
                    int2 cellY = int2(cellPos.x, cellPos.y + stepDirDDA.y);
                    cellPos += stepDirDDA;
                    tMax += tDelta;

                    // 只有两侧格子都是障碍才遮挡（90度实心夹角）
                    half maskX = GetObstacleMask_RawCamera(UnscaleUV((float2(cellX) + 0.5) * unitSize));
                    half maskY = GetObstacleMask_RawCamera(UnscaleUV((float2(cellY) + 0.5) * unitSize));
                    if (maskX > maskThreshold && maskY > maskThreshold)
                        { shadowMask = 0.0; ddaStatus = -1; break; }

                    half ddaMask = GetObstacleMask_RawCamera(UnscaleUV((float2(cellPos) + 0.5) * unitSize));
                    if (ddaMask > maskThreshold)
                        { shadowMask = 0.0; ddaStatus = -1; break; }
                }
                else if (tMax.x < tMax.y)
                {
                    t = tMax.x;
                    if (t >= ddaMaxDist) { ddaStatus = 1; break; }
                    cellPos.x += stepDirDDA.x;
                    tMax.x += tDelta.x;

                    half ddaMask = GetObstacleMask_RawCamera(UnscaleUV((float2(cellPos) + 0.5) * unitSize));
                    if (ddaMask > maskThreshold)
                        { shadowMask = 0.0; ddaStatus = -1; break; }
                }
                else
                {
                    t = tMax.y;
                    if (t >= ddaMaxDist) { ddaStatus = 1; break; }
                    cellPos.y += stepDirDDA.y;
                    tMax.y += tDelta.y;

                    half ddaMask = GetObstacleMask_RawCamera(UnscaleUV((float2(cellPos) + 0.5) * unitSize));
                    if (ddaMask > maskThreshold)
                        { shadowMask = 0.0; ddaStatus = -1; break; }
                }

                ddaLastT = t;
            }

            if (ddaStatus != 0)
            {
                // DDA已得出结论（到达光源或被遮挡），退出外层循环
                break;
            }

            // 步数耗尽但未得出结论：推进current，继续SDF march
            current += direction * max(ddaLastT, unitSize);
            continue;
        } */

        if (dist <= nextStep)
        {
            shadowMask = 1.0;
            break;
        }

        current += direction * nextStep;
    }

    return shadowMask;
}

void ComputePointLight(int lightIndex, float2 positionWS, float2 uv, float mask, out half3 outColor, out half outShadow)
{
    const float UNIT_SIZE = 16.0 / (256.0 * _ChunkRange.x);
    outShadow = 0.0;
    outColor = 0.0;
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = SnapLightPoisition(light.position.xy);
    float innerRadius = light.position.z;
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

    // 阴影
    float2 lightUV = ScaleUV(UV4To3(WorldToUV(lightPos)));

    float shadowThreshold;
    if (mask > 0.01 && mask < 0.6)
        shadowThreshold = 0.6;
    else
        shadowThreshold = 0.01;
    float shadow = GetShadow(uv, lightUV, innerRadius * UNIT_SIZE, shadowThreshold);


    // Step
    float3 s = saturate(intensity * distanceAttenuation);
    s = round(s * 2.0) / 2.0;
    s *= s;

    outShadow = shadow;
    outColor = lightColor * s;
}

void ComputeSpotLight(int lightIndex, float2 positionWS, float2 uv, float mask, out half3 outColor, out half outShadow)
{
    const float UNIT_SIZE = 16.0 / (256.0 * _ChunkRange.x);
    outShadow = 0.0;
    outColor = 0.0;
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = SnapLightPoisition(light.position.xy);
    float innerRadius = light.position.z;
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
    float2 lightUV = ScaleUV(UV4To3(WorldToUV(lightPos)));
    float2 direction = normalize(uv - lightUV);
    float SdotL = dot(lightDir, direction); 
    float angleAttenuation = saturate(SdotL * scaleOffset.x + scaleOffset.y);
    angleAttenuation *= angleAttenuation; // 边缘平滑

    if (angleAttenuation <= 0)
    {
        return;
    }

    // 阴影
    float shadowThreshold;
    if (mask > 0.01 && mask < 0.6)
        shadowThreshold = 0.6;
    else
        shadowThreshold = 0.01;
    float shadow = GetShadow(uv, lightUV, innerRadius * UNIT_SIZE, shadowThreshold);


    // Step
    float s = angleAttenuation;
    s = round(s * 2.0) / 2.0;
    //s *= s * s;

    outShadow = shadow;
    outColor = intensity * distanceAttenuation * s * lightColor;
}

void ComputeAreaLight(int lightIndex, float2 positionWS, float2 uv, float mask, out half3 outColor, out half outShadow)
{
    outShadow = 0.0;
    outColor = 0.0;
    LightData2D light = _MLightDataBuffer[lightIndex];
        
    float2 lightPos = SnapLightPoisition(light.position.xy);
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

    // 阴影
    float shadowThreshold;
    if (mask > 0.01 && mask < 0.6)
        shadowThreshold = 0.6;
    else
        shadowThreshold = 0.01;

    float2 targetPoint = lerp(point1, point2, t);
    float2 lightUV = ScaleUV(UV4To3(WorldToUV(targetPoint)));
    float shadow = 1.0;

    shadow = GetShadow(uv, lightUV, 0.01, shadowThreshold);
    

    // 距离衰减
    float distanceAttenuation = saturate(1.0 - (dist / radius));
    distanceAttenuation *= distanceAttenuation * (3.0 - 2.0 * distanceAttenuation);

    // 边缘衰减
    float edgeDis = (abs(t - 0.5) * 2.0);
    float edgeT = saturate(edgeDis - innerScale) / (1 - innerScale);
    float edgeAttenuation = (1.0 - edgeT);

    outShadow = shadow;
    outColor = distanceAttenuation * edgeAttenuation * intensity * lightColor;
}

// A + B - A * B
#define SCREEN_BLEND(a, b) ((a) + (b) - (a) * (b))

half4 LightingFrag(Varyings input) : SV_Target
{
    GET_BLIT_UV();

    float2 positionWS = UVToWorld(UV3To4(uv));
    uv = UV4To3(WorldToUV(positionWS));
    uv = ScaleUV(uv);

    //return half4(UVToWorld(0.5).xxx, 1);
    //return half4(positionWS.xxx, 1);

    half3 totalLight = half3(0.0, 0.0, 0.0);
    half3 outputLight = half3(0.0, 0.0, 0.0);

    int lightCount = _MLightParams.x;
    int pointLightCount = _MLightParams.y;
    int spotLightCount = _MLightParams.z;

    //
    float mask = GetObstacleMask_RawCamera(UnscaleUV(uv));
    //return half4(mask.xxx, 1);

    // Point Light
    int start = 0;
    int end = pointLightCount;

    half3 lightColor = 0.0;
    half lightShadow = 0.0;
    half lightShadow2 = 0.0;
    for(int i = start; i < end; i++)
    {
        ComputePointLight(i, positionWS, uv, mask, lightColor, lightShadow);
        half3 litPoint = lightColor * lightShadow;
        totalLight = SCREEN_BLEND(totalLight, litPoint);
        if (mask >= 0.9)
            outputLight = SCREEN_BLEND(outputLight, lightColor);
        else
            outputLight = SCREEN_BLEND(outputLight, litPoint);
    }

    // Spot Light
    start += pointLightCount;
    end += spotLightCount;
    for(int i = start; i < end; i++)
    {
        ComputeSpotLight(i, positionWS, uv, mask, lightColor, lightShadow);
        half3 litSpot = lightColor * lightShadow;
        totalLight = SCREEN_BLEND(totalLight, litSpot);
        if (mask >= 0.9)
            outputLight = SCREEN_BLEND(outputLight, lightColor);
        else
            outputLight = SCREEN_BLEND(outputLight, litSpot);
    }

    // Area Light
    start += spotLightCount;
    end = lightCount;
    for(int i = start; i < end; i++)
    {
        ComputeAreaLight(i, positionWS, uv, mask, lightColor, lightShadow);
        half3 litArea = lightColor * lightShadow;
        totalLight = SCREEN_BLEND(totalLight, litArea);
        if (mask >= 0.9)
            outputLight = SCREEN_BLEND(outputLight, lightColor);
        else
            outputLight = SCREEN_BLEND(outputLight, litArea);
    }

    // 简易算一下光照强度
    //float s = (totalLight.r + totalLight.g + totalLight.b) / 3.0;
    float s = (outputLight.r + outputLight.g + outputLight.b) / 3.0;
    //float s = max(outputLight.r, max(outputLight.g, outputLight.b));

    //if (mask > 0.01 && mask < 0.6)
    //    return half4(1, 0, 0, 1);

    return float4(outputLight, s);
}