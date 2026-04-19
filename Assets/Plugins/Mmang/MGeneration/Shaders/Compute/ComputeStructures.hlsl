
struct NormalControlPoint
{
    float3 positionWS;
    float3 normalWS;
};

struct QuadControlPoint
{
    float3 positionWS;
    float3 normalWS;
    float quadSize;
    float rotateAngle;
    int type;
};

struct NormalGenerationPoint
{
    float3 originPositionWS;
    float3 positionOS;
    float3 normalWS;
    float2 uv;
};

struct VarietyGenerationPoint
{
    float3 originPositionWS;
    float3 positionOS;
    float3 normalWS;
    float2 uv;
    int type;
};

struct InteractionData
{
    float3 position;
    float radius;
};