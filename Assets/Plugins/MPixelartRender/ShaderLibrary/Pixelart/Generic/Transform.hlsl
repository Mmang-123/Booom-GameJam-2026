#ifndef PGENERIC_TRANSFORM_INCLUDED
#define PGENERIC_TRANSFORM_INCLUDED

float3 NormalViewToWorld(float3 normalVS)
{
    float4x4 viewTranspose = transpose(PIXELART_CAMERA_MATRIX_V);
    float3 normalWS = mul(viewTranspose, float4(normalVS, 0)).xyz;
    return normalWS;
}

float3 NormalWorldToView(float3 normalWS)
{
    float3 normalVS = mul(PIXELART_CAMERA_MATRIX_V, float4(normalWS, 0)).xyz;
    return normalVS;
}

float3 GetWorldPositionWithRawDepth(float2 uv, float sceneRawDepth)
{
    float3 worldPos;
    #if defined(UNITY_REVERSED_Z)
        sceneRawDepth = 1 - sceneRawDepth;
    #endif

    if(unity_OrthoParams.w)
    {
        float sceneDepthVS = lerp(_ProjectionParams.y, _ProjectionParams.z, sceneRawDepth);
        float2 viewRayEndPosVS_xy = float2(unity_OrthoParams.xy * (uv * 2.0 - 1.0));
        float3 posVSOrtho = float3(viewRayEndPosVS_xy, -sceneDepthVS);
        
        worldPos = mul(PIXELART_CAMERA_MATRIX_I_V, float4(posVSOrtho, 1)).xyz;
    }
    else
    {
        float4 ndc = float4(uv * 2.0 - 1.0, sceneRawDepth * 2.0 - 1.0, 1);
        float4 pos = mul(PIXELART_CAMERA_MATRIX_I_VP, ndc);
        worldPos = pos.xyz / pos.w;
    }

    return worldPos;
}

#endif // PGENERIC_TRANSFORM_INCLUDED