#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
sampler2D _MainTex;
sampler2D _EmissionMap;
half4 _MainTex_ST;
float4 _MainTex_TexelSize;

float4 _Color;
float4 _Emission;
half4 _RendererColor;
half _ObstacleMaskValue;
float _ShadingBlend;
float _LightenBlend;
CBUFFER_END