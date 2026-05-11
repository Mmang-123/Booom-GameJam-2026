Shader "Hidden/Mmang/Pixelart/Blit/ReadLighting"
{
    Properties
    {
        _ChunkIndex ("Chunk Index", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Blit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #include "../Common/Math.hlsl"
            #include "../Blit/BlitInput.hlsl"

            TEXTURE2D(_MLightingTexture);
            SAMPLER(sampler_MLightingTexture);
            float2 _ChunkIndex;  // float so even N (fractional origin) is handled correctly
            float2 _ChunkRange;

            inline float GetStrength(float3 color)
            {
                return saturate(max(color.r, max(color.g, color.b)));
            }

            half Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                float2 sampleUV = (uv + _ChunkIndex) / _ChunkRange;
                float2 unitSize = 1.0 / (256.0 * _ChunkRange);

                float s1 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(0, 0)).a;
                float s2 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(unitSize.x, 0)).a;
                float s3 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(0, unitSize.y)).a;
                float s4 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(unitSize.x, unitSize.y)).a;
                
                return (s1 + s2 + s3 + s4) / 4.0;

                /*
                float3 color1 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(0, 0)).rgb;
                float3 color2 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(unitSize, 0)).rgb;
                float3 color3 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(0, unitSize)).rgb;
                float3 color4 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(unitSize, unitSize)).rgb;
                
                return (GetStrength(color1), GetStrength(color2), GetStrength(color3), GetStrength(color4)) / 4.0;
                */
            }
            ENDHLSL
        }
    }
}
