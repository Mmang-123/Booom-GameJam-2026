float multiStep(float value, float level, float minValue, float offset)
{
    if(level <= 1.0) return 1.0;
    
    float curLevel = value * level;
    curLevel = floor(curLevel + offset);
    
    float curOffset = curLevel / (level - 1.0);
    curLevel += lerp(minValue, 1.0, curOffset);
    curLevel = curLevel / level;
    
    return saturate(curLevel);
}

float remap(float value, float rawX, float rawY, float targetX, float targetY)
{
    return targetX + (value - rawX) / (rawY - rawX) * (targetY - targetX);
}

float clampRemap(float value, float rawX, float rawY, float targetX, float targetY)
{
    return remap(clamp(value, rawX, rawY), rawX, rawY, targetX, targetY);
}

float3 GetVerticalDirection(float3 direction)
{
    if (direction.z == 0)
        return float3(0, 0, 1);
    else
        return normalize(float3(-direction.z / direction.x, 0, 1));
}
