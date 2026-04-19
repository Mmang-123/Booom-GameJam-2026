using System.Collections.Generic;
using Mmang.Util;

namespace Mmang.Game
{
    public static class PropertyUtil
    {

        public static bool MatchType(this PropertyBase property, System.Type type)
        {
            return property.PropertyType == type;
        }

        public static System.Type GetGettablePropertyType<T>(this IGettableProperty<T> gettableProperty)
        {
            return typeof(T);
        }

        public static System.Type GetSettablePropertyType<T>(this ISettableProperty<T> settableProperty)
        {
            return typeof(T);
        }

        public static List<Property<T>> GetTargetTypeProperties<T>(this PropertyContainer propertyContainer)
            => GetTargetTypeProperties<T>(propertyContainer.Properties);

        public static List<Property<T>> GetTargetTypeProperties<T>(this List<Property> source)
        {
            var result = new List<Property<T>>();
            foreach (var property in source)
            {
                if (property is Property<T> propertyT)
                    result.Add(propertyT);
            }
            return result;
        }

        public static void SelectTargetTypeProperties<T>(this PropertyContainer propertyContainer, List<Property<T>> result)
            => SelectTargetTypeProperties(propertyContainer.Properties, result);
        public static void SelectTargetTypeProperties<T>(this PropertyContainer propertyContainer, PropertyContainer result)
            => SelectTargetTypeProperties<T>(propertyContainer.Properties, result);
        public static void SelectTargetTypeProperties<T>(this List<Property> source, List<Property<T>> result)
        {
            foreach (var property in source)
            {
                if (property is Property<T> propertyT)
                    result.Add(propertyT);
            }
        }
        public static void SelectTargetTypeProperties<T>(this List<Property> source, PropertyContainer result)
        {
            foreach (var property in source)
            {
                if (property is Property<T> propertyT)
                    result.Add(propertyT);
            }
        }

        public static void SelectTargetTypeProperties<T1, T2>(this PropertyContainer propertyContainer, List<Property<T1>> result1, List<Property<T2>> result2)
            => SelectTargetTypeProperties(propertyContainer.Properties, result1, result2);
        public static void SelectTargetTypeProperties<T1, T2>(this PropertyContainer propertyContainer, PropertyContainer result1, PropertyContainer result2)
            => SelectTargetTypeProperties<T1, T2>(propertyContainer.Properties, result1, result2);
        public static void SelectTargetTypeProperties<T1, T2>(this List<Property> source, List<Property<T1>> result1, List<Property<T2>> result2)
        {
            foreach (var property in source)
            {
                if (property is Property<T1> propertyT1)
                    result1.Add(propertyT1);
                if (property is Property<T2> propertyT2)
                    result2.Add(propertyT2);
            }
        }
        public static void SelectTargetTypeProperties<T1, T2>(this List<Property> source, PropertyContainer result1, PropertyContainer result2)
        {
            foreach (var property in source)
            {
                if (property is Property<T1> propertyT1)
                    result1.Add(propertyT1);
                if (property is Property<T2> propertyT2)
                    result2.Add(propertyT2);
            }
        }

        public static void SelectTargetTypeProperties<T1, T2, T3>(this PropertyContainer propertyContainer, List<Property<T1>> result1, List<Property<T2>> result2, List<Property<T3>> result3)
            => SelectTargetTypeProperties(propertyContainer.Properties, result1, result2, result3);
        public static void SelectTargetTypeProperties<T1, T2, T3>(this PropertyContainer propertyContainer, PropertyContainer result1, PropertyContainer result2, PropertyContainer result3)
            => SelectTargetTypeProperties<T1, T2, T3>(propertyContainer.Properties, result1, result2, result3);
        public static void SelectTargetTypeProperties<T1, T2, T3>(this List<Property> source, List<Property<T1>> result1, List<Property<T2>> result2, List<Property<T3>> result3)
        {
            foreach (var property in source)
            {
                if (property is Property<T1> propertyT1)
                    result1.Add(propertyT1);
                if (property is Property<T2> propertyT2)
                    result2.Add(propertyT2);
                if (property is Property<T3> propertyT3)
                    result3.Add(propertyT3);  
            }
        }
        public static void SelectTargetTypeProperties<T1, T2, T3>(this List<Property> source, PropertyContainer result1, PropertyContainer result2, PropertyContainer result3)
        {
            foreach (var property in source)
            {
                if (property is Property<T1> propertyT1)
                    result1.Add(propertyT1);
                if (property is Property<T2> propertyT2)
                    result2.Add(propertyT2);
                if (property is Property<T3> propertyT3)
                    result3.Add(propertyT3);  
            }
        }
    }
}