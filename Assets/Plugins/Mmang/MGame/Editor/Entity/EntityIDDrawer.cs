using System.Collections.Generic;
using System.Linq;
using Mmang.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Game.Editors
{
    [CustomPropertyDrawer(typeof(EntityIDAttribute))]
    public class EntityIDDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var collectionSO = GlobalConfigAssets.GetConfigSerializedObject<EntityConfigCollection>();

            var map = BuildElementMap(property);

            VisualElement root = new();
            root.SetHorizontal();

            var popupField = UIElementHelper.CreateDropdownUInt(map, property.displayName, property);
            root.Add(popupField);

            //
            root.TrackSerializedObjectValue(collectionSO, evt =>
            {
                var newMap = BuildElementMap(property);
                var newPopupField = UIElementHelper.CreateDropdownUInt(newMap, property.displayName, property);

                UIElementHelper.RefreshDropdownMap(popupField, newMap, property.uintValue);
            });

            return root;
        }

        private Dictionary<uint, string> BuildElementMap(SerializedProperty property)
        {
             var entityIDAttribute = (EntityIDAttribute)attribute;
            var requiredTag = GameplayTag.CreateByName(entityIDAttribute.RequiredTag);

            Dictionary<uint, string> map = new();
            var collection = GlobalConfigAssets.GetConfigInstance<EntityConfigCollection>();
            if (collection == null || collection.IsError())
            {
                map.Add(property.uintValue, "ERROR");
            }
            else
            {
                bool isEmpty = true;
                foreach (var config in collection.Data)
                {
                    if (requiredTag.IsValid() && !requiredTag.IsRoot())
                    {
                        if (!config.EntityTags.Contains(requiredTag))
                        {
                            continue;
                        }
                    }

                    isEmpty = false;
                    string elementName = $"{config.EntityName} (ID: {config.ID})";
                    map.Add(config.ID, elementName);
                }

                if (isEmpty)
                {
                    map.Add(property.uintValue, "Missing");
                }
            }

            return map;
        }

    }
}
