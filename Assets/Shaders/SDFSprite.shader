Shader "Sloane/SDFSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTexture("Noise Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Float) = 0
        _ShadingBlend ("Shading Blend", Range(0, 1)) = 0
        _LightenBlend ("Lighten Blend", Range(0, 1)) = 0.16
        _ObstacleMaskValue ("Is Obstacle", Float) = 0
        _SDFThreshold ("SDF Threshold", Float) = 0.5
        _EmissionStrength ("Emission Strength", Range(0, 1)) = 0

        [HideInInspector] _BoilDuration ("Boil Duration", Float) = 0.25
        [HideInInspector] _BoilEnabled ("Boil Enabled", Float) = 0

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,0,0)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #pragma multi_compile _ _PIXELART
        ENDHLSL

        Pass
        {
            Name "PixelartRender"

            Tags
            {
                "LightMode" = "Pixelart"
            }

            HLSLPROGRAM

            // -------------------------------------
            // Shader Stages
            #pragma vertex PixelartVert
            #pragma fragment PixelartFrag

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ _BOIL_EFFECT_ENABLED
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Includes/SDFSpriteInput.hlsl"
            #include "Includes/SDFSpritePass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Obstacle Mask"
            Blend One Zero

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
            #include "Includes/SDFSpriteInput.hlsl"
            #include "Includes/SDFSpritePass.hlsl"
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
            #include "Includes/SDFSpriteInput.hlsl"
            #include "Includes/SDFSpritePass.hlsl"
            ENDHLSL
        }
    }

    CustomEditor "Sloane.SDFSpriteShaderGUI"
}
