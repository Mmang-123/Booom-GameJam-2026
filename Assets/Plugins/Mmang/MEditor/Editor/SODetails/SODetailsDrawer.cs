using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using Mmang.Util;

namespace Mmang.Editors
{
    [CustomPropertyDrawer(typeof(SODetailsAttribute))]
    public class SODetailsDrawer : PropertyDrawer
    {
        private static void CollectReferencesRecursive(Object current, HashSet<Object> result, HashSet<Object> visited)
        {
            if (current == null)
                return;

            visited.Add(current);

            var so = new SerializedObject(current);
            SerializedProperty iterator = so.GetIterator();

            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Object referenced = iterator.objectReferenceValue;
                    if (referenced != null && referenced is ScriptableObject)
                    {
                        if (visited.Contains(referenced))
                        {
                            result.Add(referenced);
                        }
                        else
                        {
                            CollectReferencesRecursive(referenced, result, visited);
                        }
                    }
                }
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var attr = (SODetailsAttribute)attribute;

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;

            // ========== 头部行 ==========
            var headerRow = new VisualElement();

            var triangle = new Foldout
            {
                text = "",
                value = property.isExpanded
            };
            triangle.AddToClassList("unity-foldout");
            triangle.contentContainer.style.display = DisplayStyle.None;

            // 绝对定位，不占布局宽度
            triangle.style.position = Position.Absolute;
            triangle.style.left = 0;
            triangle.style.top = 0;
            triangle.style.width = 14;
            triangle.style.height = EditorGUIUtility.singleLineHeight;
            triangle.style.paddingLeft = 0;
            triangle.style.paddingRight = 0;
            triangle.style.marginLeft = 0;
            triangle.style.marginRight = 0;

            PropertyField propertyField = new(property);

            headerRow.Add(triangle);
            headerRow.Add(propertyField);

            root.Add(headerRow);

            // ========== 细节区域 ==========
            var contentContainer = new VisualElement();
            //contentContainer.style.marginLeft = 20;
            contentContainer.style.marginTop = 2;
            root.Add(contentContainer);

            //int rootId = property.serializedObject.targetObject.GetEntityId();

            void Rebuild()
            {
                contentContainer.Clear();

                var target = property.objectReferenceValue;
                if (target == null || !property.isExpanded)
                {
                    contentContainer.style.display = DisplayStyle.None;
                    triangle.value = property.isExpanded;
                    return;
                }

                contentContainer.style.display = DisplayStyle.Flex;

                HashSet<Object> conflicted = new();
                CollectReferencesRecursive(target, conflicted, new());
                if (conflicted.IsNotEmpty())
                    contentContainer.Add(BuildFallbackCard(conflicted));
                else
                    DrawEmbeddedInspector(contentContainer, target);
            }

            Rebuild();

            // 三角切换展开
            triangle.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
                Rebuild();
            });

            // 引用变化
            propertyField.RegisterValueChangeCallback(_ => Rebuild());

            // 外部变化
            root.TrackPropertyValue(property, _ =>
            {
                triangle.value = property.isExpanded;
                Rebuild();
            });

            return root;
        }

        private void DrawEmbeddedInspector(VisualElement container, Object target)
        {
            var box = new VisualElement();
            box.style.paddingLeft = 16;
            box.style.paddingRight = 16;
            box.style.paddingTop = 6;
            box.style.paddingBottom = 6;
            box.style.borderTopWidth = 1;
            box.style.borderBottomWidth = 1;
            box.style.borderLeftWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderTopColor = new Color(0, 0, 0, 0.25f);
            box.style.borderBottomColor = new Color(0, 0, 0, 0.25f);
            box.style.borderLeftColor = new Color(0, 0, 0, 0.25f);
            box.style.borderRightColor = new Color(0, 0, 0, 0.25f);
            box.style.backgroundColor = new Color(0, 0, 0, 0.06f);
            container.Add(box);

            var so = new SerializedObject(target);
            var inspector = new InspectorElement(so);

            box.Add(inspector);
        }

        private void BuildFieldsForObject(Object obj, VisualElement parent)
        {
            var so = new SerializedObject(obj);
            var iterator = so.GetIterator();

            // 跳 m_Script
            if (!iterator.NextVisible(true))
                return;

            while (iterator.NextVisible(false))
            {
                var childProp = iterator.Copy();
                Debug.Log(childProp.GetFieldInfo().Name);
                if (childProp.GetPropertyCustomAttribute<SODetailsAttribute>() != null)
                {
                    Debug.Log("!!");
                    continue;
                }
                var field = new PropertyField(childProp);
                field.Bind(so);
                parent.Add(field);
            }
        }

        private VisualElement BuildFallbackCard(HashSet<Object> conflictedObjs)
        {
            var card = new VisualElement();
            card.style.paddingLeft = 8;
            card.style.paddingRight = 8;
            card.style.paddingTop = 6;
            card.style.paddingBottom = 6;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new Color(0, 0, 0, 0.25f);
            card.style.borderBottomColor = new Color(0, 0, 0, 0.25f);
            card.style.borderLeftColor = new Color(0, 0, 0, 0.25f);
            card.style.borderRightColor = new Color(0, 0, 0, 0.25f);
            card.style.backgroundColor = new Color(1f, 0.85f, 0.2f, 0.12f);

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;

            var nameLabel = new Label("存在循环引用: ");
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.flexGrow = 1;

            titleRow.Add(nameLabel);

            var objs = new VisualElement();
            foreach (var obj in conflictedObjs)
            {
                var objRow = new VisualElement();
                objRow.style.flexDirection = FlexDirection.Row;
                objRow.style.alignItems = Align.Center;


                var objLabel = new Label(obj.name);
                objLabel.style.paddingLeft = 8;


                var pingBtn = new Button(() => EditorGUIUtility.PingObject(obj)) { text = "Ping" };
                pingBtn.style.left = 16;

                objRow.Add(objLabel);
                objRow.Add(pingBtn);

                objs.Add(objRow);
            }

            card.Add(titleRow);
            card.Add(objs);

            return card;
        }
    }
}