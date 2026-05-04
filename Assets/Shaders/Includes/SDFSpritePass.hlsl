#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Generic/PixelartStructures.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Generic/PixelartShared.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/SpriteShading.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Lighting/Lighting.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/Background.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/SloaneShaderGeneric/Includes/SDF/Packing.hlsl"

#include "Packages\SloaneShaderGeneric\Includes\Random.hlsl"

float2 GetBoiledUV (float2 uv, float2 noiseUV, float strength, float duration) {
    float currentSeed = floor(_Time.y / duration);
    float2 noiseUVOffset = hash21(currentSeed) + noiseUV;
	float noiseSample = tex2D(_NoiseTexture, noiseUVOffset).r * 2.0 * PI;
	float2 direction = float2(cos(noiseSample), sin(noiseSample));
	
    return uv + direction * strength;
}

float PerturbSDF(float2 uv, float2 worldXY)
{
    float2 worldUV = worldXY * _NoiseTexture_ST.xy + _NoiseTexture_ST.zw;
    // _NoiseStrength 单位为像素，换算为 UV 偏移
    float pixelStrength = _NoiseStrength * _MainTex_TexelSize.x;
#if !defined(_BOIL_EFFECT_ENABLED)
    float sdf = UnpackSDF(tex2D(_MainTex, uv).rgb);
    float noise = tex2D(_NoiseTexture, worldUV).r * 2.0 - 1.0;
    float distToSurface = abs(sdf - _SDFThreshold);
    float surfaceMask = 1.0 - smoothstep(0.0, max(pixelStrength, 0.0001), distToSurface);
    return sdf + noise * pixelStrength * surfaceMask;
#else
    float2 boiledUV = GetBoiledUV(uv, worldUV, pixelStrength, _BoilDuration);
    return UnpackSDF(tex2D(_MainTex, boiledUV).rgb);
#endif
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
    float sdf = PerturbSDF(input.uv, input.positionWS.xy);
    clip(sdf - _SDFThreshold);

    float4 outputColor = input.color;
    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    float3 light = SampleLight(screenUV);

    if ((light.r <= 0.001 && light.g <= 0.001 && light.b <= 0.001) || (outputColor.r <= 0.001 && outputColor.g <= 0.001 && outputColor.b <= 0.001))
    {
        // Background
        outputColor.rgb = LightenBlend(SampleBackground(screenUV).rgb, light);
    }
    else
    {
        outputColor.rgb = lerp(LightenBlend(outputColor.rgb, light, _LightenBlend), outputColor.rgb * light, _ShadingBlend);
    }

    outputColor.rgb = lerp(outputColor.rgb, input.color.rgb, _EmissionStrength);

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
    float sdf = PerturbSDF(input.uv, input.positionWS.xy);
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
    float sdf = PerturbSDF(input.uv, input.positionWS.xy);
    clip(sdf - _SDFThreshold);

    return input.color;
}
