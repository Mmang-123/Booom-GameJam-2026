Shader "Mmang/Generations/Examples/Quad"
{
    Properties
    {
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque"}

        HLSLINCLUDE
        
        ENDHLSL

        Pass
        {
            Name "Opaque"

            ZWrite On
            ZTest On
            Cull Off

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma vertex BillboardVert
            #pragma fragment QuadFrag

            #include "QuadInput.hlsl"
            #include "QuadPass.hlsl"

            ENDHLSL
        }
    }
}
