#include "Background.hlsl"
#include "../../Lighting/Lighting.hlsl"
#include "SpriteShading.hlsl"
#include "../Generic/PixelartShared.hlsl"


float _FilledAt(float2 n, float radius)
{
    float dist = length(n);
    float mask = step(dist, radius);
    return mask;
}

float4 CircleFrag(Varyings input) : SV_Target
{
    // 1. 将UV坐标从 [0, 1] 映射到 [-0.5, 0.5]，将原点移动到中心点
    float2 uv = input.uv - 0.5;

    // 2. 计算当前像素到中心点的距离 (用于生成圆环遮罩)
    float dist = length(uv);

    // _UnitSize 是世界空间1像素大小，除以Sprite世界宽度得到UV空间1像素偏移
    float spriteWorldSize = length(unity_ObjectToWorld._m00_m10_m20);
    float pixelSize = _UnitSize / spriteWorldSize;

    // 环内缩1像素，留出外侧描边空间
    float radius = 0.5 - pixelSize;

    float innerMask = step(dist,radius);

    float neighborFilled = max(
        max(_FilledAt(uv + float2( pixelSize, 0), radius),
            _FilledAt(uv + float2(-pixelSize, 0), radius)),
        max(_FilledAt(uv + float2(0,  pixelSize), radius),
            _FilledAt(uv + float2(0, -pixelSize), radius))
    );

    float4 col = float4(input.color.rgb, (neighborFilled - innerMask) * input.color.a);
    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    float3 bgColor = SampleBackground(screenUV);

    col.rgb = lerp(col, bgColor, 0.4);

    return col;
}