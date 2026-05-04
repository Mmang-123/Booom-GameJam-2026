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
    float4 emission = tex2D(_EmissionMap, input.uv);
#else
    float4 outputColor = input.color;
    float4 emission = _Emission;
#endif

    outputColor.rgb = lerp(outputColor.rgb, emission.rgb, emission.a);
    outputColor.rgb *= _PreviewColor;

    return outputColor;
}

float4 EmissionFrag(Varyings input) : SV_Target
{
#ifdef TEXTURE_BASED
    float4 outputColor = tex2D(_MainTex, input.uv) * input.color;
#else
    float4 outputColor = input.color;
#endif

    return outputColor;
}

float4 BackgroundFrag(Varyings input) : SV_Target
{
#if defined(DEBUG_DISPLAY)
    float2 cell = floor(input.positionWS.xy);
    // fmod is truncation-toward-zero and returns negative for negative inputs;
    // frac(sum * 0.5) is always in [0,1) so parity works for all coordinates.
    float checker = step(0.5, frac((cell.x + cell.y) * 0.5));
    return SRGBToLinear(checker < 1.0 ? float4(0.5, 0.5, 0.5, 1.0) : float4(0.75, 0.75, 0.75, 1.0));
#else
#ifdef TEXTURE_BASED
    float4 outputColor = tex2D(_MainTex, input.uv) * input.color;
#else
    float4 outputColor = input.color;
#endif
    outputColor.rgb *= _PreviewColor;
    return outputColor;
#endif
}