#include "../Generic/PixelartStructures.hlsl"
#include "../Generic/PixelartShared.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"


Varyings PixelartVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    float3 originVS = mul(PIXELART_CAMERA_MATRIX_V, float4(originWS, 1.0)).xyz;
    float3 originVSSnapped = float3(UNITSNAP(originVS.x, _UnitSize), UNITSNAP(originVS.y, _UnitSize), originVS.z);
    float3 originVSOffset = originVSSnapped - originVS;

    o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
    float3 positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(o.positionWS, 1.0)).xyz + originVSOffset;
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

BufferOutput PixelartFrag(Varyings input) : SV_Target
{
    BUFFER_OUTPUT_INIT();

#ifdef TEXTURE_BASED
    float4 outputColor = tex2D(_MainTex, input.uv) * input.color;
#else
    float4 outputColor = input.color;
#endif

    clip(outputColor.a - 0.5);

    OUTPUT_ALBEDO4(outputColor);
    OUTPUT_OBSTACLE_MASK(_ObstacleMask);

    RETURN_BUFFER_VALUE();
}