using System;
using UnityEngine;

namespace Mmang
{
    /// <summary>
    /// 展示MEnums
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MEnumsAttribute : PropertyAttribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class MEnumAttribute : Attribute
    {
        public bool hasCustomName;
        public string enumName;
        public bool hide = false;

        public MEnumAttribute()
        {
            hasCustomName = false;
            enumName = string.Empty;
        }

        public MEnumAttribute(string enumName)
        {
            hasCustomName = true;
            this.enumName = enumName;
        }
    }
}