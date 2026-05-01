#ifndef SPRITE_BACKGROUND_INCLUDED
#define SPRITE_BACKGROUND_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float3 SampleBackground(float2 screenUV)
{
    float3 backgroundColor = float3(0.0313, 0.0313, 0.1215);

    return SRGBToLinear(backgroundColor);
}

#endif