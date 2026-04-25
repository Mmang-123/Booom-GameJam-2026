#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

//sampler2D _BlitTexture;
struct LightData2D {
    float4 position; 
    float4 color;
};

StructuredBuffer<LightData2D> _CustomLightBuffer;
int _LightCount;


half4 LightingFrag(Varyings input) : SV_Target
{
    GET_BLIT_UV();


    return float4(uv, 0, 1);
}