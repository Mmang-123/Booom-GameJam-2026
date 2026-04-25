#ifndef M_LIGHTING_INCLUDED
#define M_LIGHTING_INCLUDED

TEXTURE2D(_MLightingTexture);
SAMPLER(sampler_MLightingTexture);

float3 SampleLight(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_MLightingTexture, sampler_MLightingTexture, screenUV).rgb;
}

#endif
