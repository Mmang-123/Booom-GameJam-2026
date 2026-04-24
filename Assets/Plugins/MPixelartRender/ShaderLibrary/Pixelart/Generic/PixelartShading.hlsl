#ifndef PIXELART_SHADING_INCLUDED
#define PIXELART_SHADING_INCLUDED
#include "Lighting.hlsl"
#include "Transform.hlsl"
#include "PixelartLUT.hlsl"
#include "Outline.hlsl"
#include "../../Cloud/CloudTexture.hlsl"

float3 MainLightShading(Light light, float3 normalWS, float3 positionWS, float metallic, uint lutIndex)
{
    float ndotl = dot(normalWS, light.direction);
    ndotl = saturate((ndotl + 1) / 2);
    
    float kd = ndotl * (1.0 - metallic * 0.3);

    float shadow = step(0.25, light.shadowAttenuation);
    float cloudShadow = GetToonCloudShadow(light.direction, positionWS);
    shadow *= cloudShadow;

    // LUT
    float3 col = SearchPixelartLUT(lutIndex, 1 - shadow, kd).rgb;

    return col;
}

float3 MainLightShading_WithOutline(Light light, float3 normalWS, float3 positionWS, float2 screenUV, float metallic, uint lutIndex)
{
    float ndotl = dot(normalWS, light.direction);
    ndotl = saturate((ndotl + 1) / 2);
    
    float kd = ndotl * (1.0 - metallic * 0.3);

    float shadow = step(0.25, light.shadowAttenuation);
    float cloudShadow = GetToonCloudShadow(light.direction, positionWS);
    shadow *= cloudShadow;

    // Outline
    half4 outline = GetOutline(screenUV, 0.04, 1);

    // 逆光时，深度差别大的描边拉高diffuse
    if (ndotl <= 0.4 && outline.z >= 0.5)
    {
        ndotl = clamp(ndotl + 0.35, 0, 0.75);
    }

    // LUT
    float3 col = SearchPixelartLUT(lutIndex, 1 - shadow, kd).rgb;

    if (outline.x + outline.y >= 1)
    {
        if (ndotl > 0.5 || outline.x >= 1)
        {
            col *= 1.25;
        }
        else
        {
            col *= 0.875;
        }
    }

    return col;
}

float3 DiffuseShading(Light light, float3 normalWS)
{
    float ndotl = dot(normalWS, light.direction);
    ndotl *= light.shadowAttenuation;
    ndotl *= light.distanceAttenuation;
    ndotl = saturate(ndotl);
    return ndotl * light.color;
}

float RoundStep(float inValue, int step)
{
    return floor((inValue) * step) / step;
}

float3 SpecularShading(Light light, float3 albedo, float3 normalWS, float3 positionWS, float3 viewDir, float smoothness, float metallic)
{
    float3 lightDir = light.direction;
    float roughness = 1.0 - smoothness;
    float3 halfVector = normalize(lightDir + viewDir);
    
    // temp
    float shadow = step(0.25, light.shadowAttenuation);
    float cloudShadow = GetToonCloudShadow(light.direction, positionWS);
    shadow *= cloudShadow;

    float NdotL = max(0, dot(normalWS, lightDir));
    float NdotV = max(0, dot(normalWS, viewDir));
    float NdotH = max(0, dot(normalWS, halfVector));
    float HdotV = max(0, dot(halfVector, viewDir));
    
    // 2. 计算F0（基础反射率）
    float3 dielectricF0 = 0;
    float3 F0 = lerp(dielectricF0, albedo, metallic);
    
    // 3. 计算Cook-Torrance BRDF各项
    // D项：GGX分布
    float alpha = roughness * roughness;
    float alpha2 = alpha * alpha;
    float NdotH2 = NdotH * NdotH;
    float denomD = NdotH2 * (alpha2 - 1.0) + 1.0;
    float D = alpha2 / (PI * denomD * denomD);
    
    // G项：Smith-Schlick-GGX
    float k = (roughness + 1.0) * (roughness + 1.0) / 8.0;
    float G = (NdotV / (NdotV * (1.0 - k) + k)) * 
              (NdotL / (NdotL * (1.0 - k) + k));
    
    // F项：Schlick菲涅尔
    float3 F = F0 + (1.0 - F0) * pow(1.0 - HdotV, 5.0);
    
    // 4. 计算Cook-Torrance BRDF
    float3 specularBRDF = (D * G * F) / (4.0 * NdotV * NdotL + 0.001);

    return specularBRDF * light.color.rgb * shadow;
}


#endif // PIXELART_SHADING_INCLUDED