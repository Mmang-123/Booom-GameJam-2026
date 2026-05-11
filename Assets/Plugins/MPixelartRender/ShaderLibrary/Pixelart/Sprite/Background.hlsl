#ifndef SPRITE_BACKGROUND_INCLUDED
#define SPRITE_BACKGROUND_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../../Lighting/ObstacleShared.hlsl"

float _Hash(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// 生成单层雪花的函数
float Snowflake(float2 uv, float scale, float speed, float wind, float2 sdfPushVector)
{
    float2 st = uv * scale;

    // URP 中 _Time 同样是内置的 float4 变量：(t/20, t, t*2, t*3)
    // 加入正弦波模拟空气阻力的左右摇摆
    st.x += sin(_Time.y * 2.0 + st.y * 5.0) * 0.1;

    // 根据时间和速度/风向移动
    st.y -= _Time.y * speed;
    st.x -= _Time.y * wind;

    float2 gridId = floor(st);
    float2 gridUv = frac(st) - 0.5;

    // 随机偏移和距离计算
    float2 offset = float2(_Hash(gridId) - 0.5, _Hash(gridId + 13.3) - 0.5) * 0.8;
    offset += sdfPushVector * 0.1;

    float dist = length(gridUv - offset);

    // 边缘柔和
    float radius = 0.01 + _Hash(gridId + 7.7) * 0.01;
    //float intensity = smoothstep(radius, radius * 0.3, dist);
    float intensity = step(dist, radius);

    return intensity * (0.5 + 0.5 * _Hash(gridId + 11.1));
}

float2 GetSDFGradient(float2 uv)
{
    float2 e = 1.0 / _ScreenParams.xy; // 采样偏移量
    float nx = GetObstacleSDF(uv + float2(e.x, 0)) - GetObstacleSDF(uv - float2(e.x, 0));
    float ny = GetObstacleSDF(uv + float2(0, e.y)) - GetObstacleSDF(uv - float2(0, e.y));
    return normalize(float2(nx, ny) + 1e-5); // 加上 1e-5 防止除以 0
}

float3 SampleBackground(float2 screenUV)
{
    float3 backgroundColor = SRGBToLinear(float3(0.0313, 0.0313, 0.1215));

    /*
    float2 originUV = WorldToUV(float2(0.0, 0.0));

    float2 warpUV = screenUV;
                
    // 如果在障碍物的影响半径内，且在障碍物外部（dist > 0）
    float sdfDist = GetObstacleSDF(screenUV);
    float2 pushVector = 0;
    if (sdfDist < 0.2 && sdfDist > 0.0)
    {
        float2 gradient = GetSDFGradient(screenUV);
        
        // 距离障碍物越近，推力越大
        float pushFactor = (0.2 - sdfDist) / 0.2;
        
        // 视觉空间扭曲：梯度指向外部。我们希望雪花向外走，所以把采样点往内部拉(减去梯度)
        //warpUV -= gradient * pushFactor * 0.2 * 0.1; 
        pushVector = gradient * pushFactor;
    }

    float2 offsetUV = screenUV - originUV;


    float snow = 0.0;
    snow += Snowflake(offsetUV, 8, -0.5, -0.1, pushVector) * 0.8;

    float3 outputColor = backgroundColor + snow * 0.25;
    */

    return backgroundColor;
    //return outputColor;
}

#endif