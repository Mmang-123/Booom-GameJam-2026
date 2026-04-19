using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Mmang.Util;

namespace Mmang.Game.Editors
{
    [CustomPropertyDrawer(typeof(GameplayTagContainer))]
    public class GameplayTagContainerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var tagsProperty = property.FindPropertyRelative("m_Tags");
            PropertyField propertyField = new(tagsProperty)
            {
                label = property.displayName
            };

            return propertyField;
        }
    }

}