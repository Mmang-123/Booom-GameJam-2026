using System;
using UnityEngine;

namespace Mmang
{

    public enum EGlobalConfigType
    {
        Single, Reference
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MGlobalConfig : TypeCollectionAttribute
    {
        public override Type BaseType => typeof(ScriptableObject);
        public string configName;
        public EGlobalConfigType type = EGlobalConfigType.Single;

        public int order = 0;
    }


}
