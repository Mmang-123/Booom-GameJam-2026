using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_BufferOutput : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Buffer Output";

        private EPixelartBuffer m_BufferType;

        private class PassData
        {
            internal TextureHandle Source;
        }

        public void SetOutputBufferType(EPixelartBuffer bufferType)
        {
            m_BufferType = bufferType;   
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var pixelartBufferData = frameData.GetOrCreate<PixelartBufferData>();
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var bufferHandle = pixelartBufferData.GetBuffer(m_BufferType);
            if (!bufferHandle.IsValid())
                return;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.UseTexture(bufferHandle);

                passData.Source = bufferHandle;

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    ExecutePass(rgContext.cmd, data);
                });
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            Blitter.BlitTexture(cmd, passData.Source, new Vector4(1, 1, 0, 0), 0, false);
        }
    }
}