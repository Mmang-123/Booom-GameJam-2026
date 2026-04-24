#ifndef PIXELART_STRUCTURES_INCLUDED
#define PIXELART_STRUCTURES_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD0;
    float4 tangentWS : TEXCOORD1; 
    half4 color : COLOR;
    float2 uv : TEXCOORD2;
    float3 positionWS : TEXCOORD3;
    //float3 positionVS : TEXCOORD4;
    float2 originUV : TEXCOORD4;

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
#ifdef DYNAMICLIGHTMAP_ON
    float2 dynamicLightmapUV  : TEXCOORD6;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#endif // PIXELART_STRUCTURES_INCLUDED