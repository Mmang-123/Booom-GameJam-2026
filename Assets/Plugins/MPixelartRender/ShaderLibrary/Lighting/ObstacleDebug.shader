Shader "Hidden/Mmang/Pixelart/Blit/ObstacleDebug"
{
    Properties
    {
        _DebugType ("Is Obstacle", Int) = 0
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
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #include "../Common/Math.hlsl"
            #include "../Blit/BlitInput.hlsl"

            #include "ObstacleShared.hlsl"

            int _DebugType;

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                if (_DebugType == 0)
                {
                    float mask = GetObstacleMask(uv);
                    return float4(mask.xxx, 1);
                }
                else if (_DebugType == 1)
                {
                    float sdf = GetObstacleSDF(uv);
                    return float4(sdf, 0, 0, 1);
                }
                else
                {
                    float sdf = GetObstacleSDF(uv);
                    sdf = frac(sdf * 50);
                    return float4(sdf, 0, 0, 1);
                }
            }
            ENDHLSL
        }
    }
}
