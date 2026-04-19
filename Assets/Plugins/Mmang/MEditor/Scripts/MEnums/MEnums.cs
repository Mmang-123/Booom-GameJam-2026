using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Mmang.Util;

namespace Mmang
{
    public static class MEnums
    {
        public class MEnumsInfo
        {
            public Dictionary<object, string> CustomNames = new();
            public HashSet<object> HiddenValues = new();
            public Dictionary<object, int> IndexMap = new();
        }

        private static Dictionary<Type, MEnumsInfo> s_InfosMap = new();

        public static MEnumsInfo GetInfo(Type type)
        {
            if (s_InfosMap.TryGetValue(type, out var result))
            {
                return result;
            }
            MEnumsInfo info = new();

            var values = Enum.GetValues(type);
            int index = 0;
            foreach (var value in values)
            {
                info.IndexMap.Add(value, index);
                var field = type.GetField(value.ToString());
                if (field != null && field.GetCustomAttribute<MEnumAttribute>() is { } enumAttribute)
                {
                    if (enumAttribute.hasCustomName)
                    {
                        info.CustomNames.Add(value, enumAttribute.enumName);
                    }

                    if (enumAttribute.hide)
                    {
                        info.HiddenValues.Add(value);
                    }
                }

                index++;
            }

            s_InfosMap.Add(type, info);
            return info;
        }

        public static MEnumsInfo GetInfo<T>() where T : Enum
        {
            return GetInfo(typeof(T));
        }

        public static int GetIndex(Type enumType, object value)
        {
            var info = GetInfo(enumType);

            if (info != null && info.IndexMap.TryGetValue(value, out var result))
            {
                return result;
            }

            return 0;
        }

        public static int GetIndex(Enum mEnum)
        {
            return GetIndex(mEnum.GetType(), mEnum);
        }

        public static string GetName(Type enumType, object value)
        {
            var info = GetInfo(enumType);

            if (info != null && info.CustomNames.TryGetValue(value, out var result))
            {
                return result;
            }

            return Enum.GetName(enumType, value);
        }

        public static string GetName(Enum mEnum)
        {
            return GetName(mEnum.GetType(), mEnum);
        }

        public static List<string> GetNames(Type enumType, bool disableHidden = false)
        {
            var result = new List<string>();

            var values = Enum.GetValues(enumType);
            var info = GetInfo(enumType);
            foreach (var value in values)
            {
                if (!disableHidden && info.HiddenValues.Contains(value))
                {
                    continue;
                }

                result.Add(GetName(enumType, value));
            }

            return result;
        }

        public static List<string> GetNames<T>(bool disableHidden = false) where T : Enum
        {
            return GetNames(typeof(T), disableHidden);
        }

        public static Dictionary<object, string> GetValueNameMap(Type enumType, bool disableHidden = false)
        {
            var result = new Dictionary<object, string>();

            var values = Enum.GetValues(enumType);
            var info = GetInfo(enumType);
            foreach (var value in values)
            {
                if (!disableHidden && info.HiddenValues.Contains(value))
                {
                    continue;
                }

                result.Add(value, GetName(enumType, value));
            }

            return result;
        }

        public static Dictionary<object, string> GetValueNameMap<T>(bool disableHidden = false) where T : Enum
        {
            return GetValueNameMap(typeof(T), disableHidden);
        }

        #region Extensions

        public static string GetMEnumName(this Enum mEnum)
        {
            return GetName(mEnum);
        }

        #endregion
    }
}