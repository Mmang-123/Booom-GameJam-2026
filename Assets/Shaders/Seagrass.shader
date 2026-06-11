Shader "Mmang/Seagrass"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ObstacleMaskValue ("Is Obstacle", Float) = 0
        _ShadingBlend ("Shading Blend", Range(0, 1)) = 0
        _LightenBlend ("Lighten Blend", Range(0, 1)) = 0.16
        _Emission ("Emission", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}

        _Color ("Tint", Color) = (1,1,1,1)
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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Includes/SeagrassInput.hlsl"
            #include "Includes/SeagrassPass.hlsl"
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
            #include "Includes/SeagrassInput.hlsl"
            #include "Includes/SeagrassPass.hlsl"
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
            #include "Includes/SeagrassInput.hlsl"
            #include "Includes/SeagrassPass.hlsl"
            ENDHLSL
        }
    }
}
