#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../Pixelart/Generic/PixelartShared.hlsl"

struct LightData2D {
    float4 position; 
    float4 color;
};

StructuredBuffer<LightData2D> _MLightDataBuffer;
int _MLightCount;

half4 LightingFrag(Varyings input) : SV_Target
{
    GET_BLIT_UV();

    float3 positionWS = GetWorldPositionWithRawDepth(uv, 0);

    half3 totalLight = half3(0.0, 0.0, 0.0);

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

        // 累加当前光源的贡献
        totalLight = lightColor * intensity * atten;
    }

    return float4(totalLight, 1);
}