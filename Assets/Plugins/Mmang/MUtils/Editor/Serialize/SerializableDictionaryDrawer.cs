using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Util
{
    [CustomPropertyDrawer(typeof(SerializableDictionary), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            //
            VisualElement root = new VisualElement();

            SerializedProperty itemsProperty = property.FindPropertyRelative("m_List");

            //
            Foldout foldout = new Foldout();
            foldout.text = property.displayName;

            //
            foldout.value = property.isExpanded;

            // 监听折叠点击事件，将状态保存到 SerializedProperty 中
            foldout.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
            });

            // 创建并配置 ListView
            ListView listView = new ListView();
            listView.bindingPath = itemsProperty.propertyPath;
            listView.showAddRemoveFooter = true;
            listView.showBoundCollectionSize = false;
            listView.reorderable = true;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            var scrollView = listView.Q<ScrollView>();
            scrollView.SetBorderWidth(1);
            scrollView.SetBorderColor(new Color(0.1f, 0.1f, 0.1f));

            //
            listView.style.marginLeft = -15;

            // 数量显示
            VisualElement spacer = new();
            spacer.style.flexGrow = 1;
            var header = foldout.Q<Toggle>();
            header.style.marginBottom = 0;

            IntegerField countField = new();
            countField.style.marginTop = countField.style.marginBottom = 0;
            countField.style.marginLeft = 0;
            countField.style.marginRight = 2;
            var textElement = countField.Q<TextElement>();
            textElement.style.paddingLeft = 2;
            textElement.style.marginLeft = 0;

            countField.isDelayed = true;
            countField.BindProperty(itemsProperty.FindPropertyRelative("Array.size"));

            header.Add(spacer);
            header.Add(countField);

            // 定义列表项的生成逻辑
            listView.makeItem = () =>
            {
                VisualElement row = new();
                row.style.flexDirection = FlexDirection.Row;

                VisualElement dragHandle = CreateDragHandle();

                VisualElement pair = new();
                pair.style.flexGrow = 1;
                pair.style.flexDirection = FlexDirection.Column;
                pair.style.justifyContent = Justify.SpaceBetween;
                pair.style.paddingRight = 4;

                PropertyField keyField = new();
                keyField.style.flexGrow = 1;
                keyField.style.marginRight = 5;

                PropertyField valueField = new();
                valueField.style.flexGrow = 1;

                pair.Add(keyField);
                pair.Add(valueField);

                row.Add(dragHandle);
                row.Add(pair);
                return row;
            };

            // 6. 定义列表项的绑定和重复键检测逻辑
            listView.bindItem = (element, i) =>
            {
                if (i >= itemsProperty.arraySize) return;

                SerializedProperty itemProp = itemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = itemProp.FindPropertyRelative("Key");
                SerializedProperty valueProp = itemProp.FindPropertyRelative("Value");

                PropertyField keyF = element.Query<PropertyField>().First();
                PropertyField valueF = element.Query<PropertyField>().Last();

                keyF.BindProperty(keyProp);
                valueF.BindProperty(valueProp);

                keyF.label = "Key";
                valueF.label = "Value";

                // --- 重复键检测逻辑 ---
                string currentKey = keyProp.stringValue;
                bool isDuplicate = false;

                for (int j = 0; j < itemsProperty.arraySize; j++)
                {
                    if (j != i)
                    {
                        string otherKey = itemsProperty.GetArrayElementAtIndex(j).FindPropertyRelative("Key").stringValue;
                        if (currentKey == otherKey)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }
                }

                if (isDuplicate)
                {
                    keyF.style.backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.3f);
                }
                else
                {
                    keyF.style.backgroundColor = StyleKeyword.Null;
                }

                keyF.Unbind();
                keyF.BindProperty(keyProp);
                keyF.TrackPropertyValue(keyProp, prop =>
                {
                    listView.RefreshItems();
                });
            };

            //
            foldout.Add(listView);
            root.Add(foldout);

            return root;
        }

        private VisualElement CreateDragHandle()
        {
            // 1. 创建手柄的主容器
            VisualElement handleContainer = new VisualElement();
            handleContainer.style.width = 20;
            handleContainer.style.justifyContent = Justify.Center;
            handleContainer.style.alignItems = Align.Center;
            handleContainer.style.marginRight = 5;

            // 设置鼠标悬停时的原生拖拽光标 (四向箭头或手型)
            // handleContainer.style.cursor = new Cursor() { defaultCursorId = MouseCursor.Pan };

            // 2. 在容器内部生成两条原生样式的横线
            for (int i = 0; i < 2; i++)
            {
                VisualElement line = new VisualElement();
                line.style.width = 12;
                line.style.height = 2;
                line.style.marginTop = 1;
                line.style.marginBottom = 1;

                // 使用半透明灰色，这样无论是在深色 (Dark) 还是浅色 (Light) 主题下看起来都很自然
                line.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

                // 稍微加一点圆角，使其边缘柔和，完全贴合原生 UI 的质感
                line.style.borderTopLeftRadius = 1;
                line.style.borderTopRightRadius = 1;
                line.style.borderBottomLeftRadius = 1;
                line.style.borderBottomRightRadius = 1;

                handleContainer.Add(line);
            }

            return handleContainer;
        }
    }
}