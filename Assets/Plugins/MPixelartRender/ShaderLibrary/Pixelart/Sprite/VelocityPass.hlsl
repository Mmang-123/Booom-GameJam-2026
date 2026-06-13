
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

Varyings UnlitVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    const float UNIT_SIZE = 16.0 / 256.0;

    o.positionWS = TransformObjectToWorld(v.positionOS);

    o.positionCS = TransformWorldToHClip(float4(o.positionWS, 1));

#ifdef TEXTURE_BASED
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#else
    o.uv = v.uv;
#endif
    o.color = v.color * _Color * _RendererColor;
    return o;
}

half4 VelocityFrag(Varyings input) : SV_Target
{
    float2 av = abs(_Velocity.xy);
    av = min(1.0, av);
    clip(av.x + av.y - 0.1);

    float2 velocity = float2(sign(_Velocity.x) * av.x, sign(_Velocity.y) * av.y);

    float UNIT_SIZE = 1.0 / _ScreenParams.x;

#ifdef TEXTURE_BASED
    float4 color = float4(0, 0, 0, 0);
    color += tex2D(_MainTex, input.uv + float2(0, 0));
    color += tex2D(_MainTex, input.uv + float2(UNIT_SIZE, 0));
    color += tex2D(_MainTex, input.uv + float2(0, UNIT_SIZE));
    color += tex2D(_MainTex, input.uv + float2(-UNIT_SIZE, 0));
    color += tex2D(_MainTex, input.uv + float2(0, -UNIT_SIZE));

    color *= input.color;
#else
    float4 color = input.color;
#endif

    clip(color.a - 0.1);

    return float4((velocity + 1.0) / 2.0, 1, 1);
}