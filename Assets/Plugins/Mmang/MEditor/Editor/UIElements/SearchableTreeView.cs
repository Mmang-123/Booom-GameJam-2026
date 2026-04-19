using System;
using System.Collections.Generic;
using System.Linq;
using Mmang.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Editors
{

    public class SearchTreeItem<T>
    {
        public string Name;
        public T UserData;
        public bool IsFolder;

        public SearchTreeItem(string name, T userData = default, bool isFolder = false)
        {
            Name = name;
            UserData = userData;
            IsFolder = isFolder;
        }
    }

    public class SearchableTreeView<T> : VisualElement
    {
        public event Action<SearchTreeItem<T>> OnItemSelected;

        //
        private ToolbarSearchField m_SearchField;
        private TreeView m_TreeView;
        private List<TreeViewItemData<SearchTreeItem<T>>> m_FullData;

        private string m_SearchingText;
        private Dictionary<int, int> m_MatchedMap = new();

        public SearchableTreeView()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MEditorPathStorage.GetStyleSheetPath("SearchableTreeView"));
            this.styleSheets.Add(styleSheet);
            
            this.style.flexGrow = 1;
            this.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);

            // 搜索框
            m_SearchField = new ToolbarSearchField();
            m_SearchField.style.marginTop = 4;
            m_SearchField.style.marginLeft = 4;
            m_SearchField.style.marginRight = 4;
            m_SearchField.style.marginBottom = 4;
            m_SearchField.RegisterValueChangedCallback(evt => FilterData(evt.newValue));
            Add(m_SearchField);

            // TreeView
            m_TreeView = new TreeView();
            m_TreeView.style.flexGrow = 1; // 填满剩余空间
            m_TreeView.makeItem = () =>
            {
                var itemRoot = new VisualElement();
                itemRoot.AddToClassList("st-item-root");

                itemRoot.style.flexGrow = 1;
                itemRoot.style.flexDirection = FlexDirection.Row;
                itemRoot.style.alignItems = Align.Center;

                var preLabel = new Label() { name = "pre_label" };
                preLabel.AddToClassList("st-label");
                preLabel.SetPadding(Vector4.zero);

                var midLabel = new Label() { name = "mid_label" };
                midLabel.AddToClassList("st-label");
                midLabel.SetPadding(Vector4.zero);

                var postLabel = new Label() { name = "post_label" };
                postLabel.AddToClassList("st-label");
                postLabel.SetPadding(Vector4.zero);

                itemRoot.Add(preLabel);
                itemRoot.Add(midLabel);
                itemRoot.Add(postLabel);

                return itemRoot;
            };

            m_TreeView.bindItem = (element, index) =>
            {
                var item = m_TreeView.GetItemDataForIndex<SearchTreeItem<T>>(index);
                int id = m_TreeView.GetIdForIndex(index);
                //var label = element.Q<Label>();
                var parent = element.parent?.parent;

                var preLabel = element.Q<Label>("pre_label");
                var midLabel = element.Q<Label>("mid_label");
                var postLabel = element.Q<Label>("post_label");

                if (m_MatchedMap.TryGetValue(id, out int matchedIndex) && matchedIndex >= 0)
                {
                    int textLength = m_SearchingText.Length;
                    preLabel.text = item.Name[..matchedIndex];      
                    midLabel.text = item.Name.Substring(matchedIndex, textLength);
                    postLabel.text = item.Name[(matchedIndex + textLength)..];  

                    midLabel.AddToClassList("st-item-selected");
                    parent?.AddToClassList("st-row-selected");
                }
                else
                {
                    preLabel.text = string.Empty;
                    postLabel.text = string.Empty;
                    midLabel.text = item.Name;

                    midLabel.RemoveFromClassList("item-selected");
                    parent?.RemoveFromClassList("st-row-selected");
                }
                //label.text = item.Name;
            };

            // 监听选择
            m_TreeView.selectionChanged += OnSelectionChanged;

            Add(m_TreeView);
        }

        public void SetData(List<TreeViewItemData<SearchTreeItem<T>>> data)
        {
            m_FullData = data;
            RefreshTree(m_FullData);

            // 默认聚焦搜索框，提升体验
            m_SearchField.Focus();
        }

        private void RefreshTree(IList<TreeViewItemData<SearchTreeItem<T>>> data)
        {
            m_TreeView.SetRootItems(data);
            m_TreeView.Rebuild();
            m_TreeView.ExpandAll();
        }


        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = m_TreeView.selectedItem as SearchTreeItem<T>;
            if (selected == null) return;

            if (!selected.IsFolder)
            {
                OnItemSelected?.Invoke(selected);
            }
        }

        private void FilterData(string searchText)
        {
            m_SearchingText = searchText;
            m_MatchedMap.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                RefreshTree(m_FullData);
                return;
            }

            var filtered = new List<TreeViewItemData<SearchTreeItem<T>>>();
            foreach (var item in m_FullData)
            {
                var match = FilterRecursive(item, searchText);
                if (match != null) filtered.Add(match.Value);
            }

            RefreshTree(filtered);
        }

        private TreeViewItemData<SearchTreeItem<T>>? FilterRecursive(TreeViewItemData<SearchTreeItem<T>> node, string search, bool forceKept = false)
        {
            // 逻辑：名字包含搜索词
            int index = node.data.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            bool selfMatch = index >= 0;

            List<TreeViewItemData<SearchTreeItem<T>>> keptChildren = new();
        
            if (m_MatchedMap.ContainsKey(node.id))
            {
                m_MatchedMap[node.id] = index;
            }
            m_MatchedMap.Add(node.id, index);

            // 自身匹配, 子节点全部显示
            if (selfMatch || forceKept)
            {
                foreach (var child in node.children)
                {
                    var childResult = FilterRecursive(child, search, true);
                    if (childResult != null) keptChildren.Add(childResult.Value);
                }

                return new TreeViewItemData<SearchTreeItem<T>>(node.id, node.data, keptChildren);
            }

            if (node.children != null && node.children.Count() > 0)
            {
                foreach (var child in node.children)
                {
                    var childResult = FilterRecursive(child, search);
                    if (childResult != null) keptChildren.Add(childResult.Value);
                }
            }

            if (keptChildren != null && keptChildren.Count > 0)
            {
                return new TreeViewItemData<SearchTreeItem<T>>(node.id, node.data, keptChildren);
            }

            return null;
        }
    }
}