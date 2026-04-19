using System;

namespace Mmang
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SOComponentContainerAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SOComponentAttribute : Attribute
    {
        public string Name;
        public SOComponentAttribute(string name)
        {
            Name = name;
        }
    }
}