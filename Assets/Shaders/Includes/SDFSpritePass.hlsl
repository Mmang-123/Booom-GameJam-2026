#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Generic/PixelartStructures.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Generic/PixelartShared.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/SpriteShading.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Lighting/Lighting.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/Background.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/SloaneShaderGeneric/Includes/SDF/Packing.hlsl"

// 采样噪声并按表面接近度加权扰动 SDF
// 仅在距阈值 _NoiseStrength 范围内的边界区域施加噪声，深内部/外部权重为 0
float PerturbSDF(float sdf, float2 worldXY)
{
    float noise = tex2D(_NoiseTexture, worldXY * _NoiseTexture_ST.xy + _NoiseTexture_ST.zw).r * 2.0 - 1.0;
    float distToSurface = abs(sdf - _SDFThreshold);
    float surfaceMask = 1.0 - smoothstep(0.0, max(_NoiseStrength, 0.0001), distToSurface);
    return sdf + noise * _NoiseStrength * surfaceMask;
}

// ========================
// Pixelart Pass
// ========================

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

    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color * _Color * _RendererColor;

    return o;
}

float4 PixelartFrag(Varyings input) : SV_Target
{
    // 从 SDF 纹理解压距离值（R8G8 packed magnitude + B channel sign）
    float sdf = UnpackSDF(tex2D(_MainTex, input.uv).rgb);
    sdf = PerturbSDF(sdf, input.positionWS.xy);
    clip(sdf - _SDFThreshold);

    float4 outputColor = input.color;
    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    float3 light = SampleLight(screenUV);

    if (light.r <= 0.001 && light.g <= 0.001 && light.b <= 0.001)
    {
        // Background
        outputColor.rgb = SampleBackground(screenUV);
    }
    else
    {
        outputColor.rgb = lerp(LightenBlend(outputColor.rgb, light, _LightenBlend), outputColor.rgb * light, _ShadingBlend);
    }

    return outputColor;
}

// ========================
// Obstacle Pass
// ========================

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

    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color * _Color * _RendererColor;

    return o;
}

half4 ObstacleFrag(Varyings input) : SV_Target
{
    float sdf = UnpackSDF(tex2D(_MainTex, input.uv).rgb);
    sdf = PerturbSDF(sdf, input.positionWS.xy);
    clip(sdf - _SDFThreshold);

    return _ObstacleMaskValue;
}

// ========================
// Preview (Unlit) Pass
// ========================

Varyings UnlitVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.positionWS = TransformObjectToWorld(v.positionOS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color * _Color * _RendererColor;
    return o;
}

float4 UnlitFrag(Varyings input) : SV_Target
{
    float sdf = UnpackSDF(tex2D(_MainTex, input.uv).rgb);
    sdf = PerturbSDF(sdf, input.positionWS.xy);
    clip(sdf - _SDFThreshold);

    return input.color;
}
