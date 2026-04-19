using System.Reflection;
using Mmang.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Editors
{
    public class SOComponentContainerInterfaceEditor : Editor
    {
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(CreateComponentsView());

            return root;
        }

        public VisualElement CreateComponentsView()
        {
            var root = new ComponentContainerView() { name = "so_components_root" };
            root.style.marginTop = -2;

            var iTarget = target as ISOComponentContainer;
            var property = serializedObject.FindProperty(iTarget.SOComponentsFieldName);

            if (property != null && property.isArray)
            {
                int size = property.arraySize;
                for (int i = 0; i < size; i++)
                {
                    var componentProperty = property.GetArrayElementAtIndex(i);
                    var componentView = CreateComponentView(componentProperty);
                    root.AddComponentBlock(componentView);
                }
            }
            else
            {
                Debug.LogError($"{target.GetType()} 应该包含字段 {iTarget.SOComponentsFieldName}");
                return root;
            }

            return root;
        }

        public virtual ComponentBlock CreateComponentView(SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
            {
                return new ComponentBlock("Missing") { name = "so_component" };
            }

            var propertyType = property.managedReferenceValue.GetType();

            string componentName;
            
            if (propertyType.GetCustomAttribute<SOComponentAttribute>() is { } componentAttribute)
            {
                componentName = componentAttribute.Name;
            }
            else
            {
                componentName = ObjectNames.NicifyVariableName(propertyType.Name);
            }

            var componentBlock = new ComponentBlock(componentName) { name = "so_component" };

            var drawer = SOComponentDrawerManager.GetDrawer(propertyType);
            drawer.DrawComponentView(property, componentBlock);

            return componentBlock;
        }

        public virtual VisualElement CreateAddComponentButtonView()
        {
            VisualElement view = new();



            return view;
        }
    }

    [CustomEditor(typeof(SOComponentContainer))]
    public class SOComponentContainerEditor : SOComponentContainerInterfaceEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return base.CreateInspectorGUI();
        }
    }
}