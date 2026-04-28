#define TEXTURE_BASED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
sampler2D _MainTex;
half4 _MainTex_ST;
float4 _Color;
float3 _Emission;
half4 _RendererColor;
CBUFFER_END