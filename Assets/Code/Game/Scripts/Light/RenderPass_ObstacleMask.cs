using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Mmang.PixelartRender
{

    public class RenderPass_ObstacleMask : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "ObstacleMask Rendering";

        public ShaderTagId TargetShaderTag { get; private set; }
        private FilteringSettings m_FilteringSettings;

        public bool ClearDepthAfterRender = false;

        private class PassData
        {
            internal RendererListHandle RendererList;
            internal Vector4 Params;
            internal Vector2Int ChunkRange;
        }

        public RenderPass_ObstacleMask(LayerMask layerMask, ShaderTagId shaderTag)
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            TargetShaderTag = shaderTag;
        }


        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            PixelartBufferData pixelartBufferData = frameData.GetOrCreate<PixelartBufferData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, 0);

                var manager = ObstacleMaskManager.Instance;

                SortingCriteria sortingCriteria = cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(TargetShaderTag, renderingData, cameraData, lightData, sortingCriteria);
                var param = new RendererListParams(renderingData.cullResults, drawingSettings, m_FilteringSettings);
                passData.RendererList = renderGraph.CreateRendererList(param);
                passData.Params = new Vector4
                (
                    manager.UnitSize,
                    manager.CenterIndex.x,
                    manager.CenterIndex.y,
                    0.0f
                );
                passData.ChunkRange = manager.ChunkRange;

                builder.UseRendererList(passData.RendererList);

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    ExecutePass(rgContext.cmd, data);
                });
            }

        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler(s_PassTag)))
            {
                cmd.SetGlobalVector(Shader.PropertyToID("_ObstacleParams"), passData.Params);
                cmd.SetGlobalVector(Shader.PropertyToID("_ChunkRange"), new(passData.ChunkRange.x, passData.ChunkRange.y));
                cmd.SetGlobalFloat(PShaderPropertyID.UnitSize, passData.Params.x);
                cmd.ClearRenderTarget(true, true, Color.clear);
                cmd.DrawRendererList(passData.RendererList);
            }
        }

    }





    /*
    public class RenderPass_ObstacleMask : ScriptableRenderPass
    {
        private FilteringSettings m_FilteringSettings;
        private int m_Index;

        public RenderPass_ObstacleMask(int index)
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, ~0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 获取相机数据
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            var manager = ObstacleMaskManager.Instance;

            {
                string passName = $"DrawObstacle_{m_Index}";
                int index = m_Index;

                TextureHandle targetHandle = renderGraph.ImportTexture(manager.GetMaskHandel(index));

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    builder.SetRenderAttachment(targetHandle, 0, AccessFlags.Write);
                    passData.index = index;

                    SortingCriteria sortingCriteria = cameraData.defaultOpaqueSortFlags;
                    DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(PShaderTag.ObstacleMask, renderingData, cameraData, lightData, sortingCriteria);


                    RendererListParams listParams = new(renderingData.cullResults, drawingSettings, m_FilteringSettings);
                    RendererListHandle rendererListHandle = renderGraph.CreateRendererList(listParams);

                    passData.rendererList = rendererListHandle;
                    builder.UseRendererList(rendererListHandle);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Vector3 worldPos = manager.GetPositionByIndex(data.index);

                        Vector3 cameraPos = new(worldPos.x, worldPos.y, -10f);
                        Matrix4x4 viewMatrix = Matrix4x4.TRS(cameraPos, Quaternion.identity, Vector3.one).inverse;

                        float halfSize = 8f;
                        Matrix4x4 projMatrix = Matrix4x4.Ortho(-halfSize, halfSize, -halfSize, halfSize, 0.1f, 100f);
                        projMatrix = GL.GetGPUProjectionMatrix(projMatrix, true);

                        // 保存并替换矩阵
                        Matrix4x4 oldView = cameraData.GetViewMatrix();
                        Matrix4x4 oldProj = cameraData.GetProjectionMatrix();
                        
                        context.cmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
                        context.cmd.ClearRenderTarget(false, true, Color.clear);
                        context.cmd.DrawRendererList(data.rendererList);

                        // 恢复矩阵
                        context.cmd.SetViewProjectionMatrices(oldView, oldProj);
                    });
                }
            }
        }

        private class PassData
        {
            public int index;
            public RendererListHandle rendererList; // 用于跨域传递 Handle
        }
    }
    */


}