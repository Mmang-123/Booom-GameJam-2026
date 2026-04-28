#include "Generic/PixelartStructures.hlsl"
#include "Generic/PixelartShared.hlsl"
#include "Generic/PixelartVertPass.hlsl"


BufferOutput PixelartFrag(Varyings input) : SV_TARGET
{
    BUFFER_OUTPUT_INIT();

    half2 baseMapUV = TRANSFORM_TEX(input.uv, _BaseMap);
    half3 baseMapColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseMapUV).rgb;
    half3 albedoColor = baseMapColor * _BaseColor.rgb;

    //
    OUTPUT_DEPTHNORMAL(input.positionCS.z, input.normalWS);
    OUTPUT_ALBEDO3(albedoColor);
    OUTPUT_SMOOTHNESS_METALLIC(_Smoothness, _Metallic);
    //OUTPUT_NORMAL(input.normalWS);
    OUTPUT_ORIGIN_UV(input.originUV);
    OUTPUT_PROPERTIES(_LUTIndex, _Outline, SHADOWTYPE_NORMAL);

    //
    RETURN_BUFFER_VALUE();
}