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


half GetShadow(float2 screenUV, float2 lightUV, int steps)
{
    half shadowMask = 1.0;
    float2 rayVectorUV = lightUV - screenUV;
    float2 stepDelta = rayVectorUV / steps;
    
    float noise = frac(sin(dot(screenUV, float2(12.9898, 78.233))) * 43758.5453);

    [loop]
    for(int j = 0; j < steps; j++)
    {
        float2 sampleUV = screenUV + stepDelta * j;

        half obstacle = PB_GetObstacleMask(sampleUV);
        
        if(obstacle > 0.1) 
        {
            shadowMask = 0.0;
            break;
        }
    }
    
    return shadowMask;
}

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

half4 LightingFrag(Varyings input) : SV_Target
{
    GET_BLIT_UV();

    float3 positionWS = GetWorldPositionWithRawDepth(uv, 0);

    half3 totalLight = half3(0.0, 0.0, 0.0);

    /*
    [loop]
    for(int i = 0; i < _MLightCount; i++)
    {
        LightData2D light = _MLightDataBuffer[i];
        
        float2 lightPos = light.position.xy;
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
        //float shadow = GetShadow(uv, lightUV, 32);
        float shadow = GetShadowDDA(uv, lightUV);

        // 累加当前光源的贡献
        totalLight += lightColor * intensity * atten * shadow;
    }
    */

    float sdf = GetObstacleSDF(uv);
    return float4(sdf, 0, 0, 1);

    return float4(totalLight, 1);
}