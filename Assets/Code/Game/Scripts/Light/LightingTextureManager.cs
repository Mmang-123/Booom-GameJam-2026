using System.Collections.Generic;
using Mmang.PixelartRender;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    public class LightingTextureManager : SingletonMono<LightingTextureManager>
    {
        [SerializeField] private Shader m_ReadLightShader;
        [SerializeField] private uint m_UpdateIntervalFrameCount = 4;

        //
        private Material m_ReadShadowMaterial;        
        private Dictionary<Vector2Int, Texture2D> m_TextureMap = new();
        private HashSet<Vector2Int> m_UpdatedSet = new();
        private int m_CurrentFrameCount = 0;

        private void FixedUpdate()
        {
            if (m_CurrentFrameCount < m_UpdateIntervalFrameCount)
            {
                m_CurrentFrameCount++;
            }
            else
            {
                m_UpdatedSet.Clear();
                m_CurrentFrameCount = 0;   
            }
        }

        public void Clear()
        {
            m_UpdatedSet.Clear();
        }

        private void InitMaterial()
        {
            if (m_ReadShadowMaterial != null)
                return;
            
            //Shader shader = Shader.Find("Hidden/Mmang/Pixelart/Blit/ReadLighting");
            var shader = m_ReadLightShader;
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

        private Texture2D UpdateLightingTexture(Vector2Int chunk)
        {
            InitMaterial();
            var maskManager = ObstacleMaskManager.Instance;

            //
            // Float division so even N (fractional origin) aligns correctly with _MLightingTexture
            float fx = chunk.x - maskManager.CenterIndex.x + (maskManager.ChunkRange.x - 1) / 2.0f;
            float fy = chunk.y - maskManager.CenterIndex.y + (maskManager.ChunkRange.y - 1) / 2.0f;
            m_ReadShadowMaterial.SetVector(Shader.PropertyToID("_ChunkIndex"), new Vector4(fx, fy, 0, 0));

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

        private Texture2D GetLightingTexture(Vector2Int chunk)
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

                m_UpdatedSet.Add(chunk);
                var result = UpdateLightingTexture(chunk);
                return result;
            }
        }

        public float GetLightStrength(Vector2 worldPosition)
        {
            var maskManager = ObstacleMaskManager.Instance;
            var chunkIndex = maskManager.GetChunkIndex(worldPosition, out Vector2 offsetInChunk);
            var texture = GetLightingTexture(chunkIndex);

            if (texture == null)
            {
                return 0f;
            }

            int resolution = maskManager.Resolution / 4;
            int pixelX = Mathf.FloorToInt(offsetInChunk.x * resolution);
            int pixelY = Mathf.FloorToInt(offsetInChunk.y * resolution);
            return texture.GetPixel(pixelX, pixelY).r;
        }

        public bool InValidChunk(Vector2 worldPosition)
        {
            var maskManager = ObstacleMaskManager.Instance;
            var chunkIndex = maskManager.GetChunkIndex(worldPosition);
            return maskManager.IsVaildChunk(chunkIndex);
        }
    }
}