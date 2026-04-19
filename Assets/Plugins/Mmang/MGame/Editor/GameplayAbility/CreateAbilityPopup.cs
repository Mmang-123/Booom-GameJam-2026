using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mmang.Editors;
using Mmang.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Game.Editors
{
    using SearchTreeItem = SearchTreeItem<CreateAbilityPopup.NodeData>;

    public class CreateAbilityPopup : EditorWindow
    {
        internal class NodeData
        {
            public string AssetName;
            public int Order;
            public Type Type;
        }

        [MenuItem("Assets/Create/Gameplay/Create Ability", false, 80)]
        public static void ShowWindow()
        {
            var window = GetWindow<CreateAbilityPopup>(true, "Create Ability", true);
            window.Show();
        }

        private void OnLostFocus()
        {
            EditorApplication.delayCall += () =>
            {
                //Close();
            };
        }

        private void CreateGUI()
        {
            var searchTree = new SearchableTreeView<NodeData>();

            var data = BuildData();
            searchTree.SetData(data);

            searchTree.OnItemSelected += (item) =>
            {
                if (item.UserData is NodeData data)
                {
                    OnItemSelected(data);
                }
                // 选中后关闭窗口
                Close();
            };


            rootVisualElement.style.borderTopWidth = 1;
            rootVisualElement.style.borderBottomWidth = 1;
            rootVisualElement.style.borderLeftWidth = 1;
            rootVisualElement.style.borderRightWidth = 1;
            rootVisualElement.style.borderTopColor = Color.black;
            rootVisualElement.style.borderBottomColor = Color.black;
            rootVisualElement.style.borderLeftColor = Color.black;
            rootVisualElement.style.borderRightColor = Color.black;

            rootVisualElement.Add(searchTree);

            // 监听 ESC 关闭
            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape) Close();
            });
        }

        // --- 构建数据 ---
        private List<TreeViewItemData<SearchTreeItem>> BuildData()
        {
            List<(string, NodeData)> paths = new();

            var types = TypeCollectionManager.GetTypeList<CreateAbilityMenuAttribute>();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<CreateAbilityMenuAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                paths.Add((attribute.Path, new() { AssetName = attribute.AssetName, Order = attribute.Order, Type = type }));
            }

            var tree = PathTreeBuilder.Build(paths);

            int id = 0;

            SearchTreeItem Convert(PathTreeBuilder.Node<NodeData> node)
            {
                return new(node.Name, node.UserData, !node.IsLeaf);
            }

            TreeViewItemData<SearchTreeItem> BuildDataTree(PathTreeBuilder.Node<NodeData> node, out List<TreeViewItemData<SearchTreeItem>> children)
            {
                children = null;

                if (node.Children.Count > 0)
                {
                    children = new();

                    var list = node.Children.Values.ToList();
                    list.Sort((a, b) =>
                    {
                        int aOrder = a.UserData == null ? 0 : a.UserData.Order;
                        int bOrder = b.UserData == null ? 0 : b.UserData.Order;
                        return aOrder.CompareTo(bOrder);
                    });

                    foreach (var child in list)
                    {
                        var childItem = BuildDataTree(child, out _);
                        children.Add(childItem);
                    }
                }

                var item = new TreeViewItemData<SearchTreeItem>(id++, Convert(node), children);
                return item;
            }

            BuildDataTree(tree, out var children);
            return children;
        }

        private void OnItemSelected(NodeData nodeData)
        {
            if (nodeData.Type == null)
            {
                return;
            }

            ScriptableObject asset = CreateInstance(nodeData.Type);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = Path.GetDirectoryName(path);
            }

            string fileName = nodeData.AssetName + ".asset";
            
            ProjectWindowUtil.CreateAsset(asset, path + "/" + fileName);
        }
    }
}