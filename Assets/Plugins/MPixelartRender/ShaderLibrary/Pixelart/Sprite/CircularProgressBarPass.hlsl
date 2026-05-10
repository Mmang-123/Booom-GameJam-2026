

float4 BarFrag(Varyings input) : SV_Target
{
    // 1. 将UV坐标从 [0, 1] 映射到 [-0.5, 0.5]，将原点移动到中心点
    float2 uv = input.uv - 0.5;

    // 2. 计算当前像素到中心点的距离 (用于生成圆环遮罩)
    float dist = length(uv);

    float outerRadius = 0.5;
    float innerRadius = 0.45;

    float outerMask = step(dist, outerRadius); // 距离 <= 外半径时为1
    float innerMask = step(innerRadius, dist); // 距离 >= 内半径时为1
    
    // 环形遮罩：内圈和外圈相交的部分
    float ringMask = outerMask * innerMask;

    // 3. 计算当前像素的角度 (用于生成进度遮罩)
    // atan2(x, y) 可以让 0 度朝向正上方（Y轴正方向），且右侧为正，左侧为负
    // 结果范围是 [-PI, PI]
    float angle = atan2(uv.x, uv.y);

    // 将角度从 [-PI, PI] 映射到 [0, 1] 的范围
    // 使用 frac() 函数完美避免了着色器中的 if 分支（处理负数角度）
    #define PI 3.14159265359
    float normalizedAngle = frac(angle / (2.0 * PI) + 1.0);

    // 4. 根据外部参数 _CircleT 裁剪进度
    // 如果当前角度小于进度值，保留；否则丢弃（透明度设为0）
    float progressMask = step(normalizedAngle, input.color.a);

    // 5. 【新增】生成虚线遮罩
    // 将 0~1 的角度乘以分段数，这样数值就变成了 0 ~ _SegmentCount
    float dashCoord = normalizedAngle * 6;
    // 取小数部分，让每一段都在 0~1 之间循环。
    // 如果当前处于 0 ~ _DashFill 之间，显示实体(1)；否则显示间隙(0)
    float dashMask = step(frac(dashCoord), 0.9);

    // 6. 组合遮罩并输出颜色
    float4 col = float4(input.color.rgb, 1);
    // 最终的 Alpha 值是 环形遮罩 和 进度遮罩 的乘积
    col.a *= ringMask * progressMask * dashMask;

    return col;
}