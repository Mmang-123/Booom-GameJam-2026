#include "QuadPass.hlsl"

sampler2D _Palette;
sampler2D _PaletteDark;
sampler2D _ObjectLightTexture;

float4 LeafFrag(QuadVaryings input) : SV_TARGET
{
    float2 uv = input.uv;
    float shape = tex2D(_BaseMap, uv).a;
    clip(shape - 0.5);

    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;

    ZONE_CULL(screenUV, input.positionWS.z);

    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    Light mainLight = GetMainLight(shadowCoord);
    mainLight.distanceAttenuation = 1.0;
#if defined(_SHADING_TOON)
    mainLight = ApplyToonCloudShadow(mainLight, input.positionWS);
#endif
    float3 outputColor = float3(0.0, 0.0, 0.0);

//
#if defined(_SHADING_TOON)
    float shadow = mainLight.shadowAttenuation;
    float halfDiffuse = (dot(mainLight.direction, input.normalWS) + 1) / 2;
    float3 leafColor = input.type == 1
        ? SamplePaletteColor(_PaletteDark, halfDiffuse, 1)
        : SamplePaletteColor(_Palette, halfDiffuse, 1);
    //outputColor += leafColor + inputData.bakedGI;
#else
    float3 leafColor = input.type == 1
        ? SamplePaletteColor(_PaletteDark, 0.9, 1)
        : SamplePaletteColor(_Palette, 0.9, 1);

#endif

/*
    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = leafColor;
    surfaceData.specular = 0;
    surfaceData.metallic = 0;
    surfaceData.smoothness = 0;
    surfaceData.normalTS = half3(0, 0, 1);
    surfaceData.emission = 0;
    surfaceData.occlusion = 1;
    surfaceData.alpha = 1;
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 1;
*/  

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
    //return float4(inputData.bakedGI.rgb, 1);

    /*
    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);


    half3 giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
    */
    //return half4(input.normalWS.xyz, 1);
    //return half4(inputData.bakedGI, 1);
    half3 bakedGI = inputData.bakedGI;
    //float rawB = max(max(bakedGI.r, bakedGI.g), bakedGI.b);
    //float maxB = clamp(sqrt((clamp(rawB, 0.05, 0.7) - 0.05) * 2), 0, 0.8) * 0.5;
    half3 giColor;
    giColor = bakedGI /** (maxB / rawB)*/;
//

#if defined(_SHADING_TOON)
    outputColor += giColor;
    //outputColor += leafColor;
#else
#if defined(_BACKGROUND)
    float strength = max(max(giColor.r, giColor.g), giColor.b);
    float clampStrength = clamp(strength, 0, 0.1);
    if (strength != 0)
        giColor = giColor * clampStrength / strength;
#endif
    outputColor += giColor;
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
        float ndotl = (dot(light.direction, input.normalWS) + 1) / 2;
        ndotl *= light.distanceAttenuation;
#if defined(_BACKGROUND)
        ndotl = clamp(ndotl, 0, 0.1) * 0.5;
#else
        ndotl = saturate(ndotl) * 0.5;
#endif
        outputColor += ndotl * light.color;
    LIGHT_LOOP_END
    float mainLightNdotl = (dot(mainLight.direction, input.normalWS) + 1) / 2;
    outputColor += mainLightNdotl * mainLight.color * mainLight.shadowAttenuation;
    outputColor *= leafColor;
#endif

    

#ifdef _PIXELART
    float3 objectLight = tex2D(_ObjectLightTexture, screenUV).rgb;
    outputColor += objectLight;
#endif

    return float4(outputColor, 1.0);
}