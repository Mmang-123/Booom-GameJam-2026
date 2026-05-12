Shader "Mmang/Pixelart/Sprite/DefaultEmission"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ObstacleMaskValue ("Is Obstacle", Float) = 0
        _LightenBlend ("Lighten Blend", Range(0, 1)) = 0.16
        _ShadingBlend ("Shading Blend", Range(0, 1)) = 0
        _Emission ("Emission", Color) = (0,0,0,0)
        _PreviewColor ("Preview Color", Color) = (1,1,1,1)

        _StencilRef ("Stencil Reference", Int) = 0

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,0,0)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Stencil
        {
            Ref [_StencilRef]    // 参考值，比如 1
            Comp Always          // 总是通过测试
            Pass Replace         // 测试通过时，将模板缓冲区的值替换为 Ref (即 1)
        }

        Pass
        {
            Tags { "LightMode" = "MForward" }

            HLSLPROGRAM

            // -------------------------------------
            // Shader Stages
            #pragma vertex UnlitVert
            #pragma fragment UnlitEmissionFrag

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "DefaultInput.hlsl"
            #include "DefaultPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Obstacle Mask"

            Tags
            {
                "LightMode" = "ObstacleMask"
            }

            HLSLPROGRAM

            // -------------------------------------
            // Shader Stages
            #pragma vertex ObstacleVert
            #pragma fragment ObstacleExtendFrag

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "PixelartInput.hlsl"
            #include "ObstaclePass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "Preview"
            }

            HLSLPROGRAM

            // -------------------------------------
            // Shader Stages
            #pragma vertex UnlitVert
            #pragma fragment UnlitFrag

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "PreviewInput.hlsl"
            #include "PreviewPass.hlsl"
            ENDHLSL
        }
    }
}
