using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using Mmang.Util;

namespace Mmang.Editors
{

    public class DropdownField<T> : BaseField<T>
    {
        private VisualElement m_InputContainer;

        public DropdownField(string label) : base(label, null)
        {
            m_InputContainer = this.Q(inputUssClassName);
            
            var button = new Button(() =>
            {
                if (this is BaseField<float> fThis)
                    fThis.value++;                    
            });
            button.Add(new Label("Click"));


            m_InputContainer.Add(button);
        }

        public override void SetValueWithoutNotify(T newValue)
        {
            base.SetValueWithoutNotify(newValue);
        }
    }

    [CustomPropertyDrawer(typeof(MEnumsAttribute))]
    public class MEnumsDrawer : PropertyDrawer
    {

        VisualElement Test(SerializedProperty property)
        {
            DropdownField<string> field = new("Test");
            field.RegisterValueChangedCallback(e => {Debug.Log("NewValue: " + e.newValue);});
            
            
            return field;
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            //return Test(property);
            Type type = property.GetPropertyType();
            if (!typeof(Enum).IsAssignableFrom(type))
                return new PropertyField(property);
        
            Dictionary<object, string> map = MEnums.GetValueNameMap(type);
            
            if (map == null)
                return UIElementHelper.CreateDefaultPropertyField(property);

            VisualElement root = new();
            root.SetHorizontal();

            int currentIndex = property == null ? 0 : property.enumValueIndex;
            var popupField = UIElementHelper.CreateDropdownObject(map, property.displayName, currentIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                property.serializedObject.Update();
                property.enumValueIndex = MEnums.GetIndex(type, evt.newValue);
                property.serializedObject.ApplyModifiedProperties();
            });
            popupField.TrackPropertyValue(property, evt =>
            {
                popupField.value = Enum.ToObject(type, property.enumValueIndex);
            });
            root.Add(popupField);

            return root;
        }
    }

}