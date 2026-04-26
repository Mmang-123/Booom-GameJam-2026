Shader "Mmang/Pixelart/Sprite/Default"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ObstacleMask ("Is Obstacle", Float) = 0

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

        Pass
        {
            Tags { "LightMode" = "MForward" }

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
            #pragma fragment ObstacleFrag

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
            #include "PixelartInput.hlsl"
            #include "PixelartPreviewPass.hlsl"
            ENDHLSL
        }
    }
}
