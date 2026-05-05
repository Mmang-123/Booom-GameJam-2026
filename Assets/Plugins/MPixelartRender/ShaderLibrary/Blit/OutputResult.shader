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
            float _CameraScale;

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();
                //return float4(uv, 0, 1);
                // SubPixel Offset
                float aspectRatioTex = 16.0 / 9.0;
                float aspectRatioScreen = _ScreenParams.x / _ScreenParams.y;

                //
                //float cameraScale = _CameraScale;
                // Scale
                float2 scale;
                scale.x = aspectRatioScreen / aspectRatioTex;
                scale.y = 1.0;
            
                uv = (uv - 0.5) * scale + 0.5;
                
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0, 0, 0, 1); // Render black if outside bounds
                    
                float3 sceneColor = tex2D(_BlitTexture, uv).rgb;
                return float4(sceneColor, 1);
            }
            ENDHLSL
        }
    }
}
