#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

#include "../Common/Math.hlsl"

//
float _Speed;
float _CloudScale;
float _CloudDark;
float _CloudLight;
float _CloudCover;
float _CloudAlpha;
float _SkyTint;
//
float4 _MainColor;
float4 _SkyColor1;
float4 _SkyColor2;


float _Rainy, _DayTime;
float4 _CenterFocusPosition;
float _CloudSize;

//
float2x2 m = float2x2(1.6, 1.2, -1.2, 1.6);


float2 hash( float2 p )
{
    p = float2(dot(p,float2(127.1,311.7)), dot(p,float2(269.5,183.3)));
    return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

float noise( in float2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;
    float2 i = floor(p + (p.x+p.y)*K1);	
    float2 a = p - i + (i.x+i.y)*K2;
    float2 o = (a.x>a.y) ? float2(1.0,0.0) : float2(0.0,1.0); //float2 of = 0.5 + 0.5*float2(sign(a.x-a.y), sign(a.y-a.x));
    float2 b = a - o + K2;
    float2 c = a - 1.0 + 2.0*K2;
    float3 h = max(0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
    float3 n = h*h*h*h*float3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
    return dot(n, float3(70.0,70.0,70.0));	
}

float fbm(float2 n)
{
    float2x2 m = float2x2(1.6,  1.2, -1.2,  1.6 );
    float total = 0.0, amplitude = 0.1;
    for (int i = 0; i < 8; i++)
    {
        total += noise(n) * amplitude;
        n = mul(m,n);
        amplitude *= 0.4;
    }
    return total;
}

real4 CloudShadowFrag(Varyings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 texUV = input.texcoord;

    texUV -= float2(0.5, 0.5);
    float3 worldPosition = float3(_CenterFocusPosition.x + texUV.x * _CloudSize, 0.0, _CenterFocusPosition.z + texUV.y * _CloudSize);

    float2x2 m = float2x2(1.6, 1.2, -1.2, 1.6);
    float time = _Time.x * _Speed;
    float2 sourceUV = worldPosition.xz / 10.0;
    //float2 sourceUV = input.texcoord * 3;
    //sourceUV = mul(m,sourceUV);
    //float2 sourceUV = IN.uv;
    float2 uv = sourceUV;
    float q = fbm(uv * _CloudScale * 0.5);

    //return real4(noise(mul(m,uv)+time * _CloudScale) * float3(1.0,1.0,1.0),1.0);
    
    //ridged noise shape
    float r = 0.0;
    uv *= _CloudScale;
    uv -= q - time;
    float weight = 0.8;
    for (int i=0; i<7; i++)
    {
        r += abs(weight*noise( uv ));
        uv = mul(m ,uv) + time;
        weight *= 0.7;
    }
    //return real4(noise(uv * _CloudScale) * float3(1.0,1.0,1.0),1.0);
    //return float4(r,r,r,1.0);

    //noise shape
    float f = 0.0;
    uv = sourceUV;
    uv *= _CloudScale;
    uv -= q - time;
    weight = 0.5;
    for (int i=0; i<7; i++)
    {
        f += weight*noise( uv );
        uv = mul(m ,uv) + time;
        weight *= 0.6;
    }

    f *= r + f;

    //noise colour
    float c = 0.0;
    time = _Time.x * _Speed * 2.0;
    uv = sourceUV;
    uv *= _CloudScale*2.0;
    uv -= q - time;
    weight = 0.4;
    for (int i=0; i<7; i++)
    {
        c += weight*noise( uv );
        uv = mul(m ,uv) + time;
        weight *= 0.6;
    }

    //noise ridge colour
    float c1 = 0.0;
    time = _Time.x * _Speed * 3.0;
    uv = sourceUV;
    uv *= _CloudScale*3.0;
    uv -= q - time;
    weight = 0.4;
    for (int i=0; i<7; i++)
    {
        c1 += abs(weight*noise( uv ));
        uv = mul(m ,uv) + time;
        weight *= 0.6;
    }

    c += c1;

    //float3 skyColor = lerp(_SkyColor2, _SkyColor1, IN.uv.y);
    float3 cloudColor = float3(1.0, 1.0, 1.0) * clamp((_CloudDark + _CloudLight*c), 0.0, 1.0);

    f = lerp(-0.2, 1, _Rainy) + _CloudAlpha*f*r;

    //float3 result = lerp(skyColor, clamp(_SkyTint * skyColor + cloudColor, 0.0, 1.0), clamp(f + c, 0.0, 1.0));
    float3 result = lerp(float3(0.0, 0.0, 0.0), clamp(cloudColor, 0.0, 1.0), clamp(f + c, 0.0, 1.0));
    result.x = 1 - result.x;
    
    //时间
    _DayTime = clamp(_DayTime, 0.35, 0.47); 
    result.x = lerp(result.x, 1, (_DayTime - 0.35) / 0.12);

    //return result;

    
    //if (result.x <= 0.5)
    //    result = float3(0.25, 0.25, 0.25);
    //else if(result.x <= 0.7)
    //    result = float3(0.5, 0.5, 0.5);
    //else
    //    result = float3(1, 1, 1);
    
    //
    return float4(result.xxx, 1);
}