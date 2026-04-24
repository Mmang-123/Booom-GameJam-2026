#ifndef PIXELART_LUT_INCLUDED
#define PIXELART_LUT_INCLUDED
TEXTURE2D(_PixelartLUT);
SAMPLER(sampler_PixelartLUT);

inline uint GetRealPixelartLUTIndex(uint rawPixelartIndex, float shadowLevel)
{
    // 阴影分三层
    return rawPixelartIndex * 3 + floor(saturate(shadowLevel) * 2.9);
}

inline float4 SearchPixelartLUT_RealIndex(uint index, float value)
{
    // index 0 ~ 255 256 = 64 * 4 = 85 * 3 + 1
    uint row = index % 256;
    uint col = index / 256;
    
    //float u = clamp(value, 0, 1) * 0.25 + col * 0.25;
    //float v = 1 - ((row * 1.0 + 0.5) / 255);
    float2 uv = float2
    (
        clamp(value, 0, 1) * 0.25 + col * 0.25,
        1 - ((row * 1.0 + 0.5) / 255)
    );

    return SAMPLE_TEXTURE2D(_PixelartLUT, sampler_PixelartLUT, uv);
}

float4 SearchPixelartLUT(uint rawIndex, float shadowLevel, float value)
{
    return SearchPixelartLUT_RealIndex(GetRealPixelartLUTIndex(rawIndex, shadowLevel), value);
}

#endif // PIXELART_LUT_INCLUDED