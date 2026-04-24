Shader "Mmang/Effect/GodRay"
{
    Properties
    {
        [Gamma] _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _BottomY("Bottom", Float) = 0
        _Height("Height", Float) = 8
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent"}

        Pass
        {
            Name "GodRay"

            ZWrite Off
            ZTest On
            Blend One One
            Cull Front

            HLSLPROGRAM

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #pragma vertex GodRayVert
            #pragma fragment GodRayFrag
            
            #include "GodRayInput.hlsl"
            #include "GodRayPass.hlsl"
            ENDHLSL
        }
        
    }
}
