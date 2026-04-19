#include "QuadPass.hlsl"

QuadVaryings QuadShadowVert(uint id : SV_VERTEXID)
{
    QuadVaryings output = (QuadVaryings)0;
    VarietyGenerationPoint input = _GenerationPoints[id];

    //
    float3 positionWS = input.originPositionWS + input.positionOS;
    output.positionWS = positionWS;
    //output.positionWS = input.originPositionWS;

    float3 positionVS = mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).xyz;

    output.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));
    //output.positionCS = TransformObjectToHClip(positionWS);

    output.uv = input.uv;

    return output;
}

QuadVaryings QuadShadowVertSmall(uint id : SV_VERTEXID)
{
    QuadVaryings output = (QuadVaryings)0;
    VarietyGenerationPoint input = _GenerationPoints[id];

    //
    input.positionOS.xyz *= 0.5;
    float3 positionWS = input.originPositionWS + input.positionOS;
    output.positionWS = positionWS;
    //output.positionWS = input.originPositionWS;

    float3 positionVS = mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).xyz;

    output.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));
    //output.positionCS = TransformObjectToHClip(positionWS);

    output.uv = input.uv;

    return output;
}

half QuadShadowFrag(QuadVaryings input) : SV_TARGET
{
    float2 uv = input.uv;
    float4 tex = tex2D(_BaseMap, uv);

    clip(tex.a - 0.5);

    return 0;
}