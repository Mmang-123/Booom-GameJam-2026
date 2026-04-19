using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Generations
{

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class GenerationPointStructAttribute : Attribute
    {
        public int size;
        public GenerationPointStructAttribute(int size)
        {
            this.size = size;
        }
    }

    public interface IGenerationPointStruct
    {

    }

}