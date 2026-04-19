#include "QuadPass.hlsl"
#include "Assets/Plugins/Mmang/PixelartRender/Shaders/Includes/Inputs/DepthNormals.hlsl"
#include "Assets/Plugins/Mmang/PixelartRender/Shaders/Includes/Transform.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

struct QuadInfoV2F
{
    float4 positionCS : SV_POSITION;
    float4 nz : TEXCOORD0;
    float2 uv : TEXCOORD1;
    float3 positionWS : TEXCOORD2;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

QuadInfoV2F BillboardInfosVert(uint id : SV_VERTEXID)
{
    QuadInfoV2F output = (QuadInfoV2F)0;
    VarietyGenerationPoint input = _GenerationPoints[id];

    //float3 originPositionVS = positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.originPositionWS, 1.0)) + originVSOffset;

    // 摄像机信息
    float3 upCamVec = normalize(UNITY_MATRIX_V._m10_m11_m12);
    float3 forwardCamVec = -normalize(UNITY_MATRIX_V._m20_m21_m22);
    float3 rightCamVec = normalize(UNITY_MATRIX_V._m00_m01_m02);
    float4x4 rotationCamMatrix = float4x4(rightCamVec, 0, upCamVec, 0, forwardCamVec, 0, 0, 0, 0, 1);

    // 对齐摄像机
    float3 positionOS = input.positionOS;
    float3 rotatedPositionOS =
        + positionOS.x * rightCamVec
        + positionOS.y * upCamVec
        + positionOS.z * forwardCamVec;

    float3 allRotatedPositionWS = input.originPositionWS + rotatedPositionOS;
    float3 positionWS = input.originPositionWS + float3(rotatedPositionOS.x, 0, rotatedPositionOS.z);
    //output.positionWS = input.originPositionWS;

#ifdef _PIXELART
    float3 originWS = input.originPositionWS;
    float3 originVS = mul(PIXELART_CAMERA_MATRIX_V, float4(originWS, 1.0)).xyz;
    float3 originVSOffset = float3(UNITSNAP(originVS.x, _UnitSize), UNITSNAP(originVS.y, _UnitSize), originVS.z) - originVS;
    float3 positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(allRotatedPositionWS, 1.0)).xyz + originVSOffset;
#else
    // 默认变换下世界坐标等于物体坐标
    float3 positionVS = mul(UNITY_MATRIX_V, float4(allRotatedPositionWS, 1.0)).xyz;
#endif

    output.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));
    //output.positionCS = TransformObjectToHClip(allRotatedPositionWS);

    output.nz.xyz = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, input.normalWS));
    output.nz.w = (-positionVS.z) / _ProjectionParams.z;

    output.uv = input.uv;
    output.positionWS = positionWS;

    return output;
}

InfoOutput InfosFrag(QuadInfoV2F input) : SV_TARGET
{
    InfoOutput output = (InfoOutput)0;
    //return float4(1, 0, 0, 1);
    float2 uv = input.uv;
    float a = tex2D(_BaseMap, uv).a;

    clip(a - 0.5);

    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    float4 depthNormals = EncodeDepthNormal(input.nz.w, input.nz.xyz);
    
    //float depth = input.nz.w;
    //float3 positionWS = GetWorldPositionWithDepth(screenUV, depth);

    output.depthNormals = depthNormals;
    output.worldPosition = input.positionWS;
    output.paintLayer = _PaintLayer.rgb;

    return output;
}