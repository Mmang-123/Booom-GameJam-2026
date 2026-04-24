Shader "Mmang/Effect/CloudShadow"
{
    Properties
    {
        _MainColor ("MainColor", color) = (1,1,1,1)
        _CloudScale ("CloudScale", float) = 1.1
        _Speed ("CloudSpeed", float) = 0.03
        _CloudDark ("CloudDark", float) = 0.5
        _CloudLight ("CloudLight", float) = 0.3
        _CloudCover ("CloudCover", float) = 0.2
        _CloudAlpha ("CloudAlpha", float) = 8.0
        _SkyTint ("SkyTint", float) = 0.5
        _SkyColor1 ("SkyColor1", color) = (0.2,0.4,0.6,1.0)
        _SkyColor2 ("SkyColor2", color) = (0.4,0.7,1.0,1.0)
    }
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
            #pragma fragment CloudShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #include "../Blit/BlitInput.hlsl"            
            #include "CloudShadowPass.hlsl"

            ENDHLSL
        }
    }
}
