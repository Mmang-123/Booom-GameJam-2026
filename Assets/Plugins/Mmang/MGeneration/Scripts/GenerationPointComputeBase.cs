using System;
using System.Collections.Generic;
using System.Reflection;
using Mmang.Game;
using UnityEngine;

namespace Mmang.Generations
{
    /*
        编辑器负责编辑生成控制点Buffer 包含坐标、法线、附加信息
        ComputeShader 负责使用控制点Buffer实时生成包含所有顶点信息的Buffer
        最后传递给特殊的材质进行渲染
    */

    /*
        todo: 移植的时候注释掉了交互相关的代码，后面考虑重写下
    */

    // 这层抽象主要为编辑器提供接口
    /// <summary>
    /// 不要继承这个抽象类，请继承GenerationPointComputeBase<TControl, TGeneration>
    /// </summary>
    public abstract class GenerationPointComputeBase : MonoBehaviour
    {
        public abstract void AddControlPoint(Vector3 position, Vector3 normal, GenerationPointAdditionalSetting additionalSetting = null);
    
        public virtual void Editor_LoadConfigProperties() { }
        public virtual void Editor_CheckConfigProperties() { }
        public virtual void Editor_Refresh() { }
    }

    [ExecuteAlways]
    public abstract class GenerationPointComputeBase<TControl, TGeneration> : GenerationPointComputeBase
        where TControl : struct, IControlPointStruct
        where TGeneration : struct, IGenerationPointStruct
    {
        #region Name
        protected static readonly string KERNEL_INITPOINTS = "InitPoints";
        protected static readonly string KERNEL_GENERATION = "Generation";
        protected static readonly string BUFFER_CONTROLPOINTS = "_ControlPoints";
        protected static readonly string BUFFER_GENERATIONPOINTS = "_GenerationPoints";
        protected static readonly string BUFFER_INTERACTIONDATAS = "_InteractionDatas";
        protected static readonly string PROPERTY_CONTROLPOINTCOUNT = "_ControlPointCount";
        protected static readonly string PROPERTY_GENERATIONPOINTCOUNT = "_GenerationPointCount";
        protected static readonly string PROPERTY_INTERACTIONDATACOUNT = "_InteractionDataCount";
        #endregion


        //
        [SerializeField] private bool m_Preview = true;
        [SerializeField] private GenerationPointComputeConfig m_ComputeConfig;
        [SerializeField] private Material m_Material;
        [SerializeField] private UnityEngine.Rendering.ShadowCastingMode m_CastShadow;
        //[SerializeField] private List<ShaderFloatProperty> m_FloatProperties = new();
        //[SerializeField] private List<ShaderRangeFloatProperty> m_FloatRangeProperties = new();
        [SerializeField, PropertyContainer(typeof(float))]
        private PropertyContainer m_FloatProperties = new();
        [SerializeField, PropertyContainer(typeof(int))]
        private PropertyContainer m_IntProperties = new();

        [SerializeField] private List<TControl> m_ControlPointDatas = new(); // 后面隐藏掉

        // Compute Shader
        public ComputeShader ComputeShaderInstance { get; private set; }
        protected int m_Kernel_InitPoints;
        protected int m_Kernel_Generation;
        protected int m_DispatchSize;

        // Material
        public Material MaterialInstance { get; private set; }

        // Compute Buffer
        //protected ComputeBuffer m_ArgsBuffer;
        protected GraphicsBuffer m_ArgsBuffer;
        protected ComputeBuffer m_ControlPointBuffer;
        protected ComputeBuffer m_GenerationPointBuffer;

        public int ControlPointCount => m_ControlPointDatas.Count;
        public int GenerationPointCount => ControlPointCount * m_ComputeConfig.GenerationCountPerControlPoint;
        public int GenerationTriangleCount => GenerationPointCount / 3;

        // 包围盒
        private Bounds m_Bounds;

        // 
        [NonSerialized] private bool m_Inited;

        private readonly uint[] m_InitArgsBuffer = new uint[4]
        {
            0,  // Number of vertices to render (Calculated in the compute shader with "InterlockedAdd(_IndirectArgsBuffer[0].numVertices);")
            1,  // Number of instances to render (should only be 1 instance since it should produce a single mesh)
            0,  // Index of the first vertex to render
            0,  // Index of the first instance to render
        };

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_Preview)
            {
                m_Inited = false;
                return;
            }
#endif
            Init();
        }

        private void OnDisable()
        {
            if (m_Inited)
                Dispose();    
        }

        public override void Editor_Refresh() => Refresh();
        
        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (m_Inited)
                Dispose();
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_Preview)
            {
                m_Inited = false;
                return;
            }
#endif
            Init();
        }

        private void Dispose()
        {
            if (m_ArgsBuffer != null)
            {
                m_ArgsBuffer.Release();
                m_ArgsBuffer = null;
            }
            if (m_ControlPointBuffer != null)
            {
                m_ControlPointBuffer.Release();
                m_ControlPointBuffer = null;
            }
            if (m_GenerationPointBuffer != null)
            {
                m_GenerationPointBuffer.Release();
                m_GenerationPointBuffer = null;
            }
            m_Inited = false;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_Preview)
                return;
#endif
            if (!m_Inited)
                return;

            /*
#if UNITY_EDITOR
            if (Application.isPlaying && m_ComputeConfig.ReceiveInteractionBuffer)
            {
                ComputeShaderInstance.SetInt(PROPERTY_INTERACTIONDATACOUNT, ShaderInteractionManager.Instance.InteractorCount);
            }
            else
            {
                ComputeShaderInstance.SetInt(PROPERTY_INTERACTIONDATACOUNT, 0);   
            }
#else
            if (m_ComputeConfig.ReceiveInteractionBuffer)
            {
                ComputeShaderInstance.SetInt(PROPERTY_INTERACTIONDATACOUNT, ShaderInteractionManager.Instance.InteractorCount);   
            }
#endif
            */

            GenerationPoints();
            Render();
        }

        private void UpdateBounds()
        {
            // Get the bounds of all the points and then expand
            m_Bounds = new Bounds(m_ControlPointDatas[0].Position, Vector3.one);
            for (int i = 0; i < m_ControlPointDatas.Count; i++)
            {
                Vector3 target = m_ControlPointDatas[i].Position;
                m_Bounds.Encapsulate(target);
            }
        }

        #region Init
        private void Init()
        {
            if (m_ComputeConfig?.ComputeShader == null || m_Material == null || ControlPointCount <= 0)
            {
                m_Inited = false;
                return;
            }

#if UNITY_EDITOR
            /*
            if (!Application.isPlaying && m_ComputeConfig.ReceiveInteractionBuffer && ShaderInteractionManager.InstanceCanbeNull == null)
            {
                m_Inited = false;
                return;
            }
            */
#endif

            InitComputeShader();
            InitBuffer();
            InitMaterial();
            UpdateBounds();
            InitProperties();
            m_Inited = true;
            //InitPoints();
        }

        private void InitComputeShader()
        {
            ComputeShaderInstance = Instantiate(m_ComputeConfig.ComputeShader);
            m_Kernel_InitPoints = ComputeShaderInstance.FindKernel(KERNEL_INITPOINTS);
            m_Kernel_Generation = ComputeShaderInstance.FindKernel(KERNEL_GENERATION);

            // DispatchSize = 生成点数量 = 控制点数量 * m_GenerationCountPerControlPoint
            //m_DispatchSize = Mathf.CeilToInt(GenerationPointCount / 8f);

            if (m_ComputeConfig.GenerationType == EGenerationType.PerControlPoint)
                m_DispatchSize = Mathf.CeilToInt(ControlPointCount / 8f);
            else if (m_ComputeConfig.GenerationType == EGenerationType.PerTriangles)
                m_DispatchSize = Mathf.CeilToInt(GenerationTriangleCount / 8f);
            else if (m_ComputeConfig.GenerationType == EGenerationType.PerGenerationPoint)
                m_DispatchSize = Mathf.CeilToInt(GenerationPointCount / 8f);
        }

        private void InitBuffer()
        {
            var controlPointAttribute = typeof(TControl).GetCustomAttribute<ControlPointStructAttribute>();
            if (controlPointAttribute == null)
            {
                Debug.Log("控制点结构体缺少属性");
                return;
            }
            var generationPointAttribute = typeof(TGeneration).GetCustomAttribute<GenerationPointStructAttribute>();
            if (generationPointAttribute == null)
            {
                Debug.Log("生成点点结构体缺少属性");
                return;
            }

            // 绘制属性Buffer 动态裁剪之后才需要进行动态填充
            //m_ArgsBuffer = new(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            m_ArgsBuffer = new(GraphicsBuffer.Target.IndirectArguments, 1, 4 * sizeof(uint));
            m_ArgsBuffer.SetData(new uint[4] { (uint)GenerationPointCount, 1, 0, 0 });

            // 控制点Buffer
            int controlPointSize = controlPointAttribute.size;
            m_ControlPointBuffer = new(ControlPointCount, controlPointSize);

            // 生成点Buffer
            int generationPointSize = generationPointAttribute.size;
            m_GenerationPointBuffer = new(GenerationPointCount, generationPointSize);

            // 填充ComputeBuffer
            m_ControlPointBuffer.SetData(m_ControlPointDatas);
            m_GenerationPointBuffer.SetData(new TGeneration[GenerationPointCount]);

            // 为ComputeShader绑定Buffer
            ComputeShaderInstance.SetBuffer(m_Kernel_InitPoints, BUFFER_CONTROLPOINTS, m_ControlPointBuffer);
            ComputeShaderInstance.SetBuffer(m_Kernel_InitPoints, BUFFER_GENERATIONPOINTS, m_GenerationPointBuffer);
            ComputeShaderInstance.SetBuffer(m_Kernel_Generation, BUFFER_CONTROLPOINTS, m_ControlPointBuffer);
            ComputeShaderInstance.SetBuffer(m_Kernel_Generation, BUFFER_GENERATIONPOINTS, m_GenerationPointBuffer);
            /*
            if (m_ComputeConfig.ReceiveInteractionBuffer)
            {
                var buffer = ShaderInteractionManager.Instance.InteractionBuffer;
                ComputeShaderInstance.SetBuffer(m_Kernel_InitPoints, BUFFER_INTERACTIONDATAS, buffer);
                ComputeShaderInstance.SetBuffer(m_Kernel_Generation, BUFFER_INTERACTIONDATAS, buffer);
            }
            */

            // 为ComputeShader设置Property
                ComputeShaderInstance.SetInt(PROPERTY_CONTROLPOINTCOUNT, ControlPointCount);
            ComputeShaderInstance.SetInt(PROPERTY_GENERATIONPOINTCOUNT, GenerationPointCount);
        }

        private void InitMaterial()
        {
            MaterialInstance = new(m_Material);

            // 为Material绑定Buffer
            MaterialInstance.SetBuffer(BUFFER_GENERATIONPOINTS, m_GenerationPointBuffer);
        }

        private void InitProperties()
        {
            foreach (var property in m_FloatProperties.Properties)
            {
                if (property is Property<float> floatProperty)
                    SetFloatProperty(property.Name, floatProperty.Value);
            }
            foreach (var property in m_IntProperties.Properties)
            {
                if (property is Property<int> intProperty)
                    SetIntProperty(property.Name, intProperty.Value);
            }
        }

        private void InitPoints()
        {
            // Dispatch
            ComputeShaderInstance.Dispatch(m_Kernel_InitPoints, m_DispatchSize, 1, 1);
        }

        #endregion


        #region Dispatch/Render
        private void GenerationPoints()
        {
            // Dispatch
            ComputeShaderInstance.Dispatch(m_Kernel_Generation, m_DispatchSize, 1, 1);
        }

        private void Render()
        {
            RenderParams rp = new(MaterialInstance);
            rp.worldBounds = m_Bounds;
            rp.layer = gameObject.layer;
            rp.shadowCastingMode = m_CastShadow;
            
            Graphics.RenderPrimitivesIndirect(rp, MeshTopology.Triangles, m_ArgsBuffer);

            //Graphics.DrawProceduralIndirect(MaterialInstance, m_Bounds, MeshTopology.Triangles,
            //    m_ArgsBuffer, 0, null, null, m_CastShadow, true, gameObject.layer);
        }

        public override void AddControlPoint(Vector3 position, Vector3 normal, GenerationPointAdditionalSetting additionalSetting = null)
        {
            TControl newStruct = new();
            newStruct.SetPosition(position);
            newStruct.SetNormal(normal);
            if (additionalSetting != null)
                ApplyAdditionalSetting(additionalSetting, ref newStruct);
            m_ControlPointDatas.Add(newStruct);
        }

        protected void AddControlPoint(TControl controlPoint)
        {
            m_ControlPointDatas.Add(controlPoint);
        }

        protected virtual void ApplyAdditionalSetting(GenerationPointAdditionalSetting setting, ref TControl newStruct)
        {

        }

        #endregion

        #region Property

#if UNITY_EDITOR
        public override void Editor_LoadConfigProperties()
        {
            if (Application.isPlaying)
                return;
            m_FloatProperties.Clear();
            m_IntProperties.Clear();
            if (m_ComputeConfig == null)
                return;
            
            m_ComputeConfig.Properties.SelectTargetTypeProperties<float, int>(m_FloatProperties, m_IntProperties);
            //m_FloatProperties.AddRange(m_ComputeConfig.Properties.GetTargetTypeProperties<float>());
        }

        public override void Editor_CheckConfigProperties()
        {
            if (Application.isPlaying || m_ComputeConfig == null)
                return;
            foreach (var property in m_ComputeConfig.Properties.Properties)
            {
                if (!m_FloatProperties.Contains(property.Name))
                {
                    if (property is Property<float>)
                        m_FloatProperties.Add(property);
                }
                if (!m_IntProperties.Contains(property.Name))
                {
                    if (property is Property<int>)
                        m_IntProperties.Add(property);
                }
            }
        }
#endif

        public void SetFloatProperty(string propertyName, float value)
        {
            ComputeShaderInstance.SetFloat(propertyName, value);
            MaterialInstance.SetFloat(propertyName, value);
        }

        public void SetIntProperty(string propertyName, int value)
        {
            ComputeShaderInstance.SetInt(propertyName, value);
            MaterialInstance.SetInt(propertyName, value);
        }

        #endregion
    }


}