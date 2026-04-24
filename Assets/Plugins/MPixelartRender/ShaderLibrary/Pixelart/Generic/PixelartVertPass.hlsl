#include "Generic/PixelartStructures.hlsl"
#include "Generic/PixelartShared.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"


Varyings PixelartVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    //float3 originWS = unity_ObjectToWorld._14_24_34;
    float3 originVS = mul(PIXELART_CAMERA_MATRIX_V, float4(originWS, 1.0)).xyz;
    float3 originVSSnapped = float3(UNITSNAP(originVS.x, _UnitSize), UNITSNAP(originVS.y, _UnitSize), originVS.z);
    float3 originVSOffset = originVSSnapped - originVS;

    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS, 1.0)).xyz + originVSOffset;
    output.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));

    // origin UV
    float4 originCS = mul(GetViewToHClipMatrix(), float4(originVSSnapped, 1.0));
    output.originUV = ((originCS.xy / originCS.w) + 1) / 2;
#if UNITY_UV_STARTS_AT_TOP
    output.originUV.y = 1.0 - output.originUV.y;
#endif

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.normalWS = half3(normalInput.normalWS);
#if defined(_NORMALMAP)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif


    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#if defined(DYNAMICLIGHTMAP_ON)
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    half4 probeOcclusion;
    OUTPUT_SH4(output.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(output.positionWS), output.vertexSH, probeOcclusion);


    output.uv = input.uv;
    output.color = input.color;

    return output;
}