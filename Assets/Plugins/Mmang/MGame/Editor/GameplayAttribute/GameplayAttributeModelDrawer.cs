using System.Collections.Generic;
using System.Linq;
using Mmang.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Game.Editors
{
    /*
    [CustomPropertyDrawer(typeof(GameplayAttributeModelData))]
    public class GameplayAttributeModelDataDrawer : PropertyDrawer
    {
        private void SetPropertyStringValue(SerializedProperty property, string value)
        {
            property.serializedObject.Update();
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            List<string> selections = GameplayAttributeEditorManager.GetAttributes();
            if (selections == null || selections.Count == 0)
            {
                return UIElementHelper.CreateDefaultPropertyField(property);
            }

            VisualElement root = new();
            root.SetHorizontal();

            bool isArrayProperty = property.propertyPath.EndsWith("]");

            var currentValue = property.GetValue<GameplayAttributeModelData>();
            var modelNameProperty = property.FindPropertyRelative("m_ModelName");

            int index = selections.IndexOf(modelNameProperty.stringValue);
            if (index < 0)
            {
                index = 0;
                SetPropertyStringValue(modelNameProperty, selections[0]);
            }

            PopupField<string> popupField = new(selections.ToList(), index)
            {
                label = property.displayName,
                formatSelectedValueCallback = id => id,
                formatListItemCallback = id => id
            };
            popupField.BindProperty(modelNameProperty);

            popupField.SetFlexGrow();
            popupField.AddManipulator(new AlignLabelManipulator());

            root.Add(popupField);

            return root;
        }
    }
    */
}