#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "../Compute/ComputeStructures.hlsl"

struct QuadVaryings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD0;
    half4 color : COLOR;
    float2 uv : TEXCOORD2;
    float3 positionWS : TEXCOORD3;
    int type : TEXCOORD4;

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
#ifdef DYNAMICLIGHTMAP_ON
    float2 dynamicLightmapUV  : TEXCOORD6;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _PaintLayer;
#if defined(_WITH_SMOOTHNESS)
    float _Smoothness;
#endif
CBUFFER_END

sampler2D _BaseMap;
uint _AdditionalLightCount;

StructuredBuffer<VarietyGenerationPoint> _GenerationPoints;