#include "../Generic/PixelartStructures.hlsl"

Varyings UnlitVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    const float UNIT_SIZE = 16.0 / 256.0;

    float2 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0)).xy;
    float2 originWSSnapped = round(originWS / UNIT_SIZE) * UNIT_SIZE;
    float2 originWSOffset = originWSSnapped - originWS;

    o.positionWS = TransformObjectToWorld(v.positionOS);
    //o.positionCS = TransformObjectToHClip(v.positionOS);
    //o.positionWS += float3(originWSOffset, 0);
    o.positionCS = TransformWorldToHClip(float4(o.positionWS, 1));

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