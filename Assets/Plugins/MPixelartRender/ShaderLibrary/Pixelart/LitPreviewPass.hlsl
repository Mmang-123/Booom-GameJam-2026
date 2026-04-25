#include "Generic/Lighting.hlsl"
#include "Generic/PixelartStructures.hlsl"
#include "Generic/PixelartShared.hlsl"
#include "Generic/PixelartShading.hlsl"

Varyings PreviewVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(float4(output.positionWS, 1.0));

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.normalWS = half3(normalInput.normalWS);
#if defined(_NORMALMAP)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif

    output.uv = input.uv;
    output.color = input.color;

    return output;
}

half4 PreviewFrag(Varyings input) : SV_TARGET
{
    half2 baseMapUV = TRANSFORM_TEX(input.uv, _BaseMap);
    half3 baseMapColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseMapUV).rgb;
    half3 albedoColor = baseMapColor * _BaseColor.rgb;

    float3 outputColor = 0;

    // Main Light
    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    Light mainLight = GetMainLight(shadowCoord);
    outputColor += MainLightShading(mainLight, input.normalWS, input.positionWS, _Metallic,  _LUTIndex);

    // Albedo
    outputColor *= albedoColor;

    //
    //output.albedo = float4(albedoColor, 1);
    //output.normal = normalize(input.normalWS) * 0.5 + 0.5;
    //output.lutIndex = _LUTIndex * 1.0 / 1024.0;

    // 直接返回颜色
    return half4(albedoColor, 1);

    //return half4(outputColor, 1);
}