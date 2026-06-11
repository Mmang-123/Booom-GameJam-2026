Shader "Hidden/VelocityReproject"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
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
            #include "Assets/Plugins/MPixelartRender/ShaderLibrary/Pixelart/Sprite/Background.hlsl"

            #include "Assets/Plugins/MPixelartRender/ShaderLibrary/Blit/BlitInput.hlsl"

            sampler2D _BlitTexture;

            float2 _CameraDelta;      // currentPos - lastPos (world units)
            float2 _CameraWorldSize;  // camera visible width, height (world units)
            float _DeltaTime;

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                float2 prevUV = uv + _CameraDelta / _CameraWorldSize;
                float2 tex = tex2D(_BlitTexture, prevUV);
                float2 rawV = (tex * 2.0) - 1.0;
                
                float2 aV = abs(rawV);
                aV = max(0.0, aV - _DeltaTime);
                float2 newV = float2(sign(rawV.x) * aV.x, sign(rawV.y) * aV.y);
                
                return half4((newV + 1.0) / 2.0, 0, 1); 
            }
            ENDHLSL
        }
    }
}
