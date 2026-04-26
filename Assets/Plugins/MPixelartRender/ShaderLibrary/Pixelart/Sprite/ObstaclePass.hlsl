#include "../Generic/PixelartStructures.hlsl"
#include "../Generic/PixelartShared.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

float4 _ObstacleParams;

Varyings ObstacleVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    float3 originVS = mul(UNITY_MATRIX_V, float4(originWS, 1.0)).xyz;
    float3 originVSSnapped = float3(UNITSNAP(originVS.x, _ObstacleParams.x), UNITSNAP(originVS.y, _ObstacleParams.x), originVS.z);
    float3 originVSOffset = originVSSnapped - originVS;

    o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
    float3 positionVS = mul(UNITY_MATRIX_V, float4(o.positionWS, 1.0)).xyz + originVSOffset;
    o.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));


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

half4 ObstacleFrag(Varyings input) : SV_Target
{
#ifdef TEXTURE_BASED
    float alpha = tex2D(_MainTex, input.uv).a * input.color.a;
#else
    float alpha = input.color.a;
#endif

    clip(alpha - 0.5);

    return 1;
}