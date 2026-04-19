using System;
using UnityEngine;

namespace Mmang
{

    public class VariableTypeAttribute : PropertyAttribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class VariableTypeDefineAttribute : TypeCollectionAttribute
    {
        public bool CanBeNull = false;
        public bool RequireFlag = false;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VariableTypeFlagAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VariableTypeNameAttribute : VariableTypeFlagAttribute
    {
        public string Name;
        public VariableTypeNameAttribute(string name)
        {
            Name = name;
        }
    }
}