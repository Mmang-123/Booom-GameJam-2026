using System;
using UnityEngine;

namespace Mmang.Game
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GAMAttribute : VariableTypeNameAttribute
    {
        public float DefaultValue;

        public GAMAttribute(string name, float defaultValue = 0f)
            : base(name)
        {
            DefaultValue = defaultValue;
        }
    }
}