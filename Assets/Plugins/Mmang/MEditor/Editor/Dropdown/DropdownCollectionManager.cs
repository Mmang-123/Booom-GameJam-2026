using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Editors
{
    [InitializeAfterTypeCollection]
    public static class DropdownCollectionManager
    {
        private static Dictionary<Type, Dictionary<int, string>> m_TypeToInfoMap = new();

        public static void Init()
        {
            var collectionClasses = TypeCollectionManager.GetTypeList<DropdownCollectionAttribute>();
            foreach (Type c in collectionClasses)
            {
                var fields = c.GetFields();
                Dictionary<int, string> map = new();
                foreach (var field in fields)
                {
                    if (!field.IsStatic || field.FieldType != typeof(int))
                        continue;
                    string name = field.Name;
                    int value = (int)field.GetValue(null);
                    map.Add(value, name);
                }
                m_TypeToInfoMap.Add(c, map);
            }
        }

        public static Dictionary<int, string> GetMap(Type type)
        {
            if (m_TypeToInfoMap.TryGetValue(type, out var result))
                return result;
            return null;
        }
    }
}