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

            #include "../Blit/BlitInput.hlsl"
            #include "ObstacleShared.hlsl"
            #include "Lighting.hlsl"


            sampler2D _BlitTexture;
            
            float4 _DarkSightParams;

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                // 平滑插值
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                return lerp(a, b, u.x) + 
                       (c - a) * u.y * (1.0 - u.x) + 
                       (d - b) * u.x * u.y;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                GET_BLIT_UV();

                float3 sceneColor = tex2D(_BlitTexture, uv).rgb;

                float2 scaledUV = uv;
                scaledUV.y = scaledUV.y * _ScreenParams.y / _ScreenParams.x;
                float2 scaledDarkCenterUV = _DarkSightParams.xy;
                scaledDarkCenterUV.y = scaledDarkCenterUV.y * _ScreenParams.y / _ScreenParams.x;

                float dis = distance(scaledDarkCenterUV, scaledUV);
                if (dis > _DarkSightParams.z)
                    return half4(sceneColor, 1);

                float3 sampledLight = SampleLight(uv);
                if (_DarkSightParams.w == 1 && (sampledLight.r + sampledLight.g + sampledLight.b) >= 0.001)
                    return half4(sceneColor, 1);

                float sdf = GetObstacleSDF(uv);
                //sdf += 0.0008 * sin(10.0 * (_Time.y + uv.x + uv.y));

                float n = noise(uv * 5 + _Time.y * 2);
                n = n * 2.0 - 1.0; // 将噪声从 [0, 1] 映射到 [-1, 1]

                sdf += 0.0008 * n;

                float3 outlineColor = 0;

                if (sdf < 0.008 && sdf > 0.002) // 假设描边同时向内向外扩展半个厚度
                {
                    outlineColor = 0.8;
                }

                float3 luminanceWeight = float3(0.2126, 0.7152, 0.0722);
                float luminance = dot(sceneColor, luminanceWeight);
                float3 grayColor = float3(luminance, luminance, luminance);
                
                float3 outputColor = grayColor * 0.7 + outlineColor;
                
                return float4(outputColor, 1);
            }
            ENDHLSL
        }
    }
}
