using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Generations
{

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ControlPointStructAttribute : Attribute
    {
        public int size;
        public ControlPointStructAttribute(int size)
        {
            this.size = size;
        }
    }

    public interface IControlPointStruct
    {
        public void SetPosition(Vector3 position);
        public void SetNormal(Vector3 normal);
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
    }

}