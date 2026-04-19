using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Mmang.Util;

namespace Mmang.Game.Editors
{
    public class GameplayTagEditorWindow : EditorWindow
    {
        public enum EGameplayTagEditType
        {
            None, Single, Container
        }

        private TreeView m_TreeView;

        private SerializedProperty m_SerializedProperty;
        private EGameplayTagEditType m_EditType;

        private readonly Dictionary<GameplayTagNode, int> m_NodeToIDMap = new();

        public void BindProperty(SerializedProperty serializedProperty)
        {
            m_SerializedProperty = serializedProperty;

            // 判断类型
            var type = m_SerializedProperty.GetPropertyType();
            if (typeof(GameplayTag).IsAssignableFrom(type))
            {
                m_EditType = EGameplayTagEditType.Single;
            }
            else if (typeof(GameplayTagContainer).IsAssignableFrom(type))
            {
                m_EditType = EGameplayTagEditType.Container;
            }
            else
            {
                m_EditType = EGameplayTagEditType.None;
            }
        }

        public void CreateGUI()
        {
            // uss
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MGamePathStorage.GetStyleSheetPath("GameplayTagEditor"));

            VisualElement root = rootVisualElement;
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");

            VisualElement container = new();
            container.AddToClassList("container");

            m_TreeView = new TreeView();

            m_TreeView.style.flexGrow = 1;
            m_TreeView.fixedItemHeight = 26;
            m_TreeView.style.marginTop = 5;

            m_TreeView.makeItem = MakeTagItem;
            m_TreeView.bindItem = BindTagItem;

            m_TreeView.viewDataKey = "GameplayTagTreeState_v1";

            VisualElement headerContainer = new();
            headerContainer.style.flexDirection = FlexDirection.Row;

            Button expandAllButton = new() { text = "Expand All" };
            Button collapseAllButton = new() { text = "Collapse All" };
            expandAllButton.clicked += () => { m_TreeView.ExpandAll(); };
            collapseAllButton.clicked += () => { m_TreeView.CollapseAll(); };
            expandAllButton.style.flexGrow = 1;
            collapseAllButton.style.flexGrow = 1;

            headerContainer.Add(expandAllButton);
            headerContainer.Add(collapseAllButton);

            container.Add(headerContainer);
            container.Add(m_TreeView);
            root.Add(container);

            ReloadTreeData();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
        
        private void OnUndoRedo()
        {
            if (m_TreeView != null)
            {
                m_TreeView.Rebuild();
            }
        }

        private VisualElement MakeTagItem()
        {
            var itemRoot = new VisualElement();
            itemRoot.AddToClassList("item-root");

            var toggle = new Toggle();
            toggle.style.marginLeft = 2;
            toggle.style.flexGrow = 1;
            itemRoot.Add(toggle);
            return itemRoot;
        }

        private void BindTagItem(VisualElement element, int index)
        {
            var tagNode = m_TreeView.GetItemDataForIndex<GameplayTagNode>(index);
                
            var toggle = element.Q<Toggle>();

            if (tagNode != null && toggle != null)
            {
                toggle.text = tagNode.NodeName;

                if (m_EditType == EGameplayTagEditType.Single)
                {
                    GameplayTag propertyValue = m_SerializedProperty.GetValue<GameplayTag>();
                    toggle.SetValueWithoutNotify(propertyValue.Contains(tagNode));

                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        if (m_SerializedProperty != null)
                            OnEditSingleTag(tagNode, evt.newValue);
                    });
                }
                else
                {
                    toggle.SetValueWithoutNotify(false);
                }
            }
        }

        private void OnEditSingleTag(GameplayTagNode tagNode, bool check)
        {
            var tree = GameplayTagsSettings.Tree;
            if (!tree.ContainsTag(tagNode))
            {
                return;
            }

            // 更新property
            string newGuid = check ? tagNode.Guid : tree.GetDirectParent(tagNode).Guid;
            Undo.RegisterCompleteObjectUndo(m_SerializedProperty.serializedObject.targetObject, $"GameplayTag Changed");
            m_SerializedProperty.SetValue<GameplayTag>(new(newGuid));
            EditorUtility.SetDirty(m_SerializedProperty.serializedObject.targetObject);

            //
            m_TreeView.Rebuild();
        }

        private void ReloadTreeData()
        {
            var tree = GameplayTagsSettings.Tree;
            var rootNode = tree.RootNode;

            m_NodeToIDMap.Clear();
            int id = 0;

            TreeViewItemData<GameplayTagNode> ExpandNode(GameplayTagNode node)
            {
                var children = tree.GetDirectChildrenNodes(node);

                List<TreeViewItemData<GameplayTagNode>> childrenData = new();
                int childID = 0;
                foreach (var child in children)
                {
                    childrenData.Add(ExpandNode(child));
                    childID++;
                }

                m_NodeToIDMap.Add(node, id);

                return new TreeViewItemData<GameplayTagNode>(id++, node, childrenData);
            }

            var rootData = ExpandNode(rootNode);
            m_TreeView.SetRootItems(rootData.children.ToList());

            m_TreeView.Rebuild();
        }
    }
}