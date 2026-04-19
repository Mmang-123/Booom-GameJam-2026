using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;
using UnityEditor.UIElements;

namespace Mmang.Game.Editors
{
    [CustomPropertyDrawer(typeof(GlobalPropertyReference))]
    public class GlobalPropertyReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            VisualElement root = new();

            root.Add(new Label("A"));

            return root;
        }
    }


    [CustomPropertyDrawer(typeof(GlobalPropertyReference<>))]
    public class GenericGlobalPropertyReferenceDrawer : PropertyDrawer
    {
        SerializedObject configSO;
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            VisualElement root = new();

            string name = ObjectNames.NicifyVariableName(serializedProperty.name);

            var property = serializedProperty.GetValue() as IGenericGlobalPropertyReference;
            //var propertyType = serializedProperty.GetPropertyType();
            if (property == null)
            {
                root.Add(new Label($" {name} is NULL"));
                return root;
            }

            var globalPropertyConfig = GlobalConfigAssets.GetConfigInstance<GlobalPropertiesConfig>();
            if (globalPropertyConfig == null)
            {
                root.Add(new Label($" {name}  (Config Missing)"));
                return root;
            }

            if (!globalPropertyConfig.TryGetProperty(property.PropertyName, out var globalProperty))
            {
                root.Add(new Label($" {name}  ({property.PropertyName} is Missing)"));
                return root;
            }

            // todo 类型匹配
            //..

            // temp
            int propertyIndex = 0; //todo 查找index
            configSO = new(globalPropertyConfig);
            var targetSerializedProperty = configSO.FindProperty($"m_PropertyElements.Array.data[{propertyIndex}].m_Value");
            PropertyField propertyField = new(targetSerializedProperty)
            {
                label = $"{name} (Global Property)"
            };
            propertyField.Bind(configSO);

            root.Add(propertyField);

            return root;
        }
    }
}