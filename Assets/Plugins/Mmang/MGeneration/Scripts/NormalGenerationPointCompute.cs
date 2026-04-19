using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Generations
{
    /*
        通用控制点类型 只包含坐标和法线
    */

    [Serializable]
    [ControlPointStruct(sizeof(float) * (3 + 3))]
    public struct NormalControlPoint : IControlPointStruct
    {
        public Vector3 positionWS;
        public Vector3 normalWS;
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

    [GenerationPointStruct(sizeof(float) * (2 + 3 + 3 + 3))]
    public struct NormalGenerationPoint : IGenerationPointStruct
    {
        public Vector2 uv;
        public Vector3 originPositionWS;
        public Vector3 positionOS;
        public Vector3 normalWS;
    }

    public class NormalGenerationPointCompute : GenerationPointComputeBase<NormalControlPoint, NormalGenerationPoint>
    {
        
    }
}