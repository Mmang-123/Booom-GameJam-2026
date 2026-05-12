Shader "Hidden/Mmang/Pixelart/Blit/TransitionBlitBack"
{
    Properties
    {
        _MaskStencilRef ("Mask Stencil Ref", Int) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Stencil
        {
            Ref [_MaskStencilRef] // 参考值，设为 1
            Comp NotEqual         // 仅当模板缓冲区的值 不等于 1 时，才渲染这个黑屏像素
            Pass Keep //(默认行为就是保持缓冲区不变，这里可以不写)
        }

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


            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();
                float3 rawColor = tex2D(_BlitTexture, uv);
                return half4(rawColor, 1.0);
            }
            ENDHLSL
        }
    }
}
