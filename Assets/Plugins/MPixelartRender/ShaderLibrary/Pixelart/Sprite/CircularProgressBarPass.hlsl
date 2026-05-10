

#include "Background.hlsl"
#include "../../Lighting/Lighting.hlsl"
#include "SpriteShading.hlsl"
#include "../Generic/PixelartShared.hlsl"

float _Fade;     // 0=透明 1=不透明，由 MaterialPropertyBlock 设置
float _Progress; // 进度 0~1，由 MaterialPropertyBlock 设置

#define SEGMENT_COUNT   5       // 进度条段数
#define GAP_RATIO       0.1     // 每段中间隙所占比例（两侧各一半）

float _FilledAt(float2 n, float outerR, float innerR)
{
    float d = length(n);
    float rm = step(d, outerR) * step(innerR, d);
    float na = frac(atan2(n.x, n.y) / 6.28318530718 + 1.0);
    float nf = frac(na * SEGMENT_COUNT);
    float halfGap = GAP_RATIO * 0.5;
    float dash = step(halfGap, nf) * (1.0 - step(1.0 - halfGap, nf));
    float nSegIdx = floor(na * SEGMENT_COUNT);
    float effectiveNf = saturate((nf - halfGap) / (1.0 - GAP_RATIO));
    float effectiveNa = (nSegIdx + effectiveNf) / SEGMENT_COUNT;
    return rm * step(effectiveNa, _Progress) * dash;
}

float4 BarFrag(Varyings input) : SV_Target
{
    // 1. 将UV坐标从 [0, 1] 映射到 [-0.5, 0.5]，将原点移动到中心点
    float2 uv = input.uv - 0.5;

    // 2. 计算当前像素到中心点的距离 (用于生成圆环遮罩)
    float dist = length(uv);

    // _UnitSize 是世界空间1像素大小，除以Sprite世界宽度得到UV空间1像素偏移
    float spriteWorldSize = length(unity_ObjectToWorld._m00_m10_m20);
    float pixelSize = _UnitSize / spriteWorldSize;

    // 环内缩1像素，留出外侧描边空间
    float outerRadius = 0.5 - pixelSize;
    float innerRadius = 0.45 - pixelSize;

    float outerMask = step(dist, outerRadius);
    float innerMask = step(innerRadius, dist);
    float ringMask = outerMask * innerMask;

    // 3. 计算当前像素的角度
    #define PI 3.14159265359
    float angle = atan2(uv.x, uv.y);
    float normalizedAngle = frac(angle / (2.0 * PI) + 1.0);

    // 5. 虚线遮罩（间隙居中于段边界，保证左右对称）
    float dashCoord = normalizedAngle * SEGMENT_COUNT;
    float dashFrac = frac(dashCoord);
    float halfGap = GAP_RATIO * 0.5;
    float dashMask = step(halfGap, dashFrac) * (1.0 - step(1.0 - halfGap, dashFrac));

    // 4. 进度遮罩（将 normalizedAngle 重映射到有效弧度，排除间隙，使间隙不占用进度值）
    float segIndex = floor(dashCoord);
    float effectiveDashFrac = saturate((dashFrac - halfGap) / (1.0 - GAP_RATIO));
    float effectiveAngle = (segIndex + effectiveDashFrac) / SEGMENT_COUNT;
    float progressMask = step(effectiveAngle, _Progress);

    float ringFinal = ringMask * progressMask * dashMask;

    // 6. 4邻域描边：当前像素未填充，但上/下/左/右任一邻域像素有填充
    float neighborFilled = max(
        max(_FilledAt(uv + float2( pixelSize, 0), outerRadius, innerRadius),
            _FilledAt(uv + float2(-pixelSize, 0), outerRadius, innerRadius)),
        max(_FilledAt(uv + float2(0,  pixelSize), outerRadius, innerRadius),
            _FilledAt(uv + float2(0, -pixelSize), outerRadius, innerRadius))
    );
    float outlineMask = (1.0 - ringFinal) * neighborFilled;

    // 7. 描边颜色：与 BackgroundSpritePass 相同（背景色 + 光照混合）
    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    float3 bgColor = SampleBackground(screenUV);
    float3 bgLight = SampleLight(screenUV);
    float3 outlineColor = LightenBlend(bgColor, bgLight);

    // 8. 组合输出：描边优先级低于环形主体
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    col = lerp(col, float4(outlineColor, 1.0), outlineMask);
    col = lerp(col, float4(input.color.rgb, 1.0), ringFinal);
    col.a *= _Fade;
    return col;
}