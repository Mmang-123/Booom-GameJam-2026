using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Mmang.Util;

namespace Mmang.Game.Editors
{
    [CustomPropertyDrawer(typeof(GameplayTag))]
    public class GameplayTagDrawer : PropertyDrawer
    {
        private StyleSheet m_StyleSheet;
        private SerializedObject m_SettingsSO;

        private string GetTagName(GameplayTag tag)
        {
            if (tag.IsValid())
            {
                return tag.GetTagName();
            }
            return "Missing";
        }

        private SerializedObject GetSettingsSO()
        {
            m_SettingsSO ??= new(GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>());
            return m_SettingsSO;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // uss
            m_StyleSheet = m_StyleSheet != null ? m_StyleSheet : AssetDatabase.LoadAssetAtPath<StyleSheet>(MGamePathStorage.GetStyleSheetPath("GameplayTag"));

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.styleSheets.Add(m_StyleSheet);
            root.AddToClassList("gameplay-tag-field");

            bool isArrayProperty = property.propertyPath.EndsWith("]");
            
            Label label = new() { text = ObjectNames.NicifyVariableName(property.name) };
            
            VisualElement tagDisplay = new() {};
            tagDisplay.style.flexDirection = FlexDirection.Row;
            tagDisplay.style.height = 20;
            tagDisplay.style.justifyContent = Justify.Center;
            tagDisplay.AddToClassList("gameplay-tag-display");
            tagDisplay.style.alignSelf = Align.Center;

            VisualElement tagDisplayImage = new();
            tagDisplayImage.AddToClassList("gameplay-tag-display-image");

            VisualElement tagDisplayName = new();
            tagDisplayName.style.flexGrow = 1;
            tagDisplayName.style.alignItems = Align.Center;
            tagDisplayName.style.justifyContent = Justify.Center;

            Label tagDisplayLabel = new(GetTagName(property.GetValue<GameplayTag>()));
            tagDisplayLabel.AddToClassList("gameplay-tag-display__label");
            
            tagDisplayName.Add(tagDisplayLabel);
            
            tagDisplay.Add(tagDisplayImage);
            tagDisplay.Add(tagDisplayName);

            // 点击事件
            tagDisplay.RegisterCallback<ClickEvent>(evt =>
            {
                Vector2 mousePos = evt.position;
                mousePos = GUIUtility.GUIToScreenPoint(mousePos);
                Rect popupRect = new(mousePos.x, mousePos.y, 0, 0);

                OpenGameplayEditWindow(property, popupRect);

                evt.StopPropagation();
            });

            // 监听更新
            // 一个element只能监听一个SO, 所以这里的监听者不一样..
            tagDisplayImage.TrackPropertyValue(property, evt =>
            {
                tagDisplayLabel.text = GetTagName(evt.GetValue<GameplayTag>());
            });
            tagDisplayName.TrackSerializedObjectValue(GetSettingsSO(), evt =>
            {
                tagDisplayLabel.text = GetTagName(property.GetValue<GameplayTag>());
            });

            if (!isArrayProperty)
                root.Add(label);
            root.Add(tagDisplay);
            root.AddManipulator(new AlignLabelManipulator(label));

            return root;
        }

        private void OpenGameplayEditWindow(SerializedProperty property, Rect popupRect)
        {
            GameplayTagEditorWindow popup = ScriptableObject.CreateInstance<GameplayTagEditorWindow>();

            popup.BindProperty(property);

            Vector2 popupSize = new(400, 600);
            popup.ShowAsDropDown(popupRect, popupSize);
        }

    }
}