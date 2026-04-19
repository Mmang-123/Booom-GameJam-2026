using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Mmang.Util
{
    public static class SerializationUtils
    {
        public static byte[] ToBinary(this object obj)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(mStream, obj);
                return mStream.ToArray();
            }
        }
        public static T Deserialize<T>(this byte[] bytes) where T : class
        {
            using (MemoryStream mStream = new MemoryStream(bytes))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(mStream) as T;
            }
        }

        public static JsonElement Serialize(this object obj)
        {
            JsonElement elem = new JsonElement();

            elem.type = obj.GetType().AssemblyQualifiedName;
#if UNITY_EDITOR
            elem.jsonDatas = UnityEditor.EditorJsonUtility.ToJson(obj);
#else
			elem.jsonDatas = JsonUtility.ToJson(obj);
#endif

            return elem;
        }
        public static T Deserialize<T>(this JsonElement e)
        {
            var obj = Activator.CreateInstance(Type.GetType(e.type));
#if UNITY_EDITOR
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#else
			JsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#endif
            return (T)obj;
        }
        public static void FromJsonOverwrite(JsonElement jsonElement, object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(jsonElement.jsonDatas, target);
#else
			JsonUtility.FromJsonOverwrite(jsonElement.jsonDatas, target);
#endif
        }

        public static T Clone<T>(this object origin)
        {
            JsonElement jsonElement = Serialize(origin);
            T clone = Deserialize<T>(jsonElement);
            return clone;
        }
    }
}
