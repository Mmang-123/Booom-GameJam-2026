#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;

    half _Metallic;
    half _Smoothness;

    int _LUTIndex;
    int _Outline;

    half _Cutoff;
    half _Surface;
CBUFFER_END
