using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mmang.Util;
using System.Reflection;
using UnityEditor.UIElements;

namespace Mmang.Generations
{
    public class GenerationPointComputeEditorWindow : EditorWindow
    {
        [SerializeField] private LayerMask m_PaintLayerMaskValue = ~0;
        [SerializeField] private float m_PaintSpacingValue = 0.3f;
        //[SerializeField] private float m_BrushSizeValue = 0.3f;
        [SerializeField] private GenerationPointBehaviour m_BindingBehaviour;

        //
        private ObjectField m_BindingObjectField;
        private GenerationPointComputeBase BindingObject => m_BindingObjectField?.value as GenerationPointComputeBase;
        private ObjectField m_GenerationSettingField;
        private GenerationPointBehaviour Setting => m_GenerationSettingField?.value as GenerationPointBehaviour;
        private GenerationPointSettingConfig SingleSetting => m_GenerationSettingField?.value as GenerationPointSettingConfig;
        //private GenerationPointSettingCollection SettingCollection => m_GenerationSettingField?.value as GenerationPointSettingCollection;
        private SerializedObject m_SettingObject;

        //
        private Toggle m_PaintMode;
        private VisualElement m_PaintModeGUI;
        private FloatField m_PaintSpacing;
        private LayerMaskField m_PaintLayerMask;
        private FloatField m_BrushSize;

        //
        private VisualElement m_GenerationGUI;

        //
        private Vector3 m_MousePosition;
        private RaycastHit[] m_HitResults = new RaycastHit[8];
        private Vector3 m_HitPosition, m_HitNormal, m_CacheHitPosition;
        private Vector3 m_PrePaintPosition;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }


        [MenuItem("Tools/GenerationPoint Tool")]
        private static void Init()
        {
            GenerationPointComputeEditorWindow window = (GenerationPointComputeEditorWindow)EditorWindow.GetWindow(typeof(GenerationPointComputeEditorWindow), false, "GenerationPoint Tool", true);
            var icon = EditorGUIUtility.FindTexture("tree_icon");

            window.titleContent = new GUIContent("GenerationPoint Tool", icon);
            window.Show();
        }

        public void CreateGUI()
        {
            // 绑定
            var bindingObject = GameObject.FindAnyObjectByType<GenerationPointComputeBase>();
            m_BindingObjectField = new("绑定")
            {
                objectType = typeof(GenerationPointComputeBase),
                value = bindingObject
            };

            // 绘制模式
            m_PaintMode = new("启用绘制模式") { value = false };
            m_PaintMode.RegisterValueChangedCallback((evt) => OnGenerationSettingChange());

            //
            m_PaintModeGUI = new();
            m_PaintSpacing = new("绘制间隔") { value = m_PaintSpacingValue };
            m_PaintSpacing.RegisterValueChangedCallback((evt) => m_PaintSpacingValue = evt.newValue);
            m_PaintLayerMask = new("绘制层级") { value = m_PaintLayerMaskValue };
            m_PaintLayerMask.RegisterValueChangedCallback((evt) => m_PaintLayerMaskValue = evt.newValue);
            m_PaintModeGUI.Add(m_PaintSpacing);
            m_PaintModeGUI.Add(m_PaintLayerMask);

            // 生成设置
            m_GenerationSettingField = new("生成设置")
            {
                objectType = typeof(GenerationPointBehaviour),
                value = m_BindingBehaviour
            };
            m_GenerationSettingField.RegisterValueChangedCallback((evt) =>
            {
                m_BindingBehaviour = (GenerationPointBehaviour)evt.newValue;
                OnGenerationSettingChange();
            });

            // 容器
            m_GenerationGUI = new();
            OnGenerationSettingChange();

            //
            rootVisualElement.Add(m_BindingObjectField);
            rootVisualElement.Add(m_PaintMode);
            rootVisualElement.Add(m_PaintModeGUI);
            rootVisualElement.Add(m_GenerationSettingField);
            rootVisualElement.Add(m_GenerationGUI);
        }

        private void OnGenerationSettingChange()
        {
            if (m_GenerationGUI == null)
            {
                m_SettingObject = null;
                return;
            }

            m_PaintModeGUI.SetDisplayFlexOrNone(m_PaintMode.value);

            m_GenerationGUI.Clear();

            if (Setting == null)
            {
                m_SettingObject = null;
                Button bt_CreateNewSetting = new(CreateNewSetting);
                bt_CreateNewSetting.Add(new Label("创建新设置"));
                Button bt_CreateNewCollection = new(CreateNewSettingCollection);
                bt_CreateNewCollection.Add(new Label("创建新设置集合"));
                m_GenerationGUI.Add(bt_CreateNewSetting);
                m_GenerationGUI.Add(bt_CreateNewCollection);
                return;
            }

            //
            m_SettingObject = new(Setting);

            if (Setting is GenerationPointSettingConfig)
                SingleGenerationGUI();
            else if (Setting is GenerationPointSettingCollection)
                SettingCollectionGUI();
        }

        #region Paint

        private void OnSceneGUI(SceneView sceneView)
        {
            if (hasFocus && m_PaintMode != null && m_PaintMode.value)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Event e = Event.current;
                m_MousePosition = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                m_MousePosition.y = sceneView.camera.pixelHeight - m_MousePosition.y * ppp;
                m_MousePosition.x *= ppp;
                m_MousePosition.z = 0;
                var sceneCamera = sceneView.camera;
                var ray = sceneCamera.ScreenPointToRay(m_MousePosition);

                int hits = Physics.RaycastNonAlloc(ray, m_HitResults, 200f, m_PaintLayerMaskValue);
                if (hits > 0)
                {
                    var nearestHit = ColliderUtil.GetNearestCollider(sceneCamera.transform.position, m_HitResults, hits);
                    m_HitPosition = nearestHit.point;
                    m_HitNormal = nearestHit.normal;
                }

                if (e.button == 0 && (e.type == EventType.MouseDrag))
                {
                    Paint();
                }

                DrawHandles(ray);
            }
        }

        private void Paint()
        {
            if (BindingObject == null
                || Setting is not GenerationPointSettingConfig generationSetting
                || Vector3.Distance(m_PrePaintPosition, m_HitPosition) <= m_PaintSpacing.value)
                return;

            // Test 只放置一个
            AddPoint(m_HitPosition, m_HitNormal, generationSetting);
            m_PrePaintPosition = m_HitPosition;
        }

        private void DrawHandles(Ray ray)
        {
            //
            Color rawColor = Handles.color;

            //
            Color discColor = Color.green;
            Color discColor2 = new(0, 0.5f, 0, 0.4f);

            Handles.color = discColor;
            Handles.DrawWireDisc(m_HitPosition, m_HitNormal, 1f);
            Handles.color = discColor2;
            Handles.DrawSolidDisc(m_HitPosition, m_HitNormal, 1f);

            //
            Handles.color = rawColor;

            if (m_HitPosition != m_CacheHitPosition)
            {
                SceneView.RepaintAll();
                m_CacheHitPosition = m_HitPosition;
            }


        }

        #endregion

        #region Generation GUI

        private void CreateNewSetting()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create new setting", "NewSetting", "asset", "Please enter a filename");
            if (string.IsNullOrEmpty(path))
                return;

            var newSetting = ScriptableObject.CreateInstance<GenerationPointSettingConfig>();
            AssetDatabase.CreateAsset(newSetting, path);

            // TODO 验证下实例和加载出来的资源实例是否相同
            var settingAsset = AssetDatabase.LoadAssetAtPath<GenerationPointSettingConfig>(path);
            if (m_GenerationSettingField != null && settingAsset != null)
                m_GenerationSettingField.value = settingAsset;
        }

        private void CreateNewSettingCollection()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create new collection", "NewCollection", "asset", "Please enter a filename");
            if (string.IsNullOrEmpty(path))
                return;

            var newSetting = ScriptableObject.CreateInstance<GenerationPointSettingCollection>();
            AssetDatabase.CreateAsset(newSetting, path);

            var settingAsset = AssetDatabase.LoadAssetAtPath<GenerationPointSettingCollection>(path);
            if (m_GenerationSettingField != null && settingAsset != null)
                m_GenerationSettingField.value = settingAsset;
        }

        private void CreateAdditionalSetting(System.Type settingType)
        {
            if (!typeof(GenerationPointAdditionalSetting).IsAssignableFrom(settingType))
            {
                Debug.Log(settingType.Name + " 未继承 GenerationPointAdditionalSetting");
                return;
            }
            SingleSetting.AdditionalSetting = System.Activator.CreateInstance(settingType) as GenerationPointAdditionalSetting;
        }

        private bool UpdateAdditionalSetting()
        {
            if (BindingObject == null)
                return true;
            var additionalSettingAttribute = BindingObject.GetType().GetCustomAttribute<AdditionalSettingAttribute>();
            if (additionalSettingAttribute == null || additionalSettingAttribute.settingType == null)
                return true;

            if (SingleSetting.AdditionalSetting == null)
            {
                // 新建选项
                Button bt_CreateAdditionalSetting = new(() =>
                {
                    CreateAdditionalSetting(additionalSettingAttribute.settingType);
                    OnGenerationSettingChange(); //刷新UI
                });
                bt_CreateAdditionalSetting.Add(new Label("创建并绑定" + additionalSettingAttribute.settingType.Name));
                m_GenerationGUI.Add(bt_CreateAdditionalSetting);
                return true;
            }
            else
            {
                var curType = SingleSetting.AdditionalSetting.GetType();
                return curType == additionalSettingAttribute.settingType;
            }
        }

        private void BindSettingProperty(IBindable element, string propertyName)
        {
            element.BindProperty(m_SettingObject.FindProperty(propertyName));
        }

        private void SingleGenerationGUI()
        {
            // 允许开启绘制模式
            m_PaintMode.SetEnabled(true);

            // 阻挡层级
            LayerMaskField generationBlockLayerField = new("Generation Block LayerMask");
            BindSettingProperty(generationBlockLayerField, GenerationPointSettingConfig.PN_GenerationBlockLayer);

            // 过滤选中物体的碰撞体
            Toggle filterSelectionColliderField = new("Filter Selection Collider");
            BindSettingProperty(filterSelectionColliderField, GenerationPointSettingConfig.PN_FilterSelectionCollider);

            // 密度设置
            Slider generationDensityField = UIElementHelper.CreateSliderWithValueDisplay("Generation Density", 0.01f, 3f);
            BindSettingProperty(generationDensityField, GenerationPointSettingConfig.PN_GenerationDensity);

            // 生成数量
            IntegerField generationMaxCountField = new("Generation Max Count");
            BindSettingProperty(generationMaxCountField, GenerationPointSettingConfig.PN_GenerationMaxCount);

            // 偏移高度
            MinMaxSlider offsetHeightField = UIElementHelper.CreateMinMaxSliderWithValueDisplay("Offset Height", 0f, 3f);
            BindSettingProperty(offsetHeightField, GenerationPointSettingConfig.PN_OffsetHeight);

            // 法线限制
            Toggle enableNormalLimitField = new("Enable Normal Limit");
            Vector3Field targetNormalField = new("Target Normal");
            FloatField angleLessThanField = new("Angle Less Than") { value = 45f };
            UIElementHelper.BindToggleGroup(enableNormalLimitField, targetNormalField, angleLessThanField);

            BindSettingProperty(enableNormalLimitField, GenerationPointSettingConfig.PN_EnableNormalLimit);
            BindSettingProperty(targetNormalField, GenerationPointSettingConfig.PN_TargetNormal);
            BindSettingProperty(angleLessThanField, GenerationPointSettingConfig.PN_AngleLessThan);

            // 附加设置
            bool matchAdditionalSettingType = UpdateAdditionalSetting();
            m_GenerationGUI.SetEnabled(matchAdditionalSettingType);
            if (!matchAdditionalSettingType)
            {
                m_GenerationGUI.Add(UIElementHelper.CreateSimpleGroupBox(new Label("附加设置类型匹配错误")));
            }
            //m_AdditionalSettingField = UIElementHelper.CreateSimpleGroupBox();
            //var p = m_SettingObject.FindProperty("m_AdditionalSetting");


            VisualElement additionalSettingField = UIElementHelper.CreateSimpleGroupBox
            (
                UIElementHelper.DrawIncludeProperty(m_SettingObject.FindProperty("m_AdditionalSetting"), bind: true)
            );

            // 生成按钮
            Button bt_GenerationFromSelection = new(() => GenerationFromSelection());
            bt_GenerationFromSelection.Add(new Label("Generation From Selection"));


            // 添加
            m_GenerationGUI.Add(generationBlockLayerField);
            //m_GenerationGUI.Add(m_FilterSelectionColldierField);
            m_GenerationGUI.Add(generationDensityField);
            m_GenerationGUI.Add(generationMaxCountField);

            m_GenerationGUI.Add(new Label());
            m_GenerationGUI.Add(offsetHeightField);

            m_GenerationGUI.Add(new Label());
            m_GenerationGUI.Add(enableNormalLimitField);
            m_GenerationGUI.Add(targetNormalField);
            m_GenerationGUI.Add(angleLessThanField);

            m_GenerationGUI.Add(new Label());
            m_GenerationGUI.Add(additionalSettingField);

            m_GenerationGUI.Add(new Label());
            m_GenerationGUI.Add(bt_GenerationFromSelection);
        }

        private void SettingCollectionGUI()
        {
            // 不允许开启绘制模式
            m_PaintMode.SetEnabled(false);
            m_PaintMode.value = false;

            m_GenerationGUI.Add(UIElementHelper.CreateSimpleGroupBox(new Label("将进行集合操作")));

            // 操作类型
            EnumField collectionExecuteTypeField = new("Execute Type");
            BindSettingProperty(collectionExecuteTypeField, GenerationPointSettingCollection.PN_ExecuteType);

            // 集合
            PropertyField collectionField = new();
            collectionField.BindProperty(m_SettingObject.FindProperty(GenerationPointSettingCollection.PN_Behaviours));

            // 生成按钮
            Button bt_GenerationFromSelection = new(() => GenerationFromSelection());
            bt_GenerationFromSelection.Add(new Label("Generation From Selection"));


            m_GenerationGUI.Add(collectionExecuteTypeField);
            m_GenerationGUI.Add(collectionField);

            m_GenerationGUI.Add(new Label());
            m_GenerationGUI.Add(bt_GenerationFromSelection);
        }

        private void GenerationFromSelection()
        {
            if (BindingObject == null || Setting == null)
                return;

            var selections = Selection.gameObjects;
            if (Setting is GenerationPointSettingConfig singleSetting)
                GenerationWithSetting(selections, singleSetting);
            else if (Setting is GenerationPointSettingCollection settingCollection)
                GenerationWithSettingCollection(selections, settingCollection);
            /*
            int count = selections == null ? 0 : selections.Length;
            if (count <= 0)
                return;
            
            for (int i = 0; i < count; i++)
            {
                GameObject go = selections[i];
                if (go == null)
                    continue;
                GenerationEditorUtil.GeneratePositions
                (
                    go,
                    BindingObject,
                    m_GenerationDensityField.value,
                    m_GenerationMaxCountField.value,
                    AddPoint
                );
            }
            */
        }

        private void GenerationWithSettingCollection(GameObject[] selections, GenerationPointSettingCollection settingCollection)
        {
            if (settingCollection.Type == GenerationPointSettingCollection.ExecuteType.All)
            {
                foreach (var setting in settingCollection.Behaviours)
                {
                    if (setting is GenerationPointSettingConfig singleSetting)
                        GenerationWithSetting(selections, singleSetting);
                    else if (setting is GenerationPointSettingCollection settingCollection1 && settingCollection1 != settingCollection)
                        GenerationWithSettingCollection(selections, settingCollection1);
                }
            }
        }

        private void GenerationWithSetting(GameObject[] selections, GenerationPointSettingConfig setting)
        {
            int count = selections == null ? 0 : selections.Length;
            if (count <= 0)
                return;

            for (int i = 0; i < count; i++)
            {
                GameObject go = selections[i];
                if (go == null)
                    continue;
                GenerationEditorUtil.GeneratePositions
                (
                    go,
                    setting.GenerationDensity,
                    setting.GenerationMaxCount,
                    (position, normal) => AddPoint(position, normal, setting)
                );
            }
        }

        static Collider[] m_ColliderCache = new Collider[1];
        private void AddPoint(Vector3 position, Vector3 normal, GenerationPointSettingConfig setting)
        {
            // 偏移
            if (setting.OffsetHeight.y > 0f)
                position += normal * RandomUtil.GetRandomValueInRange(setting.OffsetHeight);

            // 法线检测
            if (setting.EnableNormalLimit)
            {
                float angle = Vector3.Angle(normal, setting.TargetNormal);
                if (angle > setting.AngleLessThan)
                    return;
            }

            // 碰撞体检测
            int hit = Physics.OverlapBoxNonAlloc(position, Vector3.one * 0.2f, m_ColliderCache, Quaternion.identity, setting.GenerationBlockLayer);
            if (hit > 0)
                return;

            BindingObject.AddControlPoint(position, normal, setting.AdditionalSetting);
        }
        #endregion
    }

}