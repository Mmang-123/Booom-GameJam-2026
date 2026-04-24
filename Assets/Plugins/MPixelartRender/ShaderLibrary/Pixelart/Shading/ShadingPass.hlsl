#include "../Generic/Lighting.hlsl"
#include "../Generic/PixelartShared.hlsl"
#include "../Generic/Transform.hlsl"
#include "../Generic/PixelartBuffer.hlsl"
#include "../Generic/PixelartShading.hlsl"
#include "../Generic/Outline.hlsl"

void ShadingFragment(Varyings input, out half4 outColor : COLOR0, out float3 outSpecular : COLOR1)
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float2 screenUV = input.texcoord;
    
    GET_DEPTH(screenUV, depth);
    GET_ALBEDO(screenUV, albedo);
    GET_SMOOTHNESS_METALLIC(screenUV, smoothness, metallic);
    GET_NORMAL(screenUV, normalWS);
    GET_ORIGIN_UV(screenUV, originUV);
    GET_PROPERTIES(screenUV, lutIndex, outlineProperty, shadowTypeProperty);
    GET_POSITION(screenUV, depth, positionWS, positionCS);

    // 先直接输出albedo
    // Color Output
    outColor = half4(albedo, 1);
    outSpecular = 0;

    /*
    float3 outputColor = 0;
    float3 outputSpecular = 0;
    float3 viewDir = GetWorldSpaceNormalizeViewDir(positionWS);

    // Shadow Position
    float3 shadowPosition = 0;
    if (shadowTypeProperty >= 1)
    {
        float originDepth = PB_GetDepth(originUV);
        shadowPosition = GetWorldPositionWithRawDepth(originUV, originDepth);
    }
    else
    {
        shadowPosition = positionWS;
    }

    // Main Light
    float4 shadowCoord = TransformWorldToShadowCoord(shadowPosition);
    Light mainLight = GetMainLight(shadowCoord);
    if (outlineProperty >= 1)
    {
        outputColor += MainLightShading_WithOutline(mainLight, normalWS, shadowPosition, screenUV, metallic, lutIndex);
    }
    else
    {
        outputColor += MainLightShading(mainLight, normalWS, shadowPosition, metallic, lutIndex);
    }
    outputSpecular += 5 * SpecularShading(mainLight, albedo, normalWS, positionWS, viewDir, smoothness, metallic);

    // Additional Light
    LIGHT_LOOP_BEGIN(_AdditionalLightCount)
        Light light = GetAdditionalLightWithShadowAttenuation(lightIndex, shadowPosition);
        outputColor += DiffuseShading(light, normalWS);
    LIGHT_LOOP_END

    // Albedo
    outputColor *= albedo;

    outputColor += outputSpecular;

    // Color Output
    outColor = half4(outputColor, 1);
    outSpecular = outputSpecular;
    */
}