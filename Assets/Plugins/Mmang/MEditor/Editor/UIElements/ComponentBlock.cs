using Mmang.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Editors
{
    public class ComponentBlock : Foldout
    {
        public System.Action<GenericMenu> OnBuildContextMenu;

        public ComponentBlock(string title)
        {
            this.text = title;
            this.value = true; // 默认展开

            //
            Toggle headerToggle = this.Q<Toggle>(className: "unity-foldout__toggle");
            if (headerToggle != null)
            {
                headerToggle.style.height = 22;
                headerToggle.style.backgroundColor = new Color(0.243f, 0.243f, 0.243f);
                headerToggle.style.borderTopWidth = 1;
                headerToggle.style.borderBottomWidth = 0;
                headerToggle.SetBorderColor(new Color(0.12f, 0.12f, 0.12f));

                // 调整 Header 的尺寸和边距，撑满整行
                headerToggle.style.paddingTop = 0;
                headerToggle.style.paddingBottom = 0;
                headerToggle.style.marginTop = 0;
                headerToggle.style.marginBottom = 0;
                headerToggle.style.marginLeft = -30; // 抵消 Foldout 默认的左边距缩进
                headerToggle.style.marginRight = -6;
                headerToggle.style.paddingLeft = 18; // 为折叠小三角留出空间

                // 字体加粗
                Label titleLabel = headerToggle.Q<Label>();
                if (titleLabel != null)
                {
                    titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    titleLabel.style.fontSize = 12;
                }

                headerToggle[0][1].style.paddingTop = 0;
                headerToggle[0][1].style.paddingBottom = 1;

                Image icon = new Image();
                icon.image = EditorGUIUtility.IconContent("cs Script Icon").image;
                icon.style.width = 16;
                icon.style.height = 16;
                icon.style.marginRight = 8;
                icon.style.alignSelf = Align.Center;
                // 将图标插入到文本标签之前（索引 1 通常是复选框后的位置）
                headerToggle[0].Insert(1, icon);

                // 占位符
                VisualElement spacer = new VisualElement();
                spacer.style.flexGrow = 1;
                headerToggle[0].Add(spacer);

                // 菜单图标
                Image menuIcon = new Image();
                menuIcon.image = EditorGUIUtility.IconContent("pane options").image;
                menuIcon.style.width = 16;
                menuIcon.style.height = 16;
                menuIcon.style.marginRight = 5;
                menuIcon.style.alignSelf = Align.Center;

                // 添加简单的悬停效果
                menuIcon.RegisterCallback<MouseEnterEvent>(e => menuIcon.style.backgroundColor = new Color(1, 1, 1, 0.1f));
                menuIcon.RegisterCallback<MouseLeaveEvent>(e => menuIcon.style.backgroundColor = Color.clear);

                //
                menuIcon.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button == 0 || e.button == 1)
                    {
                        e.StopPropagation();
                        ShowContextMenu(menuIcon);
                    }
                });

                headerToggle.Add(menuIcon);

            }

            VisualElement container = contentContainer;
            if (container != null)
            {
                container.style.paddingTop = 2;
                container.style.paddingBottom = 2;
                container.style.paddingLeft = 15; // 内容缩进
                container.style.paddingRight = 5;

                container.style.borderTopColor = new Color(0.188f, 0.188f, 0.188f);
                container.style.borderTopWidth = 1;

                // 抵消父级的左边距，使背景色填满面板
                container.style.marginLeft = -15;
                container.style.marginRight = -6;
            }

            //
            this.SetMargin(Vector4.zero);
        }

        private void ShowContextMenu(VisualElement anchor)
        {
            GenericMenu menu = new GenericMenu();

            if (OnBuildContextMenu != null)
            {
                OnBuildContextMenu.Invoke(menu);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Empty"));
            }

            // 在图标的正下方弹出菜单
            menu.DropDown(anchor.worldBound);
        }
    }

    public class ComponentContainerView : VisualElement
    {
        private VisualElement m_Container;

        public ComponentContainerView()
        {
            m_Container = new();
            var bottom = new VisualElement();
            bottom.style.height = 1;
            bottom.SetMargin(Vector4.zero);
            // 抵消父级的左边距，使背景色填满面板
            bottom.style.marginLeft = -15;
            bottom.style.marginRight = -6;
            bottom.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f); // 作为底部边框颜色

            Add(m_Container);
            Add(bottom);
        }

        public void AddComponentBlock(ComponentBlock componentBlock)
        {
            m_Container.Add(componentBlock);
        }

        public void Insert(int index, ComponentBlock componentBlock)
        {
            m_Container.Insert(index, componentBlock);
        }
    }
}