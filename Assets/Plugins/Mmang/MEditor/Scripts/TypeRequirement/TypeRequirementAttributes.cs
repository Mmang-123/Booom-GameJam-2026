using System;

namespace Mmang
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AllowedTypesAttribute : Attribute
    {
        public Type[] Types { get; }

        public AllowedTypesAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}