using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Mmang.Util;

namespace Mmang.Editors
{
    [InitializeAfterTypeCollection]
    public static class VariableTypeManager
    {
        public class VariableTypeInfo
        {
            public List<Type> SubTypes = new();
            public List<string> SubTypeNames = new();
        }

        private static Dictionary<Type, VariableTypeInfo> m_Map = new();

        public static void Init()
        {
            m_Map.Clear();

            var collectionClasses = TypeCollectionManager.GetTypeList<VariableTypeDefineAttribute>();
            foreach (Type c in collectionClasses)
            {
                if (m_Map.ContainsKey(c))
                {
                    Debug.LogWarning($"VariableType 存在重复类型 {c}");
                    continue;
                }

                VariableTypeInfo info = new();

                var defineAttribute = c.GetCustomAttribute<VariableTypeDefineAttribute>();
                if (defineAttribute.CanBeNull)
                {
                    info.SubTypes.Add(null);
                    info.SubTypeNames.Add("Null");
                }

                List<Type> subTypes = new();
                if (!c.IsAbstract)
                    subTypes.Add(c);
                subTypes.AddRange(c.GetSubClasses());
                
                foreach (var subClass in subTypes)
                {
                    var nameAttribute = subClass.GetCustomAttribute<VariableTypeNameAttribute>();
                    
                    if (defineAttribute.RequireFlag
                    && nameAttribute == null
                    && subClass.GetCustomAttribute<VariableTypeFlagAttribute>() == null)
                    {
                        continue;
                    }
                    
                    string className = nameAttribute != null
                        ? nameAttribute.Name
                        : subClass.Name;
                    
                    info.SubTypes.Add(subClass);
                    info.SubTypeNames.Add(className);
                }

                //
                m_Map.Add(c, info);
            }
        }

        public static VariableTypeInfo GetVariableTypeInfo<T>()
            => GetVariableTypeInfo(typeof(T));
        public static VariableTypeInfo GetVariableTypeInfo(Type type)
        {
            if (m_Map.TryGetValue(type, out var info))
                return info;
            return null;
        }

        public static Dictionary<int, string> GetNameMap(this VariableTypeInfo info)
        {
            Dictionary<int, string> map = new();

            int count = info.SubTypes.Count;
            for (int i = 0; i < count; i++)
            {
                map.Add(i, info.SubTypeNames[i]);
            }

            return map;
        }
    }
}