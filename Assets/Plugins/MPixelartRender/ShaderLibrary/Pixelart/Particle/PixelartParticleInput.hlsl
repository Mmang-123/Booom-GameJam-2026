
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

struct PixelartParticleInput
{
    float3 positionOS : POSITION;
    half4 color : COLOR;
    float4 texcoord0 : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    float4 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
};
