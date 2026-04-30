#ifndef PIXELART_SHARED_INCLUDED
#define PIXELART_SHARED_INCLUDED

// BufferOutput

struct BufferOutput
{
    half4 depthNormal : COLOR0;
    float4 albedo : COLOR1;
    float2 smoothnessMetallic : COLOR2;
    float4 emission : COLOR3;
    float2 originUV : COLOR4;
    float4 pixelartProperties : COLOR5;
};

// Properties

// R通道是LUT索引

// G通道
struct PixelartPropertiesG
{
    int outline;
    int shadowType;
};

#define OUTLINE_NONE 0
#define OUTLINE_NORMAL 1

#define SHADOWTYPE_NORMAL 0
#define SHADOWTYPE_ORIGIN 1

inline float EncodeLUTIndex(uint lutIndex)
{
    return lutIndex * 1.0 / 1024.0;
}
inline uint DecodeLUTIndex(float value)
{
    return value * 1024;
}

inline float EncodePropertiesG(PixelartPropertiesG gProperties)
{
    // 0 ~ 255
    float g = gProperties.outline + gProperties.shadowType * 2;
    return g / 255.0;
}

inline float EncodePropertiesG(int outline, int shadowType)
{
    PixelartPropertiesG g;
    g.outline = outline;
    g.shadowType = shadowType;
    return EncodePropertiesG(g);
}

inline PixelartPropertiesG DecodePropertiesG(float rawG)
{
    PixelartPropertiesG output = (PixelartPropertiesG)0;
    int g = rawG * 255;
    output.outline = g % 2;
    output.shadowType = g - output.outline;
    return output;
}

// --End Properties

#define BUFFER_OUTPUT_INIT() \
BufferOutput __buffer_output = (BufferOutput)0; \
__buffer_output.pixelartProperties.w = 1; \


#define RETURN_BUFFER_VALUE() return __buffer_output

#define OUTPUT_DEPTHNORMAL(inDepth, inNormalWS) __buffer_output.depthNormal = EncodeDepthNormal(inDepth, inNormalWS)

//#define OUTPUT_NORMAL(inNormalWS) __buffer_output.normal = normalize(inNormalWS) * 0.5 + 0.5
#define OUTPUT_EMISSION(inEmission) __buffer_output.emission = inEmission

#define OUTPUT_ORIGIN_UV(inOriginUV) __buffer_output.originUV = inOriginUV

#define OUTPUT_ALBEDO(inAlbedo) __buffer_output.albedo = inAlbedo
#define OUTPUT_ALBEDO3(inAlbedo) OUTPUT_ALBEDO(float4(inAlbedo.rgb, 1))
#define OUTPUT_ALBEDO4(inAlbedo) OUTPUT_ALBEDO(inAlbedo.rgba)

#define OUTPUT_SMOOTHNESS_METALLIC(inSmoothness, inMetallic) __buffer_output.smoothnessMetallic = float2(inSmoothness, inMetallic)

#define OUTPUT_PROPERTIES(inLutIndex, inOutLine, inShadowType) \
__buffer_output.pixelartProperties.x = EncodeLUTIndex(inLutIndex); \
__buffer_output.pixelartProperties.y = EncodePropertiesG(inOutLine, inShadowType);

#define OUTPUT_SURFACE_TYPE(inMask) \
__buffer_output.pixelartProperties.z = inMask; \
__buffer_output.pixelartProperties.w = 1; // 我擦，不知道为什么一定要写这个

// --End BufferOutput


float4x4 _CameraMatrixV;
float4x4 _CameraMatrixVP;
float4x4 _CameraMatrixInvV;
float4x4 _CameraMatrixInvVP;

float _UnitSize;
uint _AdditionalLightCount;

#define PIXELART_CAMERA_MATRIX_V _CameraMatrixV
#define PIXELART_CAMERA_MATRIX_VP _CameraMatrixVP
#define PIXELART_CAMERA_MATRIX_I_V _CameraMatrixInvV
#define PIXELART_CAMERA_MATRIX_I_VP _CameraMatrixInvVP

#define UNITSNAP(coord, size) round(coord / size) * size

#include "Transform.hlsl"

// DepthNormal Helper

float3 DecodeNormal(half4 enc4)
{
    float kScale = 1.7777;
    float3 nn = enc4.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
    float g = 2.0 / dot(nn.xyz, nn.xyz);
    float3 n;
    n.xy = g * nn.xy;
    n.z = g - 1;
    return n;
}

float DecodeDepth(half4 enc4)
{
    float depth = enc4.z + enc4.w / 255.0;
    return depth;
}

half2 EncodeFloatRG(float v)
{
    half2 kEncodeMul = half2(1.0, 255.0);
    float kEncodeBit = 1.0/255.0;
    half2 enc = kEncodeMul * v;
    enc = frac (enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}

half2 EncodeViewNormalStereo(float3 n)
{
    float kScale = 1.7777;
    half2 enc;
    enc = n.xy / (n.z+1);
    enc /= kScale;
    enc = enc*0.5+0.5;
    return enc;
}

half4 EncodeDepthNormal(float depth, float3 normalWS)
{
    half4 enc;
    enc.xy = EncodeViewNormalStereo(NormalWorldToView(normalWS));
    enc.zw = EncodeFloatRG(depth);
    return enc;
}

// --End DepthNormal Helper


#endif // PIXELART_SHARED_INCLUDED