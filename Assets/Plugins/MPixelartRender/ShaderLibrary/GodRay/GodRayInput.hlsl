#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"


CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float _BottomY;
    float _Height;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float3, _FadeParams)
UNITY_INSTANCING_BUFFER_END(Props)

uint _AdditionalLightCount;


struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD0;
    half4 color : COLOR;
    float2 uv : TEXCOORD2;
    float3 positionWS : TEXCOORD3;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};