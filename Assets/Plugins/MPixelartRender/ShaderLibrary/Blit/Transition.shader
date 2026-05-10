Shader "Hidden/Mmang/Pixelart/Blit/Transition"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
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
            #include "Assets/Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/Background.hlsl"

            #include "BlitInput.hlsl"

            sampler2D _BlitTexture;
            float _SceneTransition;

            float remap(float value, float rawX, float rawY, float targetX, float targetY)
            {
                return targetX + (value - rawX) / (rawY - rawX) * (targetY - targetX);
            }

            float clampRemap(float value, float rawX, float rawY, float targetX, float targetY)
            {
                return remap(clamp(value, rawX, rawY), rawX, rawY, targetX, targetY);
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();
                // 0~0.5: 淡入黑幕, 0.5~1: 淡出黑幕
                float fade = clampRemap(_SceneTransition, 0.0, 0.5, 0.0, 1.0)
                           - clampRemap(_SceneTransition, 0.5, 1.0, 0.0, 1.0);

                float3 rawColor = tex2D(_BlitTexture, uv);
                return half4(lerp(rawColor, SampleBackground(uv), fade), 1.0);
            }
            ENDHLSL
        }
    }
}
