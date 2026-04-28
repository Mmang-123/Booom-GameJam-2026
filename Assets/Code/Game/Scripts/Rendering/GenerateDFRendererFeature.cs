using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ThreadData = GenerateDFRendererFeaturePass.DFProcessingThread;

public class GenerateDFRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();

    GenerateDFRendererFeaturePass m_Pass;
    public GenerateDFRendererFeaturePass Pass => m_Pass;

    // ---------- Feature 自身的缓存（Pass 的 source of truth）----------
    int m_NextID = 0;
    readonly Dictionary<int, ThreadData> m_Cache = new Dictionary<int, ThreadData>();

    // ---------- 直接传结构体 ----------
    public int Pending(ThreadData thread)
    {
        int id = ++m_NextID;
        m_Cache[id] = thread;
        m_Pass?.PendingDFProcessingThread(id, thread);
        return id;
    }

    // ---------- 展开参数版本 ----------
    public int Pending(
        Texture sourceTexture,
        RenderTexture targetTexture,
        Vector2Int offset,
        float alphaThreshold          = 0.5f,
        bool  normalize               = true,
        bool  invertSelection         = false,
        int   nearestPointSearchRange = 0,
        int   extendPixels            = 0,
        bool  boundaryDistance        = false,
        bool  accurateDistance        = false,
        int   shaderPropertyID        = -1)
    {
        return Pending(new ThreadData
        {
            sourceTexture          = sourceTexture,
            targetTexture          = targetTexture,
            offset                 = offset,
            alphaThreshold         = alphaThreshold,
            normalize              = normalize,
            invertSelection        = invertSelection,
            nearestPointSearchRange = nearestPointSearchRange,
            extendPixels           = extendPixels,
            boundaryDistance       = boundaryDistance,
            accurateDistance       = accurateDistance,
            shaderPropertyID       = shaderPropertyID
        });
    }

    // ---------- 更新 ----------
    public void UpdateThread(int id, ThreadData thread)
    {
        if (!m_Cache.ContainsKey(id)) return;
        m_Cache[id] = thread;
        m_Pass?.UpdateDFProcessingThread(id, thread.sourceTexture, thread.targetTexture, thread.offset,
            thread.alphaThreshold, thread.normalize, thread.invertSelection,
            thread.nearestPointSearchRange, thread.extendPixels,
            thread.boundaryDistance, thread.accurateDistance, thread.shaderPropertyID);
    }

    public void UpdateThread(
        int   id,
        Texture sourceTexture,
        RenderTexture targetTexture,
        Vector2Int offset                 = default,
        float alphaThreshold              = 0.5f,
        bool  normalize                   = true,
        bool  invertSelection             = false,
        int   nearestPointSearchRange     = 0,
        int   extendPixels                = 0,
        bool  boundaryDistance            = false,
        bool  accurateDistance            = false,
        int   shaderPropertyID            = -1)
    {
        UpdateThread(id, new ThreadData
        {
            sourceTexture           = sourceTexture,
            targetTexture           = targetTexture,
            offset                  = offset,
            alphaThreshold          = alphaThreshold,
            normalize               = normalize,
            invertSelection         = invertSelection,
            nearestPointSearchRange = nearestPointSearchRange,
            extendPixels            = extendPixels,
            boundaryDistance        = boundaryDistance,
            accurateDistance        = accurateDistance,
            shaderPropertyID        = shaderPropertyID
        });
    }

    // ---------- 释放 ----------
    public void Release(int id)
    {
        m_Cache.Remove(id);
        m_Pass?.ReleaseDFProcessingThread(id);
    }

    // ---------- ScriptableRendererFeature ----------
    public override void Create()
    {
        m_Pass = new GenerateDFRendererFeaturePass
        {
            renderPassEvent = settings.passEvent
        };

        // 将缓存中的所有 thread 刷入新建的 Pass
        foreach (var kvp in m_Cache)
            m_Pass.PendingDFProcessingThread(kvp.Key, kvp.Value);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_Pass);
    }
}
