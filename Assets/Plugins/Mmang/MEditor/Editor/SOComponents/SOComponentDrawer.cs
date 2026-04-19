using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Editors
{
    public class SOComponentDrawer
    {
        public virtual void DrawComponentView(SerializedProperty property, ComponentBlock componentBlock)
        {
            var container = componentBlock.contentContainer;
            var endProperty = property.GetEndProperty();
            if (property.NextVisible(true))
            {
                do
                {
                    if (property.EqualContents(endProperty))
                        break;

                    PropertyField propertyField;
                    propertyField = new PropertyField() { name = "PropertyField:" + property.propertyPath };
                    propertyField.BindProperty(property.Copy());

                    container.Add(propertyField);
                }
                while (property.NextVisible(false));
            }
        }
    }
}