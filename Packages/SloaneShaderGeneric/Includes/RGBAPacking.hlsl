float4 EncodeFloatToRGBA32(float input)
{
    uint encoded = asuint(input); // 将 float 转换为 uint

    float4 output;
    output.r = (encoded & 0xFF) / 255.0;          // 提取最低 8 位
    output.g = ((encoded >> 8) & 0xFF) / 255.0;   // 提取次低 8 位
    output.b = ((encoded >> 16) & 0xFF) / 255.0;  // 提取次高 8 位
    output.a = ((encoded >> 24) & 0xFF) / 255.0;  // 提取最高 8 位

    return output;
}

float DecodeRGBA32ToFloat(float4 input)
{
    uint decoded = 0;
    decoded |= (uint)(input.r * 255.0) & 0xFF;          // 组合最低 8 位
    decoded |= ((uint)(input.g * 255.0) & 0xFF) << 8;   // 组合次低 8 位
    decoded |= ((uint)(input.b * 255.0) & 0xFF) << 16;  // 组合次高 8 位
    decoded |= ((uint)(input.a * 255.0) & 0xFF) << 24;  // 组合最高 8 位

    return asfloat(decoded); // 将 uint 转换回 float
}