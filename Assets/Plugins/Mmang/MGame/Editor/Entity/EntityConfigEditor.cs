using UnityEngine;
using UnityEditor;
using Mmang.Editors;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Mmang.Util;

namespace Mmang.Game.Editors
{
    [CustomEditor(typeof(EntityConfig))]
    public class EntityConfigEditor : SOComponentContainerEditor
    {
        private VisualElement m_InfoBox;

        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();
            var componentContainer = root.Q<ComponentContainerView>();

            componentContainer.Insert(0, CreateInfoBlock());
            
            m_InfoBox = new();
            root.Insert(0, m_InfoBox);

            CheckID();
            return root;
        }

        private ComponentBlock CreateInfoBlock()
        {
            var infoBlock = new ComponentBlock("Entity");
            var configTarget = target as EntityConfig;

            //// ID ////            
            var idPropertyField = new VisualElement();
            idPropertyField.style.flexDirection = FlexDirection.Row;
            idPropertyField.style.marginLeft = 3;

            var idPropertyLabel = new Label("ID");
            IntegerField idPropertyInputField = new();
            idPropertyInputField.SetMargin(Vector4.zero);
            idPropertyInputField.style.flexGrow = 1;
            idPropertyField.Add(idPropertyLabel);
            idPropertyField.Add(idPropertyInputField);

            var idProperty = serializedObject.FindProperty("m_ID");
            idPropertyInputField.BindProperty(idProperty);

            idPropertyField.AddManipulator(new AlignLabelManipulator(idPropertyLabel));
            infoBlock.Add(idPropertyField);


            //// Name ////
            var nameProperty = serializedObject.FindProperty("m_EntityName");
            PropertyField namePropertyField = new(nameProperty);
            namePropertyField.Bind(serializedObject);
            infoBlock.Add(namePropertyField);


            //// Tags ////
            var tagsProperty = serializedObject.FindProperty("m_EntityTags");
            PropertyField tagsPropertyField = new(tagsProperty);
            tagsPropertyField.Bind(serializedObject);
            infoBlock.Add(tagsPropertyField);


            // 检测ID改变事件
            idPropertyInputField.isDelayed = true;
            idPropertyInputField.RegisterValueChangedCallback(evt =>
            {
                var entityConfig = target as EntityConfig;
                entityConfig.Editor_OnIDChanged(evt.previousValue);
                CheckID();
            });

            return infoBlock;
        }

        private void CheckID()
        {
            m_InfoBox.Clear();
            var entityConfig = target as EntityConfig;
            var configCollection = GlobalConfigAssets.GetConfigInstance<EntityConfigCollection>();
            if (configCollection.IsError())
            {
                var box = UIElementHelper.CreateWarningBox();
                box.Add(new Label("Entity Config Collection初始化错误"));
                m_InfoBox.Add(box);
                return;
            }

            if (!configCollection.Contains(entityConfig)
            && configCollection.ContainsID(entityConfig.ID))
            {
                var box = UIElementHelper.CreateWarningBox();
                box.Add(new Label($"已存在ID为 {entityConfig.ID} 的配置文件"));
                m_InfoBox.Add(box);
                return;
            }
        }

        
    }
}