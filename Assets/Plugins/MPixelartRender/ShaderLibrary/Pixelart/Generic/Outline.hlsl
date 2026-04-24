#ifndef G_OUTLINE_INCLUDED
#define G_OUTLINE_INCLUDED

#include "PixelartBuffer.hlsl"
#include "Transform.hlsl"

half4 GetOutline(float2 screenUV, float normalSensitivity, float depthSensitivity)
{
    half depth[4];
    half2 normal[4];
    float2 uv[4];
    //
    float2 sourceNormal = PB_GetNormal2(screenUV).xy;
    float sourceDepth = PB_GetDepth2_NoReversed(screenUV);

    //
    float2 texSize = 1.0 / _ScreenParams.xy;
    uv[0] = screenUV + float2(-texSize.x, 0);
    uv[1] = screenUV + float2(texSize.x, 0);
    uv[2] = screenUV + float2(0, -texSize.y);
    uv[3] = screenUV + float2(0, texSize.y);

    //
    float depDiff = 0.0;
    float norDiff = 0.0;
    float2 norDiffV2 = float2(0.0, 0.0);
    float3 normalEdgeBias = float3(1.0, 1.0, 1.0);
    //
    for (int t = 0; t < 4; t++)
    {
        normal[t] = PB_GetNormal2(uv[t]).xy;
        depth[t] = PB_GetDepth2_NoReversed(uv[t]);

        //
        float normalIndicator = clamp(smoothstep(-.01, .01, dot(sourceNormal - normal[t], normalEdgeBias.xy)), 0.0, 1.0);
        float depthIndicator = clamp(sign((depth[t] - sourceDepth) * .25 + .0025), 0.0, 1.0);
        norDiffV2 += abs(sourceNormal - normal[t]) * depthIndicator * (1 - normalIndicator);

        //
        depDiff += clamp(depth[t] - sourceDepth, 0.0, 1.0);
    }

    //
    norDiffV2 *= normalSensitivity;
    norDiff = (norDiffV2.x + norDiffV2.y) > 0.01 ? 1 : 0;
    // 深度差异更大的描边
    float depDiff2 = floor(smoothstep(0.01, 0.02, depDiff * depthSensitivity / 4) * 2.0) / 2.0;
    depDiff = floor(smoothstep(0.01, 0.02, depDiff * depthSensitivity) * 2.0) / 2.0;

    //
    if (depDiff > 0.0)
        return half4(1, 0, depDiff2, 1);
    if (norDiff > 0.0)
        return half4(0, 1, depDiff2, 1);


    return half4(0, 0, depDiff2, 1);

}

#endif // G_OUTLINE_INCLUDED