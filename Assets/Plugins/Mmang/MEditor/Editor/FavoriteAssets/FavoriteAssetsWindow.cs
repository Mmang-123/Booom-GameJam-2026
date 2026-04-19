using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using Mmang.Util;
using System.Linq;
using Mmang;
using UnityEditor.UIElements;

public class FavoriteAssetsWindowToolkit : EditorWindow
{
    [System.Serializable]
    public class ToolkitData
    {
        public int MaxHistoryCount = 10;

        public int SelectedIndex;
        public List<Page> Pages = new();
        public bool SettingsMode;
    }


    [System.Serializable]
    public class Page
    {
        public string Name;
        public List<string> AssetGUIDs = new();

        public Page(string name)
        {
            Name = name;
        }
    }

    [SerializeField] private ToolkitData m_ToolkitData;
    [SerializeField] private Page m_RecentlySelectedPage;
    private Page m_CurrentPage;

    // 存储数据
    private const string PREFS_SELECTED_PAGE = "FavoriteAssets_Toolkit_PageData";

    // UI
    private VisualElement m_Toolbar;
    private PopupField<int> m_PageDropdown;
    private VisualElement m_PageContainer;

    private Button m_PlusButton;
    private Button m_SettingsButton;
    
    private VisualElement m_ListContainer;
    private ListView m_ListView;
    private Label m_CountLabel;

    [MenuItem("Tools/Favorite Assets")]
    public static void ShowWindow()
    {
        FavoriteAssetsWindowToolkit window = GetWindow<FavoriteAssetsWindowToolkit>();
        Texture icon = EditorGUIUtility.IconContent("Favorite Icon").image;
        window.titleContent = new GUIContent("Favorite", icon);
        window.minSize = new Vector2(250, 400);
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        SaveToolkitData();
    }

    public void CreateGUI()
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MEditorPathStorage.GetStyleSheetPath("MEditorWindow"));

        m_RecentlySelectedPage ??= new("Recently Selected");

        m_ToolkitData = null;
        if (EditorPrefs.HasKey(PREFS_SELECTED_PAGE))
        {
            var json = EditorPrefs.GetString(PREFS_SELECTED_PAGE);
            if (!string.IsNullOrEmpty(json))
            {
                m_ToolkitData = JsonUtility.FromJson<ToolkitData>(json);
            }
        }
        m_ToolkitData ??= new() { SelectedIndex = 0, Pages = new() { new Page("Default") } };

        VisualElement root = rootVisualElement;
        root.Clear();
        root.styleSheets.Add(styleSheet);
        root.style.paddingTop = 0;
        root.style.paddingBottom = 0;
        root.style.paddingLeft = 0;
        root.style.paddingRight = 0;

        BuildToolbar();
        root.Add(m_Toolbar);

        m_PageContainer = new();
        m_PageContainer.style.paddingTop = 4;
        m_PageContainer.style.paddingLeft = 4;
        m_PageContainer.style.paddingRight = 4;
        m_PageContainer.style.paddingBottom = 4;
        m_PageContainer.style.flexGrow = 1;
        
        root.Add(m_PageContainer);

        if (m_ToolkitData.SettingsMode)
            DisplaySettings();
        else
            OnPageChanged(0, m_PageDropdown.value);
    }

    private void BuildToolbar()
    {
        m_Toolbar ??= new();
        VisualElement toolbar = m_Toolbar;
        toolbar.Clear();

        toolbar.AddToClassList("m_toolbar");

        // Page
        Dictionary<int, string> pageSelection = new()
        {
            [0] = "Recently Selected",
        };

        for (int pageIndex = 0; pageIndex < m_ToolkitData.Pages.Count; pageIndex++)
        {
            pageSelection.Add(pageIndex + 1, m_ToolkitData.Pages[pageIndex].Name);
        }
        if (m_ToolkitData.SelectedIndex < 0 || m_ToolkitData.SelectedIndex >= m_ToolkitData.Pages.Count + 1)
        {
            m_ToolkitData.SelectedIndex = 0;
        }
        
        var dropdown = UIElementHelper.CreateDropdownInt(pageSelection, "", m_ToolkitData.SelectedIndex, false);
        m_PageDropdown = dropdown;
        dropdown.AddToClassList("m_toolbar_dropdown");
        dropdown.style.minWidth = 50;
        dropdown.style.maxWidth = 160;
        dropdown.RegisterValueChangedCallback(evt =>
        {
            m_ToolkitData.SettingsMode = false;
            OnPageChanged(evt.previousValue, evt.newValue);
        });

        VisualElement spacer = new();
        spacer.style.flexGrow = 1;
        spacer.style.flexShrink = 1;

        
        var plusButton = new Button();
        var plusImage = new VisualElement();
        plusButton.Add(plusImage);
        plusButton.AddToClassList("m_toolbar_button");
        plusImage.AddToClassList("m_toolbar_button_plus");
        plusButton.clicked += AddPage;
        m_PlusButton = plusButton;

        var settingsButton = new Button();
        var settingsImage = new VisualElement();
        settingsButton.Add(settingsImage);
        settingsButton.AddToClassList("m_toolbar_button");
        settingsImage.AddToClassList("m_toolbar_button_settings");
        settingsButton.clicked += OnSettingsButtonClicked;
        m_SettingsButton = settingsButton;

        toolbar.Add(dropdown);
        toolbar.Add(spacer);
        toolbar.Add(plusButton);
        toolbar.Add(settingsButton);

        UpdateToolbarDisplay();
    }

    private void AddPage()
    {
        string newPageName = $"Page {m_ToolkitData.Pages.Count + 1}";
        m_ToolkitData.Pages.Add(new Page(newPageName));
        m_ToolkitData.SelectedIndex = m_ToolkitData.Pages.Count;
        BuildToolbar();
        m_PageDropdown.value = m_ToolkitData.SelectedIndex;
    }

    private void OnSettingsButtonClicked()
    {
        m_ToolkitData.SettingsMode = !m_ToolkitData.SettingsMode;
        if (m_ToolkitData.SettingsMode)
        {
            UpdateToolbarDisplay();
            DisplaySettings();
        }
        else
        {
            BuildToolbar();
            OnPageChanged(0, m_PageDropdown.value);
        }
    }
    
    private void UpdateToolbarDisplay()
    {
        if (m_ToolkitData.SettingsMode)
        {
            m_PageDropdown.SetDisplayNone();
            m_PlusButton.SetDisplayNone();
            m_SettingsButton.AddToClassList("m_toolbar_button_active");
        }
        else
        {
            m_PageDropdown.SetDisplayFlex();
            m_PlusButton.SetDisplayFlex();
            m_SettingsButton.RemoveFromClassList("m_toolbar_button_active");
        }
    }

    private void OnPageChanged(int oldIndex, int index)
    {
        SaveToolkitData();

        if (m_ToolkitData.SettingsMode)
        {
            return;
        }

        if (index == 0)
        {
            DisplayRecentlySelectedPage();
        }
        else
        {
            DisplayNormalPage(index);
        }
    }

    private void DisplayRecentlySelectedPage()
    {
        m_PageContainer.Clear();
        m_CurrentPage = m_RecentlySelectedPage;

        // 列表视图
        m_ListContainer = new();
        m_ListView = new ListView();
        m_ListContainer.Add(m_ListView);

        m_ListContainer.style.flexGrow = 1;
        m_ListView.itemsSource = m_RecentlySelectedPage.AssetGUIDs;
        m_ListView.makeItem = MakeListItem;
        m_ListView.bindItem = BindListItem;
        m_ListView.selectionType = SelectionType.Single;
        m_ListView.fixedItemHeight = 24; // 固定高度
        m_ListView.selectionType = SelectionType.Multiple;
        m_ListView.reorderable = false;

        // 处理点击
        m_ListView.selectionChanged += OnSelectionChanged;
        m_ListView.itemsChosen += OnItemsChosen;

        // 点击到其他地方
        m_PageContainer.RegisterCallback<PointerDownEvent>(evt => 
        {
            // 如果点击的位置 不在 listView 的矩形范围内
            if (!m_ListView.worldBound.Contains(evt.position))
            {
                m_ListView.ClearSelection();
            }
        });

        m_PageContainer.Add(m_ListContainer);
    }

    private void DisplaySettings()
    {
        m_PageContainer.Clear();

        SerializedObject so = new(this);
        SerializedProperty pagesProperty = so.FindProperty("m_ToolkitData.Pages");
        PropertyField pagesField = new(pagesProperty) { label = "Pages" };
        pagesField.Bind(so);

        m_PageContainer.Add(pagesField);
    }

    private void DisplayNormalPage(int index)
    {
        m_PageContainer.Clear();
        //LoadFavorites();
        m_CurrentPage = m_ToolkitData.Pages[index - 1];

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.marginBottom = 10;

        m_CountLabel = new Label($"已收藏: {m_CurrentPage.AssetGUIDs.Count}");
        m_CountLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        m_CountLabel.style.alignSelf = Align.Center;

        var clearBtn = new Button(() => OnClearClicked()) { text = "清空" };
        
        header.Add(m_CountLabel);
        header.Add(clearBtn);
        m_PageContainer.Add(header);

        // 列表视图
        m_ListContainer = new();
        m_ListView = new ListView();
        m_ListContainer.Add(m_ListView);

        m_ListContainer.style.flexGrow = 1;
        m_ListView.itemsSource = m_CurrentPage.AssetGUIDs;
        m_ListView.makeItem = MakeListItem;
        m_ListView.bindItem = BindListItem;
        m_ListView.selectionType = SelectionType.Single;
        m_ListView.fixedItemHeight = 24; // 固定高度
        m_ListView.selectionType = SelectionType.Multiple;
        m_ListView.reorderable = true; // 可重排序
        m_ListView.viewDataKey = "favorite-assets-list-v1";

        m_ListView.itemIndexChanged += (srcIndex, dstIndex) => 
        {
            SaveToolkitData();
            RefreshList();
        };

        // 处理点击
        m_ListView.selectionChanged += OnSelectionChanged;
        m_ListView.itemsChosen += OnItemsChosen;

        header.RegisterCallback<DragUpdatedEvent>(OnDragUpdate, TrickleDown.TrickleDown);
        header.RegisterCallback<DragPerformEvent>(OnDragPerform, TrickleDown.TrickleDown);
        m_ListContainer.RegisterCallback<DragUpdatedEvent>(OnDragUpdate, TrickleDown.TrickleDown);
        m_ListContainer.RegisterCallback<DragPerformEvent>(OnDragPerform, TrickleDown.TrickleDown);

        // 点击到其他地方
        m_PageContainer.RegisterCallback<PointerDownEvent>(evt => 
        {
            // 如果点击的位置 不在 listView 的矩形范围内
            if (!m_ListView.worldBound.Contains(evt.position))
            {
                m_ListView.ClearSelection();
            }
        });

        m_PageContainer.Add(m_ListContainer);
    }

    private void OnSelectionChanged()
    {
        Object selectedObj = Selection.activeObject;

        if (selectedObj == null)
            return;

        if (AssetDatabase.Contains(selectedObj))
        {
            string path = AssetDatabase.GetAssetPath(selectedObj);
            string guid = AssetDatabase.AssetPathToGUID(path);
            AddToSelectedHistory(guid);
        }
    }

    private void AddToSelectedHistory(string objectGUID)
    {
        var history = m_RecentlySelectedPage.AssetGUIDs;
        if (history.Contains(objectGUID))
        {
            history.Remove(objectGUID);
        }

        history.Insert(0, objectGUID);

        if (history.Count > m_ToolkitData.MaxHistoryCount)
        {
            history.RemoveAt(history.Count - 1);
        }

        if (m_PageDropdown.value == 0)
        {
            m_ListView.Rebuild();
        }
    }

    private void OnDragUpdate(DragUpdatedEvent evt)
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        DragAndDrop.AcceptDrag();
        bool changed = false;
        foreach (var obj in DragAndDrop.objectReferences)
        {
            if (AddAsset(obj)) changed = true;
        }
        if (changed) RefreshList();
    }

    private void OnLostFocus()
    {
        m_ListView?.ClearSelection();    
    }

    private void OnItemsChosen(IEnumerable<object> items)
    {
        foreach (var item in items)
        {
            string guid = item as string;
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset != null)
                    {
                        AssetDatabase.OpenAsset(asset); 
                    }
                }
            }
        }
    }

    private VisualElement MakeListItem()
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.Center;
        container.style.paddingLeft = 5;

        // 选项菜单
        container.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) => 
        {
            var selections = m_ListView.selectedItems;
            if (evt.target is VisualElement element && element.userData is int index)
            {
                if (selections.Count() == 1)
                {
                    // Ping
                    evt.menu.AppendAction("Ping", (action) => 
                    {
                        PingItemAtIndex(index);
                    });   
                    evt.menu.AppendSeparator(); // 分隔线
                }

                // Remove
                evt.menu.AppendAction("Remove", (action) => 
                {
                    RemoveItemAtIndex(index);
                });
            }
            else 
            {
                evt.menu.AppendAction("...", (evt) => {});
            }
        }));

        var icon = new Image();
        icon.name = "Icon";
        icon.style.width = 16;
        icon.style.height = 16;
        icon.style.marginRight = 5;
        icon.style.flexShrink = 0;
        icon.pickingMode = PickingMode.Ignore;

        var label = new Label();
        label.name = "NameLabel";
        label.style.flexGrow = 1;
        label.style.flexShrink = 1;
        label.style.overflow = Overflow.Hidden;
        label.style.textOverflow = TextOverflow.Ellipsis;
        label.pickingMode = PickingMode.Ignore;

        container.Add(icon);
        container.Add(label);

        return container;
    }

    // ListView: 绑定数据 (Bind Item)
    private void BindListItem(VisualElement element, int index)
    {
        if (index >= m_CurrentPage.AssetGUIDs.Count) return;

        var guid = m_CurrentPage.AssetGUIDs[index];
        var path = AssetDatabase.GUIDToAssetPath(guid);
        
        var iconImg = element.Q<Image>("Icon");
        var label = element.Q<Label>("NameLabel");

        element.userData = index;

        if (string.IsNullOrEmpty(path))
        {
            iconImg.image = null;
            label.text = "资源丢失 (Missing)";
            label.style.color = Color.red;
        }
        else
        {
            // 获取资源图标
            Texture2D icon = AssetDatabase.GetCachedIcon(path) as Texture2D;
            iconImg.image = icon;
            label.text = Path.GetFileNameWithoutExtension(path);
            label.style.color = StyleKeyword.Null; // 重置颜色
        }
    }
    
    private void PingItemAtIndex(int index)
    {
        if (index >= 0 && index < m_CurrentPage.AssetGUIDs.Count)
        {
            PingByGuid(m_CurrentPage.AssetGUIDs[index]);
        }
    }

    private void PingByGuid(string guid)
    {
        if (!string.IsNullOrEmpty(guid))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }
            }
        }
    }

    private void RemoveItemAtIndex(int index)
    {
        if (index >= 0 && index < m_CurrentPage.AssetGUIDs.Count)
        {
            m_CurrentPage.AssetGUIDs.RemoveAt(index);
            SaveToolkitData();
            RefreshList();
        }
    }

    private void OnSelectionChanged(IEnumerable<object> selectedItems)
    {
        foreach (var item in selectedItems)
        {
            string guid = item as string;
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }
        }
    }

    private void OnClearClicked()
    {
        if (EditorUtility.DisplayDialog("确认", "清空列表?", "是的", "取消"))
        {
            m_CurrentPage.AssetGUIDs.Clear();
            SaveToolkitData();
            RefreshList();
        }
    }

    private bool AddAsset(Object obj)
    {
        if (obj == null) return false;
        string path = AssetDatabase.GetAssetPath(obj);
        if (!AssetDatabase.IsMainAsset(obj) && !AssetDatabase.IsSubAsset(obj)) return false;

        string guid = AssetDatabase.AssetPathToGUID(path);
        if (!m_CurrentPage.AssetGUIDs.Contains(guid))
        {
            m_CurrentPage.AssetGUIDs.Add(guid);
            SaveToolkitData();
            return true;
        }
        return false;
    }

    private void RefreshList()
    {
        m_CountLabel.text = $"已收藏: {m_CurrentPage.AssetGUIDs.Count}";
        m_ListView.Rebuild(); // 强制刷新列表
    }


    private void SaveToolkitData()
    {
        if (m_ToolkitData == null)
            return;

        m_ToolkitData.SelectedIndex = m_PageDropdown.value;
        string json = JsonUtility.ToJson(m_ToolkitData);
        EditorPrefs.SetString(PREFS_SELECTED_PAGE, json);
    }

}