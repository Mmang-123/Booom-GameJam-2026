using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Generations
{
    /*
        四边形控制点类型 额外包含旋转、大小和种类信息
    */

    [System.Serializable]
    [ControlPointStruct(sizeof(float) * (3 + 3 + 1 + 1) + sizeof(int))]
    public struct QuadControlPoint : IControlPointStruct
    {
        public Vector3 positionWS;
        public Vector3 normalWS;
        public float quadSize;
        public float rotateAngle;
        public int type;
        public readonly Vector3 Position => positionWS;
        public readonly Vector3 Normal => normalWS;
        public void SetPosition(Vector3 position)
        {
            positionWS = position;
        }
        public void SetNormal(Vector3 normal)
        {
            normalWS = normal;
        }
    }

    [GenerationPointStruct(sizeof(float) * (2 + 3 + 3 + 3) + sizeof(int))]
    public struct VarietyGenerationPoint : IGenerationPointStruct
    {
        public Vector2 uv;
        public Vector3 originPositionWS;
        public Vector3 positionOS;
        public Vector3 normalWS;
        public int type;
    }

    [AdditionalSetting(typeof(QuadGenerationPointAdditionalSetting))]
    public class QuadGenerationPointCompute : GenerationPointComputeBase<QuadControlPoint, VarietyGenerationPoint>
    {
        protected override void ApplyAdditionalSetting(GenerationPointAdditionalSetting setting, ref QuadControlPoint newStruct)
        {
            var quadSetting = setting as QuadGenerationPointAdditionalSetting;
            newStruct.quadSize = RandomUtil.GetRandomValueInRange(quadSetting.QuadSizeRange);
            newStruct.rotateAngle = RandomUtil.GetRandomValueInRange(quadSetting.RotateAngleRange);
            newStruct.type = RandomUtil.GetRandomValueInRange(quadSetting.TypeRange);
        }
    }

    [System.Serializable]
    public class QuadGenerationPointAdditionalSetting : GenerationPointAdditionalSetting
    {
        public Vector2 QuadSizeRange;
        public Vector2 RotateAngleRange;
        public Vector2Int TypeRange;
    }
}