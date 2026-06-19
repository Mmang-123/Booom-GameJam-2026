#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Generic/PixelartStructures.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Generic/PixelartShared.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/SpriteShading.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Lighting/Lighting.hlsl"
#include "../../Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/Background.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

TEXTURE2D(_VelocityBuffer);
SAMPLER(sampler_VelocityBuffer);


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

    // 偏移
    
    //float3 vPositionWS = TransformObjectToWorld(float3(0, v.positionOS.y, v.positionOS.z));
    float3 vPositionWS = originWS + originVSOffset;
    float3 vPositionWSTop = TransformObjectToWorld(float3(0.0, 1.0, 0.0)) + originVSOffset;
    float4 vPositionCS = TransformWorldToHClip(float4(vPositionWS, 1.0));
    float4 vPositionCSTop = TransformWorldToHClip(float4(vPositionWSTop, 1.0));
    float4 scrPos = ComputeScreenPos(vPositionCS);
    float4 scrPosTop = ComputeScreenPos(vPositionCSTop);

    float3 totalVelocity = 0;
    for (int i = 0; i < 4; i++)
    {
        float4 screenPos = lerp(scrPos, scrPosTop, 1.0 * i / 3.0);
        float2 screenUV = screenPos.xy / screenPos.w;
        float3 velocityTex = SAMPLE_TEXTURE2D_LOD(_VelocityBuffer, sampler_LinearClamp, screenUV, 0).xyz;
        //return float4(velocityTex.xyz, 1);
        
        if (velocityTex.z > totalVelocity.z)
        {
            float t = saturate(1.0 - velocityTex.z);
            totalVelocity = float3((((velocityTex.xy * 2.0) - 1.0) * cos(t * 1.5 * PI)).xy, velocityTex.z);
        }

        //float2 velocity = ((velocityTex.xy * 2.0) - 1.0) * cos(saturate(1.0 - velocityTex.z) * 1.5 * PI);
        
        //float2 velocity = SAMPLE_TEXTURE2D_LOD(_VelocityBuffer, sampler_LinearClamp, screenUV, 0).xy;
        //velocity = velocity * 2.0 - 1.0;

        //totalVelocity += velocity;
    }

    float2 worldOffset = totalVelocity.xy * v.color.r * ((saturate(totalVelocity.z)) * 0.6 + 0.4) * 0.5 + originWS;
    float2 objectOffset = TransformWorldToObject(float4(worldOffset, 0.0, 0.0));

    //
    o.positionWS = TransformObjectToWorld(v.positionOS.xyz + float3(objectOffset.x, 0, 1));
    float3 positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(o.positionWS, 1.0)).xyz + originVSOffset;
    o.positionCS = mul(GetViewToHClipMatrix(), float4(positionVS, 1.0));


    o.uv = v.uv;
    o.color.xy = _MainTex_ST.xy;
    
    return o;
}

float4 PixelartFrag(Varyings input) : SV_Target
{
    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    // Debug
    //float3 velocityTex = SAMPLE_TEXTURE2D(_VelocityBuffer, sampler_VelocityBuffer, screenUV).xyz;
    //return float4(abs(cos(saturate(1.0 - velocityTex.z) * 1.5 * PI)).xxx, 1);
    //float2 velocity = abs((velocityTex.xy * 2.0) - 1.0) * sin(saturate(velocityTex.z) * 2 * 3.1415926);
    //return float4(velocity, 0, 1);

    //
    int cols = (int)input.color.x;
    int rows = (int)input.color.y;
    int frameCount = cols * rows;
    int frameIndex = floor(_Time.y * _AnimSpeed) % frameCount;
    //return float4((frameIndex * 1.0 / frameCount).xxx, 1);

    float2 unitOffset = 1.0 / input.color.xy;
    // 左上开始，向右，行从上往下
    int col = frameIndex % cols;
    int row = frameIndex / cols;
    float2 texUV = input.uv * unitOffset + float2(col, rows - 1 - row) * unitOffset;

    float4 outputColor = tex2D(_MainTex, texUV) * _Color;
    float4 emissionTex = tex2D(_EmissionMap, input.uv);
    float4 emission = lerp(_Emission, emissionTex, emissionTex.a);
    outputColor.a = saturate(outputColor.a + emission.a);

    clip(outputColor.a - 0.5);

    float3 light = SampleLight(screenUV);


    if ((light.r <= 0.001 && light.g <= 0.001 && light.b <= 0.001) || (outputColor.r <= 0.001 && outputColor.g <= 0.001))
    {
        // Background
        light *= 1.0 - outputColor.b;
        outputColor.rgb = LightenBlend(SampleBackground(screenUV).rgb, light);
    }
    else
    {
        outputColor.rgb = lerp(LightenBlend(outputColor.rgb, light, _LightenBlend), outputColor.rgb * light, _ShadingBlend);
    }

    // Emission
    outputColor.rgb = lerp(outputColor.rgb, emission.rgb, emission.a);

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

    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    return o;
}

half4 ObstacleFrag(Varyings input) : SV_Target
{
    float4 color = tex2D(_MainTex, input.uv) * _Color;

    clip(min(color.a - 0.5, color.r + color.g + color.b - 0.001));

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
    o.uv = v.uv;
    o.color.xy = _MainTex_ST.xy;
    return o;
}

float4 UnlitFrag(Varyings input) : SV_Target
{
    int cols = (int)input.color.x;
    int rows = (int)input.color.y;
    int frameCount = cols * rows;
    int frameIndex = floor(_Time.y * _AnimSpeed) % frameCount;
    //return float4((frameIndex * 1.0 / frameCount).xxx, 1);

    float2 unitOffset = 1.0 / input.color.xy;
    // 左上开始，向右，行从上往下
    int col = frameIndex % cols;
    int row = frameIndex / cols;
    float2 texUV = input.uv * unitOffset + float2(col, rows - 1 - row) * unitOffset;

    float4 outputColor = tex2D(_MainTex, texUV) * _Color;
    float4 emission = tex2D(_EmissionMap, texUV);

    outputColor.rgb = lerp(outputColor.rgb, emission.rgb, emission.a);

    return outputColor;
}
