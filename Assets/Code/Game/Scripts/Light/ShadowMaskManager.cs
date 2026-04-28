using System.Collections.Generic;
using Mmang.PixelartRender;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    public class ShadowMaskManager : SingletonMono<ShadowMaskManager>
    {
        private Material m_ReadShadowMaterial;
        
        private Dictionary<Vector2Int, Texture2D> m_TextureMap = new();
        private HashSet<Vector2Int> m_UpdatedSet = new();

        private void FixedUpdate()
        {
            m_UpdatedSet.Clear();
        }

        private void InitMaterial()
        {
            if (m_ReadShadowMaterial != null)
                return;
            
            Shader shader = Shader.Find("Hidden/Mmang/Pixelart/Blit/ReadShadow");
            m_ReadShadowMaterial = new(shader);
        }

        private Texture2D GetResultTexture2D(Vector2Int chunk)
        {
            if (m_TextureMap.TryGetValue(chunk, out var result))
            {
                return result;
            }

            var maskManager = ObstacleMaskManager.Instance;
            int resolution = maskManager.Resolution / 4;

            Texture2D newTexture = new(resolution, resolution, TextureFormat.R16, false);
            m_TextureMap.Add(chunk, newTexture);
            return newTexture;
        }

        private Texture2D UpdateShadowTexture(Vector2Int chunk)
        {
            InitMaterial();
            var maskManager = ObstacleMaskManager.Instance;

            //
            Vector2Int chunkIndex = chunk - maskManager.CenterIndex + Vector2Int.one;
            m_ReadShadowMaterial.SetVector(Shader.PropertyToID("_ChunkIndex"), new(chunkIndex.x, chunkIndex.y));

            //
            int resolution = maskManager.Resolution / 4;
            var descriptor = new RenderTextureDescriptor(resolution, resolution, RenderTextureFormat.RFloat, 0)
            {
                enableRandomWrite = true
            };
            RenderTexture tempRT = RenderTexture.GetTemporary(descriptor);

            //
            CommandBuffer cmd = new CommandBuffer() { name = "Read Shadow" };
            cmd.Blit(null, tempRT, m_ReadShadowMaterial);
            Graphics.ExecuteCommandBuffer(cmd);

            var texture = GetResultTexture2D(chunk);
            RenderTexture.active = tempRT;
            texture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            texture.Apply();


            RenderTexture.ReleaseTemporary(tempRT);
            return texture;
        }

        private Texture2D GetShadowTexture(Vector2Int chunk)
        {
            if (m_UpdatedSet.Contains(chunk))
            {
                return GetResultTexture2D(chunk);
            }
            else
            {
                if (!ObstacleMaskManager.Instance.IsVaildChunk(chunk))
                {
                    return null;
                }

                var result = UpdateShadowTexture(chunk);
                return result;
            }
        }

        public float GetShadow(Vector2 worldPosition)
        {
            var manager = ObstacleMaskManager.Instance;
            var chunkIndex = manager.GetChunkIndex(worldPosition, out Vector2 offsetInChunk);
            var texture = GetShadowTexture(chunkIndex);

            if (texture == null)
            {
                return 0f;
            }

            int resolution = manager.Resolution / 4;
            int pixelX = Mathf.FloorToInt(offsetInChunk.x * resolution);
            int pixelY = Mathf.FloorToInt(offsetInChunk.y * resolution);
            return texture.GetPixel(pixelX, pixelY).r;
        }
    }
}