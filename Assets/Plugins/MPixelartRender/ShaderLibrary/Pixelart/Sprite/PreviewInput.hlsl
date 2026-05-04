#define TEXTURE_BASED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
sampler2D _MainTex;
sampler2D _EmissionMap;
half4 _MainTex_ST;
float4 _Color;
float3 _PreviewColor;
half4 _RendererColor;
CBUFFER_END