using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Game
{
    /// <summary>
    /// 速度缓冲区渲染 Pass（双缓冲 + 相机重投影）。
    ///
    /// Pass 1: 读旧 buffer，按相机偏移 blit 到新 buffer（重投影）
    /// Pass 2: 在新 buffer 上绘制当前帧物体速度（叠加）
    /// Pass 3: 将新 buffer 设为 _VelocityBuffer 全局纹理（用 Native CB 绕过校验）
    /// </summary>
    public class VelocityBufferFeaturePass : ScriptableRenderPass
    {
        private const string k_ReprojectTag = "Velocity Reproject";
        private const string k_DrawTag = "Velocity Draw";
        private const string k_SetGlobalTag = "Velocity Set Global";

        private static readonly ShaderTagId s_VelocityShaderTag = new("Velocity");
        private static readonly int s_ShaderID_VelocityBuffer = Shader.PropertyToID("_VelocityBuffer");
        private static readonly Vector4 k_ScaleBias = new(1, 1, 0, 0);

        private static readonly int s_ShaderID_CameraDelta = Shader.PropertyToID("_CameraDelta");
        private static readonly int s_ShaderID_CameraWorldSize = Shader.PropertyToID("_CameraWorldSize");
        private static readonly int s_ShaderID_DeltaTime = Shader.PropertyToID("_DeltaTime");
        

        private readonly Material m_ReprojectMaterial;
        private RTHandle m_ReadRT;
        private RTHandle m_WriteRT;

        public VelocityBufferFeaturePass(RTHandle readRT, RTHandle writeRT, Material reprojectMaterial)
        {
            m_ReadRT = readRT;
            m_WriteRT = writeRT;
            m_ReprojectMaterial = reprojectMaterial;
            profilingSampler = new ProfilingSampler(k_DrawTag);
        }

        public void SwapRTs(RTHandle readRT, RTHandle writeRT)
        {
            m_ReadRT = readRT;
            m_WriteRT = writeRT;
        }

        public void SetCameraParams(Vector2 cameraDelta, Vector2 cameraWorldSize)
        {
            m_ReprojectMaterial.SetVector(s_ShaderID_CameraDelta, cameraDelta);
            m_ReprojectMaterial.SetVector(s_ShaderID_CameraWorldSize, cameraWorldSize);
            m_ReprojectMaterial.SetFloat(s_ShaderID_DeltaTime, Time.deltaTime);
        }

        // ---------------------------------------------------------------
        // RenderGraph
        // ---------------------------------------------------------------
        private class BlitPassData
        {
            public TextureHandle SourceRT;
            public Material Material;
        }

        private class DrawPassData
        {
            public RendererListHandle RendererList;
        }

        private class SetGlobalPassData
        {
            public RTHandle OutRT;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var sourceRT = renderGraph.ImportTexture(m_ReadRT);
            var targetRT = renderGraph.ImportTexture(m_WriteRT);
            var outputRTHandle = m_WriteRT;

            // -- 获取渲染上下文 --
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var lightData = frameData.Get<UniversalLightData>();

            var sortingCriteria = cameraData.defaultOpaqueSortFlags;
            var drawingSettings = RenderingUtils.CreateDrawingSettings(
                s_VelocityShaderTag, renderingData, cameraData, lightData, sortingCriteria);

            var filterSettings = new FilteringSettings(RenderQueueRange.all, -1);

            var rendererListParams = new RendererListParams(
                renderingData.cullResults, drawingSettings, filterSettings);
            var rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

            // ================================================================
            // Pass 1: 相机重投影
            // ================================================================
            using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>(k_ReprojectTag, out var blitData))
            {
                blitData.SourceRT = sourceRT;
                blitData.Material = m_ReprojectMaterial;

                builder.UseTexture(sourceRT, AccessFlags.Read);
                builder.SetRenderAttachment(targetRT, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc(static (BlitPassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.SourceRT, k_ScaleBias, data.Material, 0);
                });
            }

            // ================================================================
            // Pass 2: 绘制当前帧物体速度
            // ================================================================
            using (var builder = renderGraph.AddRasterRenderPass<DrawPassData>(k_DrawTag, out var drawData))
            {
                drawData.RendererList = rendererListHandle;

                builder.UseRendererList(rendererListHandle);
                builder.SetRenderAttachment(targetRT, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc(static (DrawPassData data, RasterGraphContext ctx) =>
                {
                    ctx.cmd.DrawRendererList(data.RendererList);
                });
            }

            // ================================================================
            // Pass 3: 将完成的 buffer 设为 _VelocityBuffer 全局纹理
            // 使用 AddUnsafePass + 原生 CommandBuffer 绕过 RenderGraph 的
            // "texture already set as fragment attachment" 校验
            // ================================================================
            using (var builder = renderGraph.AddUnsafePass<SetGlobalPassData>(k_SetGlobalTag, out var globalData))
            {
                globalData.OutRT = outputRTHandle;

                builder.UseTexture(targetRT, AccessFlags.Read);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (SetGlobalPassData data, UnsafeGraphContext ctx) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    cmd.SetGlobalTexture(s_ShaderID_VelocityBuffer, data.OutRT);
                });
            }
        }
    }
}
