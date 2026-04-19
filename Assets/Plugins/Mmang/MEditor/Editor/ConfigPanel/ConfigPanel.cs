using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mmang.Editors
{
    public class ConfigPanel : EditorWindow
    {
        [SerializeField] private ConfigAssets m_OpenedAssets;
        [SerializeField] private ScriptableObject m_SelectedConfig;

        // UI Elements
        // left
        private VisualElement m_Left;
        private SelectionContainer<ScriptableObject> m_Selections;
        // right
        private VisualElement m_Right;
        private Label m_RightTitle;
        private InspectorElement m_Inspector;

        //
        private Dictionary<ScriptableObject, SerializedObject> m_CachedSO = new();

        [MenuItem("Tools/Global Configs")]
        public static ConfigPanel OpenGlobalConfigPanel()
        {
            ConfigPanel window = CreateWindow<ConfigPanel>();
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(MEditorPathStorage.GetImageResourcePath("ConfigPanelIcon"));
            window.titleContent = new GUIContent("Global Configs", icon);
            window.Show();
            window.Focus();

            var globalConfigAssets = GlobalConfigAssets.Instance;
            window.SetAssets(globalConfigAssets);

            return window;
        }

        public void SetAssets(ConfigAssets assets)
        {
            m_OpenedAssets = assets;
            Refresh();
        }

        public void Refresh()
        {
            //
            m_Selections.ClearSelections();
            if (m_OpenedAssets != null)
            {
                foreach (var config in m_OpenedAssets.GetOrderedConfigs())
                {
                    AddConfigSelection(config);
                }

                if (m_SelectedConfig != null)
                {
                    bool selectSuccess = m_Selections.TrySelectObject(m_SelectedConfig);
                    if (!selectSuccess)
                    {
                        m_SelectedConfig = null;
                    }
                }
            }
        }

        public void CreateGUI()
        {
            // uss
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MEditorPathStorage.GetStyleSheetPath("ConfigPanel"));

            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            root.viewDataKey = "GlobalConfigPanel_v1";

            CreateLeft();
            CreateRight();

            root.Add(m_Left);
            root.Add(m_Right);

            Refresh();
        }

        private void CreateLeft()
        {
            m_Left = new();
            m_Left.AddToClassList("left_container");

            Box titleBox = new();
            titleBox.AddToClassList("left_title_box");
            titleBox.Add(new Label("Configs"));

            m_Selections = new();
            m_Selections.OnSelectedNull += () =>
            {
                DisplayConfig(null, null);
            };

            m_Left.Add(titleBox);
            m_Left.Add(m_Selections);
        }

        private void CreateRight()
        {
            m_Right = new();
            m_Right.AddToClassList("right_container");

            m_RightTitle = new Label();

            Box titleBox = new();
            titleBox.AddToClassList("right_title_box");
            titleBox.Add(m_RightTitle);

            ScrollView scrollView = new();
            m_Inspector = new();
            m_Inspector.viewDataKey = "gcp_inspector";
            scrollView.Add(m_Inspector);

            m_Right.Add(titleBox);
            m_Right.Add(scrollView);

            scrollView.viewDataKey = "gcp-scroll";
            m_Right.viewDataKey = "gcp-right";
        }

        private void AddConfigSelection(ConfigAssets.ConfigData configData)
        {
            Button button = new();
            button.Add(new Label(configData.Name));
            button.AddToClassList("selection_button");

            var element = m_Selections.AddSelection(configData.SO, button, () => DisplayConfig(configData.Name, configData.SO));
            button.clicked += () => m_Selections.SelectElement(element);
        }

        private SerializedObject GetSerializedObject(ScriptableObject obj)
        {
            if (m_CachedSO.TryGetValue(obj, out var result))
                return result;
            
            SerializedObject so = new(obj);
            m_CachedSO.Add(obj, so);
            return so;
        }

        private void DisplayConfig(string name, ScriptableObject config)
        {
            m_SelectedConfig = config;

            m_Inspector.Unbind();
            if (config == null)
            {
                m_RightTitle.text = string.Empty;
                return;
            }

            m_RightTitle.text = name;

            var so = GetSerializedObject(config);
            m_Inspector.Bind(so);
        }
    }
}