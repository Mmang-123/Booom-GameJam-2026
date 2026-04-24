Shader "Mmang/Pixelart/PreviewOnly"
{
    Properties
    {
        _BaseMap("Albedo", 2D) = "white" {}
        [Gamma] _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)

        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5

        _BumpMap("Normal", 2D) = "bump" {}
        [Range(0, 1)] _BumpScale("Normal Scale", Float) = 1.0

        _LUTIndex("LUTIndex", Int) = 0
        _Outline("Outline", Int) = 0    

        [HideInInspector] _Surface("__surface", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            //"UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend One Zero
        ZWrite On
        ZTest LEqual
        Cull Back

        HLSLINCLUDE
        #pragma multi_compile _ _PIXELART
        #pragma multi_compile _ _IN_EDITOR
        ENDHLSL

        Pass
        {
            Name "Preview"

            // -------------------------------------
            // Render State Commands
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex PreviewVert
            #pragma fragment PreviewFrag

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "LitInput.hlsl"
            #include "LitPreviewPass.hlsl"
            ENDHLSL
        }
    }
}