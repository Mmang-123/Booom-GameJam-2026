Shader "Hidden/Mmang/DarkSight"
{
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

            #include "Assets/Plugins/MPixelartRender/ShaderLibrary/Blit/BlitInput.hlsl"

            sampler2D _BlitTexture;
            
            float4 _DarkSightParams;

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                float2 scaledUV = uv;
                scaledUV.y = scaledUV.y * _ScreenParams.y / _ScreenParams.x;


                float3 sceneColor = tex2D(_BlitTexture, uv).rgb;

                float dis = distance(_DarkSightParams.xy, scaledUV);
                if (dis > _DarkSightParams.z)
                    return half4(sceneColor, 1);



                float3 luminanceWeight = float3(0.2126, 0.7152, 0.0722);
                float luminance = dot(sceneColor, luminanceWeight);
                float3 grayColor = float3(luminance, luminance, luminance);
                
                //return float4(sceneColor, 1);
                return float4(grayColor, 1);
            }
            ENDHLSL
        }
    }
}
