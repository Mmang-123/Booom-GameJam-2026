#ifndef SPRITE_BACKGROUND_INCLUDED
#define SPRITE_BACKGROUND_INCLUDED

float3 SampleBackground(float2 screenUV)
{
    float3 backgroundColor = float3(0.0313, 0.0313, 0.1215);

    return SRGBToLinear(backgroundColor);
}

#endif