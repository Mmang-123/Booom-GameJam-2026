using System;

namespace Mmang
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class TypeCollectionAttribute : Attribute
    {
        public virtual Type BaseType { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitializeAfterTypeCollectionAttribute : TypeCollectionAttribute
    {

    }
}