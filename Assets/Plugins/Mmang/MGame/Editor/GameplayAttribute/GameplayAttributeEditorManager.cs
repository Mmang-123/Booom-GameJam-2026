using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mmang.Editors;
using UnityEngine;

namespace Mmang.Game.Editors
{
    /*
    [InitializeAfterTypeCollection]
    public static class GameplayAttributeEditorManager
    {
        private static Dictionary<string, System.Type> s_TypeMap; 

        public static void Init()
        {
            s_TypeMap ??= new();
            s_TypeMap.Clear();

            var types = TypeCollectionManager.GetTypeList<DefineGAMAttribute>();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<DefineGAMAttribute>();
                if (attribute != null)
                {
                    if (s_TypeMap.ContainsKey(attribute.Name))
                    {
                        Debug.LogWarning($"存在冲突的属性命名: {attribute.Name} 冲突类型: {type}  {s_TypeMap[attribute.Name]}");
                        continue;
                    }

                    s_TypeMap.Add(attribute.Name, type);
                }
            }
        }

        public static bool TryGetAttributeType(string attributeName, out System.Type outType)
        {
            if (s_TypeMap == null)
            {
                outType = null;
                return false;
            }

            return s_TypeMap.TryGetValue(attributeName, out outType);
        }

        public static System.Type GetAttributeType(string attributeName)
        {
            if (s_TypeMap == null || !s_TypeMap.ContainsKey(attributeName))
            {
                return null;
            }
            
            return s_TypeMap[attributeName];
        }

        public static List<string> GetAttributes()
        {
            if (s_TypeMap == null)
            {
                return null;
            }

            return s_TypeMap.Keys.ToList();
        }
    }
    */
}