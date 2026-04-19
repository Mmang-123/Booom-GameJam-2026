#define _WITH_SMOOTHNESS
#include "QuadPass.hlsl"

sampler2D _Palette;
sampler2D _ObjectLightTexture;

float4 GrassFrag(QuadVaryings input) : SV_TARGET
{
    float2 uv = input.uv;
    float shape = tex2D(_BaseMap, uv).a;
    clip(shape - 0.5);

    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;

    ZONE_CULL(screenUV, input.positionWS.z);

    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    Light mainLight = GetMainLight(shadowCoord);
    mainLight.distanceAttenuation = 1.0;
    mainLight = ApplyToonCloudShadow(mainLight, input.positionWS);

    float3 outputColor = float3(0.0, 0.0, 0.0);

//
#if defined(_SHADING_TOON)
    float shadow = mainLight.shadowAttenuation;
    float halfDiffuse = (dot(mainLight.direction, input.normalWS) + 1) / 2;
    float3 leafColor = SamplePaletteColor(_Palette, halfDiffuse, 1);
    //outputColor += leafColor + inputData.bakedGI;
#else
    float3 leafColor = SamplePaletteColor(_Palette, 0.98, 1);

#endif
    //return float4(input.normalWS, 1);
    //return float4(leafColor, 1);

    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = leafColor;
    surfaceData.specular = 0;
    surfaceData.metallic = 0;
    surfaceData.smoothness = _Smoothness;
    surfaceData.normalTS = half3(0, 0, 0);
    surfaceData.emission = 0;
    surfaceData.occlusion = 1;
    surfaceData.alpha = 1;
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 1;

    InputData inputData = (InputData)0;
    inputData.shadowCoord = shadowCoord;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = input.normalWS;
    inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV.xy, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(input.vertexSH,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        input.positionCS.xy,
        1,
        inputData.shadowMask);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#endif

    half3 bakedGI = inputData.bakedGI;
//

#if defined(_SHADING_TOON)
    outputColor += bakedGI;
#else
    
    BRDFData brdfData;
    //InitializeBRDFData(baseColor, metallic, specular, smoothness, alpha, brdfData);
    InitializeBRDFData(surfaceData, brdfData);
    //InitializeBRDFData(baseMapColor * _BaseColor.rgb, 0.1, 0, 0.5, alpha, brdfData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);

    //half3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);


    half3 giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);

    outputColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, mainLight,
                                           input.normalWS, inputData.viewDirectionWS,
                                           surfaceData.clearCoatMask, true);

    uint lightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightCount)
        Light light = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
        outputColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                               input.normalWS, inputData.viewDirectionWS,
                                               surfaceData.clearCoatMask, true);
    LIGHT_LOOP_END

    //return float4(inputData.bakedGI.rgb, 1);
    outputColor += giColor;

#endif


#ifdef _PIXELART
    float3 objectLight = tex2D(_ObjectLightTexture, screenUV).rgb;
    outputColor += objectLight;
#endif

    return float4(outputColor, 1.0);
}