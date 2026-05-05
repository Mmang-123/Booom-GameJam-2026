#include "../Generic/PixelartStructures.hlsl"
#include "../Generic/PixelartShared.hlsl"

VaryingsParticle PixelartVert(PixelartParticleInput input)
{
    VaryingsParticle output = (VaryingsParticle)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half fogFactor = 0.0;
#if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

    float3 originWS = float3(input.texcoord0.zw, input.texcoord1.x);
#ifdef _PIXELART
    float3 originVS = mul(PIXELART_CAMERA_MATRIX_V, float4(originWS, 1.0));
    float3 originVSOffset = float3(UNITSNAP(originVS.x, _UnitSize), UNITSNAP(originVS.y, _UnitSize), originVS.z) - originVS;
#endif

    // position ws is used to compute eye depth in vertFading
    output.positionWS.xyz = vertexInput.positionWS;
    output.positionWS.w = fogFactor;

#ifdef _PIXELART
    float3 positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS.xyz, 1.0)) + originVSOffset;
    //float3 positionVS = mul(UNITY_MATRIX_V, float4(output.positionWS.xyz, 1.0));
#else
    float3 positionVS = mul(UNITY_MATRIX_V, float4(output.positionWS.xyz, 1.0));
#endif
    output.clipPos = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));

    
    //output.clipPos = vertexInput.positionCS;
    output.color = GetParticleColor(input.color);

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);

#ifdef _NORMALMAP
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normalWS = half3(normalInput.normalWS);
    output.viewDirWS = viewDirWS;
#endif


    GetParticleTexcoords(output.texcoord, input.texcoord0.xy);


#if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON) || defined(_DISTORTION_ON)
    output.projectedPosition = vertexInput.positionNDC;
#endif

    return output;
}

half4 PixelartFrag(VaryingsParticle input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    ParticleParams particleParams;
    InitParticleParams(input, particleParams);

    SurfaceData surfaceData;
    InitializeSurfaceData(particleParams, surfaceData);
    InputData inputData;
    InitializeInputData(input, surfaceData, inputData);
    //SETUP_DEBUG_TEXTURE_DATA_FOR_TEX(inputData, input.texcoord, _BaseMap);

    half4 finalColor = UniversalFragmentUnlit(inputData, surfaceData);

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.clipPos);
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
        finalColor.rgb *= aoFactor.directAmbientOcclusion;
    #endif

    //finalColor.rgb = MixFog(finalColor.rgb, inputData.fogCoord);
    finalColor.a = input.color.a;

    return finalColor;
}