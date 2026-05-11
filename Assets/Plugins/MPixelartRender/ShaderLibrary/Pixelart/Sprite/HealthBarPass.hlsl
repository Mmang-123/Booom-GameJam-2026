
Varyings UnlitVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    const float UNIT_SIZE = 16.0 / 256.0;

    o.positionWS = TransformObjectToWorld(v.positionOS);
    o.positionOS = v.positionOS.xy;

    o.positionCS = TransformWorldToHClip(float4(o.positionWS, 1));

#ifdef TEXTURE_BASED
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#else
    o.uv = v.uv;
#endif
    o.color = v.color * _Color * _RendererColor;
    return o;
}

float4 HealthBarFrag(Varyings input) : SV_Target
{
    float4 outputColor = float4(input.color.rgb, 1);
    float u = input.uv.x;

    int segmentCount = _SegmentCount;
    int segmentPixelCount = 48;
    //segmentPixelCount = (pixelCount - (segmentCount - 1))
    int pixelCount = segmentCount * segmentPixelCount + (segmentCount - 1);

    int iu = floor(u * pixelCount);

    float cu = ceil(u * pixelCount) * 1.0 / pixelCount;
    
    if (input.color.a == 0)
        clip(-1);
    clip(input.color.a - cu + 0.01);

    //if (iu == 48 || iu == 97 || iu == 146)
    //    clip(-1);

    if ((iu + 1) % (segmentPixelCount + 1) == 0)
        clip(-1);

    return outputColor;
}