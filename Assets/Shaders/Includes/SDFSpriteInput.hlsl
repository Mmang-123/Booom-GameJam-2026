#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
sampler2D _MainTex;
half4 _MainTex_ST;
sampler2D _NoiseTexture;
half4 _NoiseTexture_ST;
float4 _Color;
half4 _RendererColor;
half _NoiseStrength;
half _ObstacleMaskValue;
half _SDFThreshold;
float _ShadingBlend;
float _LightenBlend;
CBUFFER_END