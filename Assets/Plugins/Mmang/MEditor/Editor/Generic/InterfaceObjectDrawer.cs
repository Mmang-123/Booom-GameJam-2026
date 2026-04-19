using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using Mmang.Generic;
using System.Collections;
using UnityEditor.UIElements;

namespace Mmang.Editors
{
    [CustomPropertyDrawer(typeof(InterfaceObject<>))]
    public class InterfaceObjectDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var interfaceType = GetInterfaceType();

            SerializedProperty objectProp = property.FindPropertyRelative("m_Object");

            var objectField = new ObjectField(property.displayName)
            {
                objectType = typeof(Object),
                bindingPath = objectProp.propertyPath
            };


            objectField.schedule.Execute(() =>
            {
                CheckGlobalDrag(objectField, interfaceType);
            }).Every(50); // 每 50ms 检查一次


            objectField.RegisterCallback<DragPerformEvent>(evt =>
            {
                var draggedObjects = DragAndDrop.objectReferences;
                if (draggedObjects.Length == 0) return;

                var validComponent = ValidateAndExtract(draggedObjects[0], interfaceType);
                if (validComponent != null)
                {
                    DragAndDrop.AcceptDrag();
                    objectField.value = validComponent;
                    objectProp.objectReferenceValue = validComponent;
                    objectProp.serializedObject.ApplyModifiedProperties();
                }
            });

            // 值改变的回调
            objectField.RegisterValueChangedCallback(evt =>
            {
                Object newValue = evt.newValue;

                if (newValue == null)
                {
                    return;
                }

                Object validComponent = ValidateAndExtract(newValue, interfaceType);

                if (validComponent != newValue)
                {
                    if (validComponent == null)
                    {
                        Debug.LogWarning($"对象 '{newValue.name}' 没有实现接口 {interfaceType.Name}");
                        objectField.value = evt.previousValue;
                    }
                    else
                    {
                        objectField.value = validComponent;
                    }

                    objectProp.objectReferenceValue = objectField.value;
                    objectProp.serializedObject.ApplyModifiedProperties();
                }
            });


            root.Add(objectField);

            return root;
        }

        private void CheckGlobalDrag(ObjectField objectField, System.Type interfaceType)
        {
            // 获取当前拖拽的物体列表
            var draggedObjects = DragAndDrop.objectReferences;

            if (draggedObjects == null || draggedObjects.Length == 0)
            {
                if (!objectField.enabledSelf)
                {
                    objectField.SetEnabled(true);
                }
                return;
            }

            var draggedObj = draggedObjects[0];
            bool isValid = ValidateAndExtract(draggedObj, interfaceType) != null;

            if (objectField.enabledSelf != isValid)
            {
                objectField.SetEnabled(isValid);
            }
        }

        private Object ValidateAndExtract(Object obj, System.Type interfaceType)
        {
            if (obj == null) return null;

            if (interfaceType.IsAssignableFrom(obj.GetType())) return obj;

            if (obj is GameObject go) return go.GetComponent(interfaceType);

            if (obj is Component comp) return comp.GetComponent(interfaceType);

            return null;
        }

        private System.Type GetInterfaceType()
        {
            System.Type fieldType = fieldInfo.FieldType;

            if (typeof(IEnumerable).IsAssignableFrom(fieldType) && fieldType.IsGenericType)
            {
                if (fieldType.GetGenericArguments().Length > 0)
                {
                    fieldType = fieldType.GetGenericArguments()[0];
                }
            }
            else if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }

            if (fieldType != null && fieldType.IsGenericType)
            {
                return fieldType.GetGenericArguments()[0];
            }

            return typeof(Object);
        }
    }
}