#include "../Cloud/CloudTexture.hlsl"
#include "../Common/Math.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"


Varyings GodRayVert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.positionCS = TransformObjectToHClip(input.positionOS);
    output.positionWS = TransformObjectToWorld(input.positionOS);

    output.uv = input.uv;

    return output;
}

half4 GodRayFrag(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float3 fadeParams = UNITY_ACCESS_INSTANCED_PROP(Props, _FadeParams);

    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    float3 outputColor = _BaseColor.rgb;
    float height = input.positionWS.y;
    float heightFade = saturate(max(0, height - _BottomY) / _Height);
    
    float distanceFade = fadeParams.x;
    float spacingFade = clampRemap(fadeParams.y, 0, 2, 0, 1);
    spacingFade = sin(spacingFade * 3.1415926);
    float diffuseFade = saturate(fadeParams.z);
    
    float alpha = _BaseColor.a * heightFade * sqrt(distanceFade) * spacingFade;
    //float alpha = _BaseColor.a * heightFade;

    //return float4(distanceFade.xxx, 1);

    Light mainLight = GetMainLight(shadowCoord);
    mainLight.distanceAttenuation = 1.0;
    mainLight = ApplyToonCloudShadow(mainLight, input.positionWS);
    
    outputColor *= mainLight.color.rgb;

    float shadow = mainLight.shadowAttenuation;
    //if (shadow < 0.8)
    //    return float4(0, 0, 0, 0);
    clip(shadow - 0.8);

    return half4(outputColor.rgb * alpha, 1);
}