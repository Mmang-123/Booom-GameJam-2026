#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

QuadVaryings BillboardVert(uint id : SV_VERTEXID)
{
    QuadVaryings output = (QuadVaryings)0;
    VarietyGenerationPoint input = _GenerationPoints[id];

    //float3 originPositionVS = positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.originPositionWS, 1.0)) + originVSOffset;

    // 摄像机信息
    float3 upCamVec = normalize(UNITY_MATRIX_V._m10_m11_m12);
    float3 forwardCamVec = -normalize(UNITY_MATRIX_V._m20_m21_m22);
    float3 rightCamVec = normalize(UNITY_MATRIX_V._m00_m01_m02);
    //float4x4 rotationCamMatrix = float4x4(rightCamVec, 0, upCamVec, 0, forwardCamVec, 0, 0, 0, 0, 1);

    // 对齐摄像机
    float3 positionOS = input.positionOS;
    float3 rotatedPositionOS =
        + positionOS.x * rightCamVec
        + positionOS.y * upCamVec
        + positionOS.z * forwardCamVec;

    float3 allRotatedPositionWS = input.originPositionWS + rotatedPositionOS;
    output.positionWS = input.originPositionWS + float3(rotatedPositionOS.x, 0, rotatedPositionOS.z);
    //output.positionWS = input.originPositionWS;

    // 默认变换下世界坐标等于物体坐标
    float3 positionVS = mul(UNITY_MATRIX_V, float4(allRotatedPositionWS, 1.0)).xyz;

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    half4 probeOcclusion; // 这个是什么用处?
    OUTPUT_SH4(output.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(output.positionWS), output.vertexSH, probeOcclusion);


    output.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));
    //output.positionCS = TransformObjectToHClip(allRotatedPositionWS);

    output.uv = input.uv;
    output.normalWS = input.normalWS;

    output.type = input.type;

    return output;
}

real4 QuadFrag(QuadVaryings input) : SV_TARGET
{
    float2 uv = input.uv;
    float4 tex = tex2D(_BaseMap, uv);

    clip(tex.a - 0.5);

    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;

    return float4(_BaseColor.rgb, 1);
}