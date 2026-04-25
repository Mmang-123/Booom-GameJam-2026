using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.PixelartRender
{
    [ExecuteAlways]
    public class LightingManager : SingletonMono<LightingManager>
    {
        [SerializeField] private List<LightData2D> m_Data = new();

        //
        public const int MAX_LIGHT_COUNT = 16;
        private LightData2D[] m_DataArray = new LightData2D[MAX_LIGHT_COUNT];
        public int LightCount { get; private set; }

        public ComputeBuffer DataBuffer { get; private set; }

        private void OnEnable()
        {
            DataBuffer = new(MAX_LIGHT_COUNT, LightData2D.SIZE);
        }

        private void OnDisable()
        {
            DataBuffer.Dispose();
            DataBuffer = null;    
        }

        private void Update()
        {
            int count = Mathf.Min(MAX_LIGHT_COUNT, m_Data.Count);
            for (int i = 0; i < count; i++)
            {
                m_DataArray[i] = m_Data[i];
            }
            LightCount = count;
            DataBuffer.SetData(m_DataArray);
        }
    }
}