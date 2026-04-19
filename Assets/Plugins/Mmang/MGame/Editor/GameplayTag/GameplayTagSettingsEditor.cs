using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;

namespace Mmang.Game
{
    [CustomEditor(typeof(GameplayTagsSettings))]
    public class GameplayTagSettingsEditor : Editor
    {
        private TreeView m_TreeView;

        private SerializableGameplayTagNode m_VirtualRootNode;
        private readonly Dictionary<SerializableGameplayTagNode, SerializableGameplayTagNode> m_ParentMap = new();

        public override VisualElement CreateInspectorGUI()
        {
            // uss
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MGamePathStorage.GetStyleSheetPath("GameplayTagSettingsEditor"));

            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Column;
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            root.viewDataKey = "GameplayTagSettings_v1";

            // Tree View
            m_TreeView = new TreeView();

            m_TreeView.style.flexGrow = 1;
            m_TreeView.fixedItemHeight = 26;
            m_TreeView.style.marginTop = 5;

            m_TreeView.makeItem = MakeTagItem;
            m_TreeView.bindItem = BindTagItem;

            m_TreeView.viewDataKey = "TagSettingsTreeState_v1";

            // header
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


            // Create New Tag Field
            VisualElement createNewTagField = new();
            createNewTagField.style.flexDirection = FlexDirection.Row;

            TextField createNewTagTextField = new();
            createNewTagTextField.multiline = false;
            //createNewTagTextField.SetPlaceholder("创建新Tag...");
            createNewTagTextField.textEdition.placeholder = "创建新Tag...";
            createNewTagTextField.style.flexGrow = 1;

            Button createNewTagButton = new();
            createNewTagButton.AddToClassList("tag-button");
            createNewTagButton.text = "+";
            createNewTagButton.clicked += () =>
            {
                AddNewTag(createNewTagTextField.text);
            };

            createNewTagField.Add(createNewTagButton);
            createNewTagField.Add(createNewTagTextField);

            //
            root.Add(headerContainer);
            root.Add(createNewTagField);
            root.Add(m_TreeView);

            ReloadTreeData();

            return root;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            GameplayTagsSettings.Refresh();
            ReloadTreeData();
        }

        private VisualElement MakeTagItem()
        {
            var itemRoot = new VisualElement();
            itemRoot.style.flexDirection = FlexDirection.Row;

            Button createNewTagButton = new() { name = "tag-create-button" };
            createNewTagButton.AddToClassList("tag-button");
            createNewTagButton.text = "+";

            Button deleteNewTagButton = new() { name = "tag-delete-button" };
            deleteNewTagButton.AddToClassList("tag-button");
            deleteNewTagButton.text = "-";

            Button moveUpButton = new() { name = "tag-up-button" };
            moveUpButton.AddToClassList("tag-button");
            moveUpButton.text = "↑";

            Button moveDownButton = new() { name = "tag-down-button" };
            moveDownButton.AddToClassList("tag-button");
            moveDownButton.text = "↓";

            TextField textField = new();
            textField.isDelayed = true;
            textField.multiline = false;
            textField.style.flexGrow = 1;

            itemRoot.Add(createNewTagButton);
            itemRoot.Add(textField);
            itemRoot.Add(moveUpButton);
            itemRoot.Add(moveDownButton);
            itemRoot.Add(deleteNewTagButton);
            return itemRoot;
        }

        private void BindTagItem(VisualElement element, int index)
        {
            var tagNode = m_TreeView.GetItemDataForIndex<SerializableGameplayTagNode>(index);

            var textField = element.Q<TextField>();
            var createButton = element.Q<Button>("tag-create-button");
            var deleteButton = element.Q<Button>("tag-delete-button");
            var moveUpButton = element.Q<Button>("tag-up-button");
            var moveDownButton = element.Q<Button>("tag-down-button");

            if (tagNode != null)
            {
                textField.SetValueWithoutNotify(tagNode.NodeName);

                textField.RegisterValueChangedCallback(evt =>
                {
                    ModifyNodeName(tagNode, evt.newValue, textField);
                });
            
                createButton.RegisterCallback<ClickEvent>(evt =>
                {
                    m_TreeView.ExpandItem(m_TreeView.GetIdForIndex(index));
                    AddNewChild(tagNode);
                });

                deleteButton.RegisterCallback<ClickEvent>(evt =>
                {
                    DeleteNode(tagNode);
                });

                moveUpButton.RegisterCallback<ClickEvent>(evt =>
                {
                    MoveNode(tagNode, up: true);
                });

                moveDownButton.RegisterCallback<ClickEvent>(evt =>
                {
                    MoveNode(tagNode, up: false);
                });
            }
        }


        #region 编辑

        private string GetDifferentNodeName(List<SerializableGameplayTagNode> otherNodes, string tagName = "NewTag")
        {
            string newNodeName = tagName;
            int childCount = otherNodes.Count;
            int currentNodeNameNum = 1;
            for (int i = 0; i < childCount; i++)
            {
                if (otherNodes.Any(o => o.NodeName == newNodeName))
                {
                    currentNodeNameNum++;
                    newNodeName = $"{tagName}{currentNodeNameNum}";
                    continue;
                }
                break;
            }

            return newNodeName;
        }

        private void AddNewTag(string newTag)
        {
            var settings = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>();
            Undo.RegisterCompleteObjectUndo(settings, $"Add New Tag {newTag}");
            settings.Editor_AddNewTag(newTag);
            EditorUtility.SetDirty(settings);
            ReloadTreeData();
        }

        private void AddNewChild(SerializableGameplayTagNode tagNode)
        {
            var settings = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>();

            // 生成一个不会重复的节点名
            string newNodeName = GetDifferentNodeName(tagNode.Children);

            Undo.RegisterCompleteObjectUndo(settings, $"Tag Add New Child");
            tagNode.Children.Add(new(newNodeName));
            EditorUtility.SetDirty(settings);
            GameplayTagsSettings.Refresh();
            ReloadTreeData();
        }

        private void DeleteNode(SerializableGameplayTagNode tagNode)
        {
            var settings = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>();
            if (!m_ParentMap.TryGetValue(tagNode, out var parentNode))
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(settings, $"Delete Node {tagNode.NodeName}");
            if (parentNode == m_VirtualRootNode)
            {
                settings.Editor_GetNodes().Remove(tagNode);
            }
            else
            {
                parentNode.Children.Remove(tagNode);
            }
            EditorUtility.SetDirty(settings);
            GameplayTagsSettings.Refresh();
            ReloadTreeData();
        }

        private void ModifyNodeName(SerializableGameplayTagNode tagNode, string newNodeName, TextField textField)
        {
            if (tagNode.NodeName == newNodeName
            || !m_ParentMap.TryGetValue(tagNode, out var parentNode))
            {
                return;
            }

            var settings = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>();

            string differentNodeName;
            if (parentNode == m_VirtualRootNode)
            {
                differentNodeName = GetDifferentNodeName(settings.Editor_GetNodes(), newNodeName);
            }
            else
            {
                differentNodeName = GetDifferentNodeName(parentNode.Children, newNodeName);   
            }

            textField.SetValueWithoutNotify(differentNodeName);

            Undo.RegisterCompleteObjectUndo(settings, $"Rename Node {tagNode.NodeName} to {differentNodeName}");
            tagNode.NodeName = differentNodeName;
            EditorUtility.SetDirty(settings);

            GameplayTagsSettings.Refresh();
        }

        private void MoveNode(SerializableGameplayTagNode tagNode, bool up)
        {
            if (!m_ParentMap.TryGetValue(tagNode, out var parentNode))
            {
                return;
            }

            var settings = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>();

            var list = parentNode == m_VirtualRootNode ? settings.Editor_GetNodes() : parentNode.Children;
            if (list == null)
            {
                return;
            }

            int count = list.Count;
            int index = list.IndexOf(tagNode);
            int newIndex = index + (up ? -1 : 1);
            if (index < 0 || index >= count || newIndex < 0 || newIndex >= count)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(settings, $"Move Node");
            var temp = list[newIndex];
            list[newIndex] = tagNode;
            list[index] = temp;
            EditorUtility.SetDirty(settings);

            GameplayTagsSettings.Refresh();
            ReloadTreeData();
        }

        #endregion
        

        private void ReloadTreeData()
        {
            var tree = GameplayTagsSettings.Tree;
            var rootNodes = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>().Editor_GetNodes();
            m_VirtualRootNode = new SerializableGameplayTagNode("", rootNodes);

            int id = 0;
            m_ParentMap.Clear();

            TreeViewItemData<SerializableGameplayTagNode> ExpandNode(SerializableGameplayTagNode node)
            {
                var children = node.Children;

                List<TreeViewItemData<SerializableGameplayTagNode>> childrenData = new();
                foreach (var child in children)
                {
                    m_ParentMap.Add(child, node);
                    childrenData.Add(ExpandNode(child));
                }

                return new TreeViewItemData<SerializableGameplayTagNode>(id++, node, childrenData);
            }

            var rootData = ExpandNode(m_VirtualRootNode);
            m_TreeView?.SetRootItems(rootData.children.ToList());

            m_TreeView?.Rebuild();
        }
    }
}