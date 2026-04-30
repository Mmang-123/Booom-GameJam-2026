#include "../Generic/Lighting.hlsl"
#include "../Generic/PixelartShared.hlsl"
#include "../Generic/Transform.hlsl"
#include "../Generic/PixelartBuffer.hlsl"
#include "../Generic/PixelartShading.hlsl"
#include "../Generic/Outline.hlsl"
#include "../../Lighting/Lighting.hlsl"
#include "../Sprite/Background.hlsl"
#include "../Sprite/SpriteShading.hlsl"

void ShadingFragment(Varyings input, out half4 outColor : COLOR0, out float3 outSpecular : COLOR1)
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float2 screenUV = input.texcoord;
    
    GET_DEPTH(screenUV, depth);
    GET_ALBEDO(screenUV, albedo);
    //GET_SMOOTHNESS_METALLIC(screenUV, smoothness, metallic);
    //GET_NORMAL(screenUV, normalWS);
    GET_EMISSION(screenUV, emission);
    //GET_ORIGIN_UV(screenUV, originUV);
    //GET_PROPERTIES(screenUV, lutIndex, outlineProperty, shadowTypeProperty);
    //GET_POSITION(screenUV, depth, positionWS, positionCS);
    GET_SURFACE_TYPE(screenUV, surfaceType);

    float3 outputColor = 0;

    //
    float3 light = SampleLight(screenUV);

    //outputColor += albedo * light + emission;

    if (light.r <= 0.001 && light.g <= 0.001 && light.b <= 0.001)
    {
        // Background
        outputColor = SRGBToLinear(SampleBackground(screenUV)).rgb;
    }
    else
    {
        outputColor = MixAlbedoAndLightColor_Background(albedo, light);
        // 有时候可以用emission表示在阴影中的颜色?
        if (surfaceType == 1)
        {
            // 在有光照的时候取消emission
            emission.a = 0;
        }
    }
    

    // Emission
    outputColor = lerp(outputColor, emission.rgb, emission.a);

    // Color Output
    outColor = half4(outputColor, 1);
    outSpecular = 0;
}