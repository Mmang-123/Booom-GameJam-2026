#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../Pixelart/Generic/PixelartShared.hlsl"
#include "../Pixelart/Generic/PixelartBuffer.hlsl"
#include "ObstacleShared.hlsl"

struct LightData2D
{
    float4 position; 
    float4 color;
};

StructuredBuffer<LightData2D> _MLightDataBuffer;
int _MLightCount;

half GetShadowDDA(float2 screenUV, float2 lightUV)
{
    float2 startPixel = screenUV * _ScreenParams.xy;
    float2 endPixel = lightUV * _ScreenParams.xy;
    
    float2 delta = endPixel - startPixel;
    float steps = max(abs(delta.x), abs(delta.y));
    
    if (steps < 0.5) return 1.0;
    
    float2 stepInc = delta / steps;
    float2 currentPixel = startPixel;
    half shadowMask = 1.0;
    
    //
    float2 invScreenParams = 1.0 / _ScreenParams.xy; 
    
    const int MAX_STEPS = 320; 
    
    [loop]
    for(int i = 0; i <= MAX_STEPS; i++)
    {
        if (i > steps) break; 
        
        currentPixel += stepInc;
        
        float2 sampleUV = (floor(currentPixel) + 0.5) * invScreenParams;
        
        half obstacle = PB_GetObstacleMask(sampleUV);
        
        if(obstacle > 0.1) 
        {
            shadowMask = 0.0;
            break; 
        }
    }
    
    return shadowMask;
}

float2 NormalizeUV(float2 uv)
{
    uv.y *= _ScreenParams.x / _ScreenParams.y;
    return uv;
}

half GetShadow(float2 screenUV, float2 lightUV)
{
    screenUV.y *= _ScreenParams.y / _ScreenParams.x;
    lightUV.y *= _ScreenParams.y / _ScreenParams.x;
    float2 direction = normalize(lightUV - screenUV);

    const int MAX_STEPS = 64;
    
    float2 current = screenUV;
    float unitSize = 1 / _ScreenParams.x;
    half shadowMask = 1.0;

    for (int i = 0; i <= MAX_STEPS; i++)
    {
        half obstacleMask = GetObstacleMask(NormalizeUV(current));
        if (obstacleMask > 0.1)
        {
            shadowMask = 0.0;
            break;
        }

        float dist = distance(current, lightUV);
        float sdf = GetObstacleSDF(NormalizeUV(current));
        float nextStep = UnpackSDF(sdf).x * 0.9;
        if (nextStep <= 4.0 * unitSize)
        {
            nextStep = min(unitSize, nextStep);

            // 采样两个分量
            half obstacleMaskX = GetObstacleMask(NormalizeUV(current + float2(sign(direction.x) * unitSize, 0.0)));
            half obstacleMaskY = GetObstacleMask(NormalizeUV(current + float2(0.0, sign(direction.y) * unitSize)));
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

half4 LightingFrag(Varyings input) : SV_Target
{
    GET_BLIT_UV();
    float3 originVSOffset = ComputeOriginVSOffset();

    float3 positionWS = SnapWorldPosition(GetWorldPositionWithRawDepth(uv, 0).xyz, originVSOffset);

    half3 totalLight = half3(0.0, 0.0, 0.0);

    [loop]
    for(int i = 0; i < _MLightCount; i++)
    {
        LightData2D light = _MLightDataBuffer[i];
        
        float2 lightPos = SnapWorldPosition(float3(light.position.xy, 0), originVSOffset);
        float radius = light.position.w;
        
        half3 lightColor = light.color.rgb;
        half intensity = light.color.w;

        // 
        float dist = distance(positionWS.xy, lightPos);
        if (dist > radius)
            continue;

        // 衰减
        float atten = saturate(1.0 - (dist * dist) / (radius * radius));

        // 阴影
        float2 lightUV = WorldToUV(lightPos);
        float shadow = 1.0;
        if (GetObstacleMask(lightUV) > 0.5)
        {
            shadow = 0.0;
        }
        else
        {
            shadow = GetShadow(uv, lightUV);
        }

        // 累加当前光源的贡献
        //totalLight += lightColor * intensity * atten * shadow;
        totalLight += shadow;
        //totalLight += 1;
    }

    return float4(totalLight, 1);
}