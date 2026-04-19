using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;
using Mmang.Util;

namespace Mmang.Editors
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.GetPropertyType() != typeof(int))
            {
                return UIElementHelper.CreateDefaultPropertyField(property);
            }
            
            var dropDownAttribute = (DropdownAttribute)attribute;

            Dictionary<int, string> map = null;
            if (dropDownAttribute.collection != null)
                map = DropdownCollectionManager.GetMap(dropDownAttribute.collection);
            else if (!string.IsNullOrWhiteSpace(dropDownAttribute.funcName))
            {
                var owner = property.GetOwner();
                var type = owner?.GetType();
                var method = type?.GetMethod(dropDownAttribute.funcName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                            BindingFlags.Public | BindingFlags.DeclaredOnly);
                
                if (method != null && !method.IsAbstract
                && method.ReturnParameter.ParameterType == typeof(Dictionary<int, string>)
                && method.GetParameters() is { } parameters
                && (parameters == null || parameters.Length == 0))
                {
                    var invoker = method.IsStatic ? null : owner;
                    map = (Dictionary<int, string>)method.Invoke(invoker, null);
                }
            }

            if (map == null)
                return UIElementHelper.CreateDefaultPropertyField(property);

            VisualElement root = new();
            root.SetHorizontal();

            var popupField = UIElementHelper.CreateDropdownInt(map, property.displayName, property);
            root.Add(popupField);
            return root;
        }

    }
}
