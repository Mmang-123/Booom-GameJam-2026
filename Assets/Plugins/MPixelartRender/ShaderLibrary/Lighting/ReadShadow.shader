Shader "Hidden/Mmang/Pixelart/Blit/ReadShadow"
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
            int2 _ChunkIndex;

            half Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                float2 sampleUV = (uv + _ChunkIndex) / 3.0;
                float unitSize = 1.0 / (256 * 3.0);

                float shadow1 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(0, 0)).a;
                float shadow2 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(unitSize, 0)).a;
                float shadow3 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(0, unitSize)).a;
                float shadow4 = SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, sampleUV + float2(unitSize, unitSize)).a;
                
                return (shadow1 + shadow2 + shadow3 + shadow4) / 4.0;
            }
            ENDHLSL
        }
    }
}
