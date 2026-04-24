Shader "Hidden/Mmang/Pixelart/ScreenHalo"
{
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

            sampler2D _BlitTexture;

            TEXTURE2D(_SpecularOutputBuffer);
            SAMPLER(sampler_SpecularOutputBuffer);

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                float2 texSize = 1.0 / _ScreenParams.xy;

                float3 halo = 0;

                //float3 specular = SAMPLE_TEXTURE2D(_SpecularOutputBuffer, sampler_SpecularOutputBuffer, uv).rgb;
                for (int index = -20; index <= 20; index++)
                {
                    float2 cellUV = uv + float2(texSize.x * index, 0);
                    float3 cellSpecular = SAMPLE_TEXTURE2D(_SpecularOutputBuffer, sampler_SpecularOutputBuffer, cellUV).rgb;

                    float strength = max(0, 20 - abs(index)) / 20.0;

                    strength = pow(strength, 2);

                    halo += cellSpecular * strength * 0.05;
                }

                float3 sceneColor = tex2D(_BlitTexture, uv).rgb;
                return float4(sceneColor + halo, 1);
            }
            ENDHLSL
        }
    }
}
