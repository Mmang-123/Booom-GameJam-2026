using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Mmang.Util;

namespace Mmang.Editors
{
    [InitializeOnLoad]
    public static class TypeCollectionManager
    {
        public class Collection
        {
            public Type OwnerAttributeType;
            public List<Type> List = new();

            public void Dispose()
            {
                OwnerAttributeType = null;
                List.Clear();
            }
        }

        public static List<Collection> Collections { get; private set; } = new();

        private static Collection GetTypeCollection(TypeCollectionAttribute attribute)
        {
            Type attributeType = attribute.GetType();
            foreach (var c in Collections)
            {
                if (c.OwnerAttributeType == attributeType)
                    return c;
            }
            var newCollection = new Collection()
            {
                OwnerAttributeType = attributeType
            };
            Collections.Add(newCollection);
            return newCollection;
        }

        public static List<Type> GetTypeList(Type ownerType)
        {
            foreach (var c in Collections)
            {
                if (c.OwnerAttributeType == ownerType)
                    return c.List;
            }
            return new(0);
        }

        public static List<Type> GetTypeList<T>() where T : TypeCollectionAttribute
        {
            foreach (var c in Collections)
            {
                if (c.OwnerAttributeType == typeof(T))
                    return c.List;
            }
            return new(0);
        }

        // 初始化
        static TypeCollectionManager()
        {
            Collections.Clear();

            var classes = ReflectionHelper.FindClassesWithAttribute<TypeCollectionAttribute>();
            List<Type> initClasses = new();
            foreach (var c in classes)
            {
                var attributes = c.GetCustomAttributes<TypeCollectionAttribute>();
                foreach (var attribute in attributes)
                {
                    if (attribute is InitializeAfterTypeCollectionAttribute)
                    {
                        //UnityEngine.Debug.Log("Init: " + attribute);
                        initClasses.Add(c);
                        continue;
                    }

                    if (attribute.BaseType != null && !attribute.BaseType.IsAssignableFrom(c))
                    {
                        UnityEngine.Debug.Log(c + "未继承自" + attribute.BaseType + ", 请勿使用" + attribute.GetType());
                        continue;
                    }

                    var collection = GetTypeCollection(attribute);
                    collection.List.Add(c);
                    //UnityEngine.Debug.Log(collection.OwnerAttributeType + ": " + c);
                }
            }

            // 集合初始化方法执行
            foreach (var c in initClasses)
            {
                var initMethod = c.GetMethod("Init");
                if (initMethod == null || !initMethod.IsStatic)
                    continue;
                initMethod.Invoke(null, null);
            }
        }

        public static void ReleaseCollection<T>() where T : TypeCollectionAttribute
            => ReleaseCollection(typeof(T));

        public static void ReleaseCollection(Type targetType)
        {
            int count = Collections.Count;
            for (int i = 0; i < count; i++)
            {
                var c = Collections[i];
                if (c.OwnerAttributeType == targetType)
                {
                    Collections[i].Dispose();
                    Collections.RemoveAt(i);
                    return;
                }
            }
        }

    }
}