using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class TempTextureData : ContextItem
    {
        private Dictionary<string, TextureHandle> m_HandleMap = new();

        public void StoreTexture(string key, TextureHandle textureHandle)
        {
            if (m_HandleMap.ContainsKey(key))
            {
                m_HandleMap[key] = textureHandle;
                return;
            }
            m_HandleMap.Add(key, textureHandle);
        }

        public TextureHandle GetTexture(string key)
        {
            if (m_HandleMap.TryGetValue(key, out var result))
            {
                return result;
            }

            return TextureHandle.nullHandle;
        }

        public bool ContainsTexture(string key)
        {
            return m_HandleMap.ContainsKey(key);
        }

        public bool TryGetTexture(string key, out TextureHandle outHandle)
        {
            return m_HandleMap.TryGetValue(key, out outHandle);
        }

        public override void Reset()
        {
            m_HandleMap.Clear();
        }
    }

    public class RenderPass_TempTextureBlitBack : ScriptableRenderPass
    {
        private class PassData
        {
            public TextureHandle Source;
        }

        private string m_TextureName;
        
        private string GetPassTag() => m_TextureName + "BlitBack";

        public RenderPass_TempTextureBlitBack(string tempTextureName)
        {
            m_TextureName = tempTextureName;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var tempTextureData = frameData.GetOrCreate<TempTextureData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            if (!tempTextureData.TryGetTexture(m_TextureName, out var textureHandle))
            {
                return;
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(GetPassTag(), out var passData))
            {
                passData.Source = textureHandle;

                //
                builder.UseTexture(textureHandle, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

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