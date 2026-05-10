

float4 HealthBarFrag(Varyings input) : SV_Target
{
    float4 outputColor = float4(input.color.rgb, 1);
    float u = input.uv.x;

    int segmentCount = 3;
    int pixelCount = segmentCount * 48 + (segmentCount - 1);

    int iu = floor(u * pixelCount);

    float fu = iu * 1.0 / pixelCount;
    
    if (input.color.a < 1)
        clip(input.color.a - fu - 0.01);

    if (iu == 48 || iu == 97 || iu == 146)
        clip(-1);

    return outputColor;
}