// <author>
//   douduck08: https://github.com/douduck08
//   Use Reflection to get instance of Unity's SerializedProperty in Custom Editor.
//   Modified codes from 'Unity Answers', in order to apply on nested List<T> or Array.
//
//   Original author: HiddenMonk & Johannes Deml
//   Ref: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
// </author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Mmang.Util
{
    public static class SerializedPropertyExtension
    {
        private static readonly Regex rgx = new(@"\[\d+\]", RegexOptions.Compiled);
        private static readonly Regex indexRgx = new(@"\[(\d+)\]", RegexOptions.Compiled);

        public static bool EqualContents(this SerializedProperty property1, SerializedProperty property2)
        {
            return SerializedProperty.EqualContents(property1, property2);
        }

        public static object GetValue(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = int.Parse(indexRgx.Match(fieldStructure[i]).Groups[1].Value);
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            return obj;
        }

        public static T GetValue<T>(this SerializedProperty property)
        {
            return (T)GetValue(property);
        }

        public static object GetOwner(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            for (int i = 0; i < fieldStructure.Length - 1; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = int.Parse(indexRgx.Match(fieldStructure[i]).Groups[1].Value);
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            return obj;
        }

        private static object GetFieldValue(string fieldName, object obj)
        {
            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                return field.GetValue(obj);
            }

            return default;
        }

        private static object GetFieldValueWithIndex(string fieldName, object obj, int index)
        {
            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    return ((object[])list)[index];
                }

                if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }

            return default;
        }


        public static bool SetValue<T>(this SerializedProperty property, T value)
        {
            object obj = property.GetOwner();
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            string fieldName = fieldStructure.Last();
            if (fieldName.Contains("["))
            {
                int index = int.Parse(indexRgx.Match(fieldName).Groups[1].Value);
                return SetFieldValueWithIndex(rgx.Replace(fieldName, ""), obj, index, value);
            }

            return SetFieldValue(fieldName, obj, value);
        }

        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            object obj = property.GetOwner();
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            string fieldName = fieldStructure.Last();
            FieldInfo field = obj.GetField(fieldName);
            return field;
        }

        private static bool SetFieldValue(string fieldName, object obj, object value)
        {
            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }

            return false;
        }

        private static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value)
        {
            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    ((object[])list)[index] = value;
                    return true;
                }

                if (list is IEnumerable)
                {
                    ((IList)list)[index] = value;
                    return true;
                }
            }

            return false;
        }

        public static string GetFieldName(this SerializedProperty property)
        {
            string path = property.propertyPath;
            string[] fieldStructure = path.Split('.');
            int length = fieldStructure.Length;
            if (length <= 0)
                return string.Empty;
            return fieldStructure[length - 1];
        }

        public static Type GetPropertyType(this SerializedProperty property)
        {
            Type type = null;
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = int.Parse(indexRgx.Match(fieldStructure[i]).Groups[1].Value);
                    obj = GetFieldTypeWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index, out type);
                }
                else
                {
                    obj = GetFieldType(fieldStructure[i], obj, out type);
                }
            }

            return type;
        }

        private static object GetFieldTypeWithIndex(string fieldName, object obj, int index, out Type type)
        {
            type = null;

            if (obj == null)
                return default;

            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    type = list.GetType().GetElementType();
                    return ((object[])list)[index];
                }

                if (list is IEnumerable)
                {
                    if (list.GetType().TryGetGenericArgumentType(typeof(IEnumerable<>), out var genericArgumentTypes))
                        type = genericArgumentTypes[0];
                    return ((IList)list)[index];
                }
            }

            return default;
        }

        private static object GetFieldType(string fieldName, object obj, out Type type)
        {
            type = null;

            if (obj == null)
                return default;

            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                type = field.FieldType;
                return field.GetValue(obj);
            }

            return default;
        }

        public static bool IsReferenceProperty(this SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ManagedReference;
        }

        public static void SetReferenceValue(this SerializedProperty property, object value)
        {
            property.managedReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static Type GetReferenceType(this SerializedProperty property)
        {
            var target = property.managedReferenceValue;

            if (target != null)
                return target.GetType();

            return property.GetReferenceFieldType();
        }

        public static Type GetReferenceFieldType(this SerializedProperty property)
        {
            var splitedInfo = property.managedReferenceFieldTypename.Split(' ');
            return ReflectionHelper.GetType(splitedInfo[0], splitedInfo[1]);
        }

        public static void AddArraySize(this SerializedProperty property, int value)
        {
            if (property.isArray)
                property.arraySize += value;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static T GetPropertyCustomAttribute<T>(this SerializedProperty property) where T : Attribute
        {
            var field = property.GetFieldInfo();
            if (field == null)
                return null;
            return field.GetCustomAttribute<T>();
        }
    }
}