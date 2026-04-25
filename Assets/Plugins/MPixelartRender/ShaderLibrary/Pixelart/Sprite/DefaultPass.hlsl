#include "../Generic/PixelartStructures.hlsl"

Varyings UnlitVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.positionCS = TransformObjectToHClip(v.positionOS);
    #if defined(DEBUG_DISPLAY)
    o.positionWS = TransformObjectToWorld(v.positionOS);
    #endif
#ifdef TEXTURE_BASED
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#else
    o.uv = v.uv;
#endif
    o.color = v.color * _Color * _RendererColor;
    return o;
}

float4 UnlitFrag(Varyings input) : SV_Target
{
#ifdef TEXTURE_BASED
    float4 outputColor = tex2D(_MainTex, input.uv) * input.color;
#else
    float4 outputColor = input.color;
#endif

    return outputColor;
}