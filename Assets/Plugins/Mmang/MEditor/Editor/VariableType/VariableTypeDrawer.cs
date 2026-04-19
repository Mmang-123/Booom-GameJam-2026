using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using Mmang.Util;

namespace Mmang.Editors
{
    [CustomPropertyDrawer(typeof(VariableTypeAttribute))]
    internal class VariableTypeDrawer : PropertyDrawer
    {
        class DrawerData
        {
            public int CurrentIndex;
            public List<Type> Types; // 从Manager获取的引用
            public SerializedProperty Property;
        }

        private StyleSheet m_StyleSheet;

        private void OnTypeIndexChanged(int newIndex, DrawerData data)
        {
            if (newIndex == data.CurrentIndex)
                return;

            var newType = data.Types[newIndex];

            if (newType != null)
            {
                var newInstance = Activator.CreateInstance(newType);
                data.Property.SetReferenceValue(newInstance);
            }
            else
            {
                data.Property.SetReferenceValue(null);
            }

            data.CurrentIndex = newIndex;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_StyleSheet = m_StyleSheet != null ? m_StyleSheet : AssetDatabase.LoadAssetAtPath<StyleSheet>(MEditorPathStorage.GetStyleSheetPath("FoldoutPopupField"));
            var root = new VisualElement();
            root.styleSheets.Add(m_StyleSheet);

            if (!property.IsReferenceProperty())
                return new PropertyField(property);

            Type type = property.GetReferenceFieldType();
            var info = VariableTypeManager.GetVariableTypeInfo(type);
            if (info == null)
                return new PropertyField(property);

            List<Type> types = info.SubTypes;
            if (types.Count == 0)
                return new PropertyField(property);

            bool isArrayProperty = property.propertyPath.EndsWith("]");

            DrawerData data = new()
            {
                Property = property,
                Types = types
            };

            if (property.managedReferenceValue == null)
            {
                data.CurrentIndex = -1;
                OnTypeIndexChanged(0, data);
            }
            else
            {
                Type propertyType = property.GetReferenceType();
                bool found = false;
                for (int i = types.Count - 1; i >= 0; i--)
                {
                    Type t = types[i];
                    if (propertyType == t)
                    {
                        found = true;
                        data.CurrentIndex = i;
                        break;
                    }
                }
                if (!found)
                {
                    data.CurrentIndex = -1;
                    OnTypeIndexChanged(0, data);
                }
            }

            Dictionary<int, string> map = info.GetNameMap();

            Foldout foldout = new() { };
            var foldoutToggle = foldout.Q<Toggle>();
            var foldoutHeader = foldoutToggle.ElementAt(0);
            var label = foldoutToggle.Q<Label>();

            foldoutHeader.AddToClassList("foldout-popup-toggle");

            string labelName = isArrayProperty ? map[data.CurrentIndex] : property.displayName;

            var popupField = UIElementHelper.CreateDropdownInt(map, labelName, data.CurrentIndex);
            popupField.RegisterValueChangedCallback((evt) => OnTypeIndexChanged(evt.newValue, data));

            foldoutHeader.Add(popupField);
            //label.parent.Add(popupField);

            var copiedProperty = property.Copy();
            var endProperty = copiedProperty.GetEndProperty();
            if (copiedProperty.NextVisible(true))
            {
                do
                {
                    if (copiedProperty.EqualContents(endProperty))
                        break;
                    PropertyField field = new();
                    field.BindProperty(copiedProperty);
                    foldout.Add(field);
                }
                while (copiedProperty.NextVisible(false));
            }

            root.Add(foldout);

            return root;
        }
    }

}