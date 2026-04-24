Shader "Hidden/Mmang/Pixelart/Blit/OutputResult"
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
            #include "BlitInput.hlsl"

            sampler2D _BlitTexture;
            float4 _Resolution;
            float4 _SubPixelOffset;

            float _CameraScale;

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();
                //return float4(uv, 0, 1);
                // SubPixel Offset
                uv += _SubPixelOffset.xy;

                //
                //float cameraScale = _CameraScale;
                // Scale
                //uv = (uv - 0.5) * cameraScale + 0.5;
                
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0, 0, 0, 1); // Render black if outside bounds
                    
                float3 sceneColor = tex2D(_BlitTexture, uv).rgb;
                return float4(sceneColor, 1);
            }
            ENDHLSL
        }
    }
}
