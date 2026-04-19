using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Util
{
    public static class UIElementHelper
    {
        public static void SetBorderWidth(this VisualElement visualElement, float width)
        {
            visualElement.style.borderTopWidth = visualElement.style.borderBottomWidth =
                visualElement.style.borderLeftWidth = visualElement.style.borderRightWidth = width;
        }

        public static void SetBorderColor(this VisualElement visualElement, Color color)
        {
            visualElement.style.borderTopColor = visualElement.style.borderBottomColor =
                visualElement.style.borderLeftColor = visualElement.style.borderRightColor = color;
        }

        public static void SetBorderRadius(this VisualElement visualElement, float radius)
        {
            visualElement.style.borderTopLeftRadius = radius;
            visualElement.style.borderTopRightRadius = radius;
            visualElement.style.borderBottomLeftRadius = radius;
            visualElement.style.borderBottomRightRadius = radius;
        }

        public static void SetTopBorderRadius(this VisualElement visualElement, float radius)
        {
            visualElement.style.borderTopLeftRadius = radius;
            visualElement.style.borderTopRightRadius = radius;
        }

        public static void SetBottomBorderRadius(this VisualElement visualElement, float radius)
        {
            visualElement.style.borderBottomLeftRadius = radius;
            visualElement.style.borderBottomRightRadius = radius;
        }

        public static void SetMargin(this VisualElement visualElement, Vector4 size)
        {
            visualElement.style.marginTop = size.x;
            visualElement.style.marginBottom = size.y;
            visualElement.style.marginLeft = size.z;
            visualElement.style.marginRight = size.w;
        }

        public static void ClearMargin(this VisualElement visualElement)
        {
            visualElement.style.marginTop = visualElement.style.marginBottom =
                visualElement.style.marginLeft = visualElement.style.marginRight = 0;
        }

        public static void SetPadding(this VisualElement visualElement, Vector4 size)
        {
            visualElement.style.paddingTop = size.x;
            visualElement.style.paddingBottom = size.y;
            visualElement.style.paddingLeft = size.z;
            visualElement.style.paddingRight = size.w;
        }

        public static void SetHorizontal(this VisualElement visualElement)
        {
            visualElement.style.flexDirection = FlexDirection.Row;
        }

        public static void SetFlexGrow(this VisualElement visualElement, bool value = true)
        {
            visualElement.style.flexGrow = value ? 1 : 0;
        }

        public static void SetDisplayNone(this VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.None;
        }

        public static void SetDisplayFlex(this VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.Flex;
        }

        public static void SetDisplayFlexOrNone(this VisualElement visualElement, bool flex)
        {
            visualElement.style.display = flex ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetFontSize(this VisualElement visualElement, float size)
        {
            visualElement.style.fontSize = size;
        }

        #region 下拉菜单
        static string GetDropdownDataName<T>(Dictionary<T, string> map, T id)
        {
            if (map.TryGetValue(id, out var name))
                return name;
            return "None";
        }

        public static void RefreshDropdownMap<T>(PopupField<T> popupField, Dictionary<T, string> map, T currentValue)
        {
            var itemList = map.Keys.ToList();
            itemList.Sort();
            int currentIndex = itemList.IndexOf(currentValue);
            currentIndex = Mathf.Max(0, currentIndex);

            popupField.choices = itemList;
            popupField.index = currentIndex;
            popupField.formatSelectedValueCallback = id => GetDropdownDataName(map, id);
            popupField.formatListItemCallback = id => GetDropdownDataName(map, id);
        }

        public static PopupField<uint> CreateDropdownUInt(Dictionary<uint, string> map, string label, int currentIndex)
        {
            List<uint> itemList = map.Keys.ToList();
            itemList.Sort();

            PopupField<uint> popupField = new(itemList, currentIndex)
            {
                label = label,
                formatSelectedValueCallback = id => GetDropdownDataName(map, id),
                formatListItemCallback = id => GetDropdownDataName(map, id)
            };

            popupField.SetFlexGrow();
            popupField.AddManipulator(new AlignLabelManipulator());
            return popupField;
        }
        public static PopupField<uint> CreateDropdownUInt(Dictionary<uint, string> map, string label, SerializedProperty property = null, bool withManipulator = true)
        {
            List<uint> itemList = map.Keys.ToList();
            itemList.Sort();

            int currentIndex = property == null ? 0 : itemList.IndexOf(property.uintValue);
            currentIndex = Mathf.Max(0, currentIndex);

            PopupField<uint> popupField = new(itemList, currentIndex)
            {
                label = label,
                formatSelectedValueCallback = id => GetDropdownDataName(map, id),
                formatListItemCallback = id => GetDropdownDataName(map, id)
            };
            if (property != null)
                popupField.BindProperty(property);
            popupField.SetFlexGrow();
            if (withManipulator)
                popupField.AddManipulator(new AlignLabelManipulator());
            return popupField;
        }

        public static PopupField<int> CreateDropdownInt(Dictionary<int, string> map, string label, int currentIndex, bool withManipulator = true)
        {
            List<int> itemList = map.Keys.ToList();
            itemList.Sort();

            PopupField<int> popupField = new(itemList, currentIndex)
            {
                label = label,
                formatSelectedValueCallback = id => GetDropdownDataName(map, id),
                formatListItemCallback = id => GetDropdownDataName(map, id)
            };

            popupField.SetFlexGrow();
            if (withManipulator)
                popupField.AddManipulator(new AlignLabelManipulator());
            return popupField;
        }
        public static PopupField<int> CreateDropdownInt(Dictionary<int, string> map, string label, SerializedProperty property = null)
        {
            List<int> itemList = map.Keys.ToList();
            itemList.Sort();

            int currentIndex = property == null ? 0 : itemList.IndexOf(property.intValue);
            currentIndex = Mathf.Max(0, currentIndex);

            PopupField<int> popupField = new(itemList, currentIndex)
            {
                label = label,
                formatSelectedValueCallback = id => GetDropdownDataName(map, id),
                formatListItemCallback = id => GetDropdownDataName(map, id)
            };
            if (property != null)
                popupField.BindProperty(property);
            popupField.SetFlexGrow();
            popupField.AddManipulator(new AlignLabelManipulator());
            return popupField;
        }
        public static PopupField<object> CreateDropdownObject(Dictionary<object, string> map, string label, int currentIndex = 0)
        {
            List<object> itemList = map.Keys.ToList();

            PopupField<object> popupField = new(itemList, currentIndex)
            {
                label = label,
                formatSelectedValueCallback = id => GetDropdownDataName(map, id),
                formatListItemCallback = id => GetDropdownDataName(map, id)
            };
            popupField.SetFlexGrow();
            popupField.AddManipulator(new AlignLabelManipulator());
            return popupField;
        }

        #endregion

        #region 组

        public static void BindToggleGroup(Toggle controlToggle, params VisualElement[] visualElements)
        {
            Setup();
            controlToggle.RegisterValueChangedCallback((e) => Setup());

            void Setup()
            {
                int count = visualElements.Length;
                for (int i = 0; i < count; i++)
                {
                    var element = visualElements[i];
                    element.SetEnabled(controlToggle.value);
                }
            }
        }

        #endregion


        #region Text Field

        public static void SetPlaceholder(this TextField textField, string placeholderText)
        {
            const string PlaceholderClassName = "unity-text-field__placeholder";

            var existingLabel = textField.Q<Label>(className: PlaceholderClassName);
            if (existingLabel != null)
            {
                // 已经有了，更新文字后直接返回
                existingLabel.text = placeholderText;
                return;
            }

            var placeholderLabel = new Label(placeholderText);
            placeholderLabel.AddToClassList(PlaceholderClassName);
            
            placeholderLabel.style.position = Position.Absolute;
            placeholderLabel.style.left = 4;
            placeholderLabel.style.top = 2;
            placeholderLabel.style.color = new StyleColor(Color.gray);
            placeholderLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            
            // 忽略鼠标事件
            placeholderLabel.pickingMode = PickingMode.Ignore;

            textField.Add(placeholderLabel);

            void UpdatePlaceholderVisibility(string newValue)
            {
                if (string.IsNullOrEmpty(newValue))
                {
                    placeholderLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    placeholderLabel.style.display = DisplayStyle.None;
                }
            }

            UpdatePlaceholderVisibility(textField.value);
            textField.RegisterValueChangedCallback(evt => UpdatePlaceholderVisibility(evt.newValue));
        }



        #endregion


        #region 个人常用布局

        public static Button CreateButton(string name, System.Action action = null)
        {
            Button bt = new(action);
            bt.Add(new Label(name));
            return bt;
        }

        public static Slider CreateSliderWithValueDisplay(string name, float min, float max)
            => CreateSliderWithValueDisplay(name, min, min, max);
        public static Slider CreateSliderWithValueDisplay(string name, float curValue, float min, float max)
        {
            Slider slider = new(name, min, max) { value = curValue };

            Box box = new();
            box.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
            box.style.width = 60;

            Label label = new(curValue.ToString());
            slider.RegisterValueChangedCallback((evt) => { label.text = evt.newValue.ToString(); });
            box.Add(label);
            slider.Add(box);
            return slider;
        }

        public static MinMaxSlider CreateMinMaxSliderWithValueDisplay(string name, float minLimit, float maxLimit)
            => CreateMinMaxSliderWithValueDisplay(name, minLimit, minLimit, minLimit, maxLimit);
        public static MinMaxSlider CreateMinMaxSliderWithValueDisplay(string name, float curMin, float curMax, float minLimit, float maxLimit)
        {
            MinMaxSlider slider = new(name, curMin, curMax, minLimit, maxLimit);

            Box box = new();
            box.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
            box.style.width = 120;

            Label label = new(curMin.ToString("0.00") + "~" + curMax.ToString("0.00"));
            slider.RegisterValueChangedCallback((evt) =>
            {
                label.text = evt.newValue.x.ToString("0.00") + "~" + evt.newValue.y.ToString("0.00");
            });
            box.Add(label);
            slider.Add(box);
            return slider;
        }

        public static GroupBox CreateSimpleGroupBox()
        {
            GroupBox box = new();
            box.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            box.SetBorderColor(Color.white * 0.5f);
            box.SetBorderWidth(1);
            box.SetPadding(new(6f, 6f, 10f, 10f));
            box.SetFlexGrow();
            return box;
        }

        public static GroupBox CreateSimpleGroupBox(VisualElement childElement)
        {
            GroupBox box = CreateSimpleGroupBox();
            box.Add(childElement);
            return box;
        }

        public static GroupBox CreateSimpleIncludeGroupBox()
        {
            GroupBox box = new();
            box.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f, 0.5f);
            box.SetBorderColor(Color.white * 0.5f);
            box.SetBorderWidth(1);
            box.SetPadding(new(10f, 10f, 2f, 2f));
            box.SetFlexGrow();
            return box;
        }

        public static GroupBox CreateWarningBox()
        {
            GroupBox box = new();
            box.style.backgroundColor = new Color(0.5f, 0.5f, 0.17f, 0.5f);
            box.SetBorderColor(Color.white * 0.5f);
            box.SetBorderWidth(1);
            box.SetPadding(new(10f, 10f, 2f, 2f));
            box.SetFlexGrow();
            return box;
        }

        #endregion
        public delegate bool DrawPropertyConditionDelegate(SerializedProperty property);
        public static DrawPropertyConditionDelegate FilterFieldName(string filter) => (property) =>
        {
            return filter == property.GetFieldName();
        };
        public static DrawPropertyConditionDelegate FilterFieldName(List<string> filter) => (property) =>
        {
            return filter.Contains(property.GetFieldName());
        };

        public static void DrawDefaultInspector(VisualElement container, SerializedObject serializedObject, DrawPropertyConditionDelegate condition = null, bool bind = false)
        {
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (condition != null && condition(iterator))
                        continue;
                        
                    PropertyField propertyField;
                    if (!bind)
                        propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };
                    else
                    {
                        propertyField = new PropertyField() { name = "PropertyField:" + iterator.propertyPath };
                        propertyField.BindProperty(iterator.Copy());
                    }

                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(value: false);

                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }
        }

        public static VisualElement DrawIncludeProperty(SerializedProperty iterator, DrawPropertyConditionDelegate condition = null, bool bind = false)
        {
            VisualElement container = new();
            var endProperty = iterator.GetEndProperty();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.EqualContents(endProperty))
                        break;

                    if (iterator.propertyPath == "m_Script" || (condition != null && condition(iterator)))
                        continue;

                    PropertyField propertyField;
                    if (!bind)
                        propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };
                    else
                    {
                        propertyField = new PropertyField() { name = "PropertyField:" + iterator.propertyPath };
                        propertyField.BindProperty(iterator.Copy());
                    }

                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }
            return container;
        }

        public static VisualElement DrawIncludePropertyWithFilter(SerializedProperty iterator, string filter, bool bind = false)
        {
            return DrawIncludeProperty(iterator, FilterFieldName(filter), bind);
        }

        public static VisualElement DrawIncludePropertyWithFilter(SerializedProperty iterator, List<string> filter, bool bind = false)
        {
            if (filter == null)
                return DrawIncludeProperty(iterator);
            return DrawIncludeProperty(iterator, FilterFieldName(filter), bind);
        }

        public static VisualElement CreateDefaultPropertyField(SerializedProperty property)
        {
            PropertyField propertyField = new(property);
            propertyField.BindProperty(property);
            return propertyField;
        }
    }

}