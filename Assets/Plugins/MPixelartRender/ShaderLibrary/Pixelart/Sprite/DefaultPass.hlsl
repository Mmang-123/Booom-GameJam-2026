//#include "../Generic/PixelartStructures.hlsl"
#include "../../Lighting/Lighting.hlsl"
#include "Background.hlsl"
#include "SpriteShading.hlsl"

Varyings UnlitVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    const float UNIT_SIZE = 16.0 / 256.0;

    o.positionWS = TransformObjectToWorld(v.positionOS);
    o.positionOS = v.positionOS.xy;

    o.positionCS = TransformWorldToHClip(float4(o.positionWS, 1));

#ifdef TEXTURE_BASED
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#else
    o.uv = v.uv;
#endif
    o.color = v.color * _Color * _RendererColor;
    return o;
}

float3 ComputeLighting(float2 positionWS)
{
    int lightCount = _MLightParams.x;
    int pointLightCount = _MLightParams.y;
    int spotLightCount = _MLightParams.z;

    float3 totalLight = 0;

    // Point Light
    int start = 0;
    int end = pointLightCount;

    half3 lightColor = 0.0;
    half lightShadow = 0.0;
    for(int i = start; i < end; i++)
    {
        ComputePointLight(i, positionWS, lightColor);
        totalLight += lightColor;
    }

    // Spot Light
    start += pointLightCount;
    end += spotLightCount;
    for(int i = start; i < end; i++)
    {
        ComputeSpotLight(i, positionWS, lightColor);
        totalLight += lightColor;
    }

    // Area Light
    start += spotLightCount;
    end = lightCount;
    for(int i = start; i < end; i++)
    {
        ComputeAreaLight(i, positionWS, lightColor);
        totalLight += lightColor;
    }

    return totalLight;
}



float4 UnlitFrag(Varyings input) : SV_Target
{
#ifdef TEXTURE_BASED
    float4 outputColor = tex2D(_MainTex, input.uv) * input.color;
#else
    float4 outputColor = input.color;
#endif

    clip(outputColor.a - 0.1);

    const float UNIT_SIZE = 16.0 / 256.0;
    float2 positionOSSnapped = floor(input.positionOS / UNIT_SIZE) * UNIT_SIZE;
    float2 positionWS = TransformObjectToWorld(float4(positionOSSnapped, 0, 1));
    positionWS = floor(positionWS / UNIT_SIZE) * UNIT_SIZE;

    float4 positionCS = TransformWorldToHClip(float4(positionWS, 0, 1));
    float4 screenPos = ComputeScreenPos(positionCS);
    float2 screenUV = screenPos.xy / screenPos.w;

    float3 lightColor = ComputeLighting(positionWS);

    if ((lightColor.r <= 0.001 && lightColor.g <= 0.001 && lightColor.b <= 0.001) || (outputColor.r <= 0.001 && outputColor.g <= 0.001 && outputColor.b <= 0.001))
    {
        // Background
        outputColor.rgb = LightenBlend(SampleBackground(screenUV).rgb, lightColor);
    }
    else
    {
        outputColor.rgb = lerp(LightenBlend(outputColor.rgb, lightColor, _LightenBlend), outputColor.rgb * lightColor, _ShadingBlend);
    }

    outputColor.rgb = lerp(outputColor.rgb, _Emission.rgb, _Emission.a);

    // return float4(lightColor, outputColor.a);
    return outputColor;
}