using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{

    public class RenderPass_MForward : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "MForward Rendering";

        public ShaderTagId TargetShaderTag { get; private set; }
        private FilteringSettings m_FilteringSettings;

        public bool ClearColor = false;
        public bool ClearDepth = true;
        public Color BackgroundColor = new(0, 0, 0, 0);

        public bool ClearDepthAfterRender = false;

        private class PassData
        {
            internal RendererListHandle RendererList;
            internal bool ClearColor;
            internal bool ClearDepth;
            internal Color BackgroundColor;
            internal bool ClearDepthAfterRender;
        }

        public RenderPass_MForward(LayerMask layerMask, ShaderTagId shaderTag)
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            TargetShaderTag = shaderTag;
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            PixelartBufferData pixelartBufferData = frameData.GetOrCreate<PixelartBufferData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            /*
            var camera = cameraData.camera;
            var pixelartCamera = PixelartManager.Instance.GetPixelartCamera(camera, EPixelartCameraType.Cast);
            if (pixelartCamera == null) return;
            */

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                //UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;

                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                /*
                for (EPixelartBuffer bufferType = PRenderStage.RawDataStart; bufferType <= PRenderStage.RawDataEnd; bufferType++)
                {
                    TextureHandle bufferHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, PBuffer.GetBufferDescriptor(pixelartCamera.CameraData, bufferType), PBuffer.GetBufferName(bufferType), false);
                    builder.SetRenderAttachment(bufferHandle, (int)bufferType, AccessFlags.Write);
                    builder.SetGlobalTextureAfterPass(bufferHandle, PBuffer.GetBufferShaderProperty(bufferType));
                    pixelartBufferData.AddBuffer(bufferType, bufferHandle);
                }
                */

                
                //TextureHandle depthHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, PBuffer.GetBufferDescriptor(pixelartCamera.CameraData, EPixelartBuffer.Depth), PBuffer.GetBufferName(EPixelartBuffer.Depth), false);
                //builder.SetRenderAttachmentDepth(depthHandle, AccessFlags.Write);
                //builder.SetGlobalTextureAfterPass(depthHandle, PBuffer.GetBufferShaderProperty(EPixelartBuffer.Depth));
                //pixelartBufferData.AddBuffer(EPixelartBuffer.Depth, depthHandle);

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, 0);

                /*
                TextureHandle depthHandle = resourceData.activeDepthTexture;
                builder.SetRenderAttachmentDepth(depthHandle, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(depthHandle, PBuffer.GetBufferShaderProperty(EPixelartBuffer.Depth));
                pixelartBufferData.AddBuffer(EPixelartBuffer.Depth, depthHandle);
                */

                SortingCriteria sortingCriteria = cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(TargetShaderTag, renderingData, cameraData, lightData, sortingCriteria);
                var param = new RendererListParams(renderingData.cullResults, drawingSettings, m_FilteringSettings);
                passData.RendererList = renderGraph.CreateRendererList(param);
                passData.ClearColor = ClearColor;
                passData.ClearDepth = ClearDepth;
                passData.BackgroundColor = BackgroundColor;
                passData.ClearDepthAfterRender = ClearDepthAfterRender;
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
                cmd.ClearRenderTarget(passData.ClearDepth, passData.ClearColor, passData.BackgroundColor);
                cmd.DrawRendererList(passData.RendererList);
                
                if (passData.ClearDepthAfterRender)
                    cmd.ClearRenderTarget(true, false, passData.BackgroundColor);
            }
        }

    }
}