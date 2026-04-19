using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mmang.Util
{
    public static class ReflectionHelper
    {
        #region Attributes

        public static T GetFirstCustomAttribute<T>(this Type type) where T : Attribute
        {
            var attributes = type.GetCustomAttributes<T>();
            var it = attributes.GetEnumerator();
            if (it.MoveNext())
            {
                return it.Current;
            }
            return null;
        }

        public static List<Type> FindClassesWithAttribute<T>() where T : Attribute
        {
            List<Type> result = new();
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                Type[] types = asm.GetTypes();
                foreach (var type in types)
                {
                    var attributes = type.GetCustomAttributes<T>();
                    if (attributes != null && attributes.Count() > 0)
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }

        public static List<(Type, T)> FindClassesAndInfoWithAttribute<T>() where T : Attribute
        {
            List<(Type, T)> result = new();
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                Type[] types = asm.GetTypes();
                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<T>();
                    if (attribute != null)
                    {
                        result.Add(new(type, attribute));
                    }
                }
            }

            return result;
        }
        #endregion

        #region

        public static Type[] FindClassesWithInterface<T>()
            => FindClassesWithInterface(typeof(T));

        public static Type[] FindClassesWithInterface(Type InterfaceTypeValue)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(a => a.GetTypes().
                Where(t => t.GetInterfaces().Contains(InterfaceTypeValue))).ToArray();
            return types;
        }

        #endregion

        #region Type

        public static Type GetType(string assemblyName, string className)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly == null)
                return null;
            return assembly.GetType(className);
        }

        public static List<Type> GetSubClasses(this Type baseType, bool canBeAbstract = false, bool canBeGeneric = false)
        {
            List<Type> subClasses = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                subClasses.AddRange(assembly.GetTypes().Where(type =>
                    type.IsSubclassOf(baseType)
                    && (canBeAbstract || !type.IsAbstract)
                    && (canBeGeneric || !type.IsGenericType)));
            }

            return subClasses;
        }

        private static List<Type> GetSelfAndBaseTypes(this object target)
        {
            return target is Type type ? GetSelfAndBaseTypes(type) : GetSelfAndBaseTypes(target.GetType());
        }

        private static List<Type> GetSelfAndBaseTypes(this Type targetType)
        {
            if (!s_CachedSelfAndBaseTypesMap.ContainsKey(targetType))
            {
                List<Type> types = new() { targetType };

                while (types.Last().BaseType != null)
                {
                    types.Add(types.Last().BaseType);
                }

                types.AddRange(targetType.GetInterfaces());
                s_CachedSelfAndBaseTypesMap.Add(targetType, types);
            }

            return s_CachedSelfAndBaseTypesMap[targetType];
        }

        public static bool IsChildTypeOf(this Type child, Type parent)
        {
            if (child == parent)
                return true;

            if (parent.IsAssignableFrom(child))
                return true;

            if (child.IsSubClassOfRawGeneric(parent))
                return true;

            return false;
        }

        /// <summary>是否是泛型类的子类</summary>
        public static bool IsSubClassOfRawGeneric(this Type type, Type genericType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (genericType == null) throw new ArgumentNullException(nameof(genericType));

            while (type != null && type != typeof(object))
            {
                if (IsTheRawGenericType(type))
                    return true;
                type = type.BaseType;
            }

            return false;

            bool IsTheRawGenericType(Type test)
                => genericType == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        /// <summary>获取泛型类的泛型类型</summary>
        public static bool TryGetGenericArgumentType(this Type type, Type genericType, out List<Type> genericArgumentTypes)
        {
            genericArgumentTypes = new List<Type>();

            var baseTypes = GetSelfAndBaseTypes(type);
            foreach (var baseType in baseTypes)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericType)
                {
                    genericArgumentTypes.Add(baseType.GetGenericArguments()[0]);
                }
            }

            return genericArgumentTypes.Count > 0;
        }

        /// <summary>
        /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
        /// </summary>
        /// <param name="type">需要测试的类型。</param>
        /// <param name="generic">泛型接口类型，传入 typeof(IXxx&lt;&gt;)</param>
        /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
        public static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            // 没有找到任何匹配的接口或类型。
            return false;

            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        public static Type GetTypeByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            if (typeName.Contains("."))
            {
                var assemblyName = typeName[..typeName.IndexOf('.')];
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        #endregion

        #region Field,Property,Method

        static readonly Dictionary<Type, List<Type>> s_CachedSelfAndBaseTypesMap = new();
        static readonly Dictionary<Type, MemberInfo[]> s_CachedTypeMemberInfoMap = new();

        /// <summary>
        /// 获取一个字段
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static FieldInfo GetField(this object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).LastOrDefault();
        }

        /// <summary>
        /// 获取一个属性
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this object target, string propertyName)
        {
            return GetAllProperties(target, p => p.Name.Equals(propertyName, StringComparison.Ordinal))
                .LastOrDefault();
        }

        /// <summary>
        /// 获取一个方法
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="methodName">方法名名</param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this object target, string methodName)
        {
            return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.Ordinal)).LastOrDefault();
        }

        public static MemberInfo GetMember(this object target, string memberName)
        {
            return GetAllMember(target, m => m.Name.Equals(memberName, StringComparison.Ordinal)).LastOrDefault();
        }

        /// <summary>
        /// 获取全部字段
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="predicate">匹配条件</param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(this object target, Func<FieldInfo, bool> predicate = null)
        {
            IEnumerable<MemberInfo> memberInfos = GetAllMember(target);
            foreach (var memberInfo in memberInfos)
            {
                if (memberInfo is FieldInfo fieldInfo && (predicate == null || predicate(fieldInfo)))
                    yield return fieldInfo;
            }
        }

        /// <summary>
        /// 获取全部属性
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="predicate">匹配条件</param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetAllProperties(this object target,
            Func<PropertyInfo, bool> predicate = null)
        {
            IEnumerable<MemberInfo> memberInfos = GetAllMember(target);
            foreach (var memberInfo in memberInfos)
            {
                if (memberInfo is PropertyInfo propertyInfo && (predicate == null || predicate(propertyInfo)))
                    yield return propertyInfo;
            }
        }

        /// <summary>
        /// 获取全部方法
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="predicate">匹配条件</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllMethods(this object target, Func<MethodInfo, bool> predicate = null)
        {
            IEnumerable<MemberInfo> memberInfos = GetAllMember(target);
            foreach (var memberInfo in memberInfos)
            {
                if (memberInfo is MethodInfo methodInfo && (predicate == null || predicate(methodInfo)))
                    yield return methodInfo;
            }
        }

        /// <summary>
        /// 获取全部成员
        /// </summary>
        /// <param name="target"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetAllMember(this object target, Func<MemberInfo, bool> predicate = null)
        {
            if (target == null)
            {
                UnityEngine.Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);
            for (int i = types.Count - 1; i >= 0; i--)
            {
                if (!s_CachedTypeMemberInfoMap.ContainsKey(types[i]))
                    s_CachedTypeMemberInfoMap.Add(types[i],
                        types[i].GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                            BindingFlags.Public | BindingFlags.DeclaredOnly));

                IEnumerable<MemberInfo> memberInfos = s_CachedTypeMemberInfoMap[types[i]];
                if (predicate != null)
                    memberInfos = memberInfos.Where(predicate);

                foreach (var memberInfo in memberInfos)
                {
                    yield return memberInfo;
                }
            }
        }

        /// <summary>
        /// 获取对象的字段值
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="fieldName">字段名</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFieldValue<T>(this object target, string fieldName)
        {
            return (T)GetField(target, fieldName)?.GetValue(target);
        }

        /// <summary>
        /// 设置对象的字段值
        /// </summary>
        /// <param name="target">对象实例或者type</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">目标值</param>
        public static void SetFieldValue(this object target, string fieldName, object value)
        {
            GetField(target, fieldName)?.SetValue(target, value);
        }

        #endregion
    }
}