using System;
using UnityEngine;

namespace Mmang
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DropdownAttribute : PropertyAttribute
    {
        public Type collection;
        public string funcName;
        
        public DropdownAttribute(Type collectionType)
        {
            collection = collectionType;
        }

        public DropdownAttribute(string getMapFuncName)
        {
            funcName = getMapFuncName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DropdownCollectionAttribute : TypeCollectionAttribute
    {

    }
}