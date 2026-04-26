using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

using Light = Mmang.PixelartRender.MLight;
using PointLight = Mmang.PixelartRender.MPointLight;

namespace Mmang.PixelartRender
{
    [ExecuteAlways]
    public class LightingManager : SingletonMono<LightingManager>
    {
        [SerializeField] private List<LightData2D> m_Data = new();

        //
        public const int MAX_LIGHT_COUNT = 16;

        private List<Light> m_Lights = new();


        private LightData2D[] m_DataArray = new LightData2D[MAX_LIGHT_COUNT];
        public int LightCount { get; private set; }

        public ComputeBuffer DataBuffer { get; private set; }

        private void OnEnable()
        {
            DataBuffer = new(MAX_LIGHT_COUNT, LightData2D.SIZE);
        }

        private void OnDisable()
        {
            if (DataBuffer != null)
            {
                DataBuffer.Dispose();
                DataBuffer = null;    
            }   
        }

        private void Update()
        {
            // TODO: 剔除后面再说
            int pointLightCount = 0;
            LightCount = 0;
            foreach (var light in m_Lights)
            {
                if (LightCount >= MAX_LIGHT_COUNT)
                {
                    break;
                }
                if (light is PointLight pointLight)
                {
                    LightData2D data = new();
                    data.color = new Vector4(pointLight.Color.r, pointLight.Color.g, pointLight.Color.b, pointLight.Intensity);
                    data.position = new Vector4(pointLight.Position.x, pointLight.Position.y, pointLight.Position.z, pointLight.Radius);

                    m_DataArray[LightCount] = data;

                    pointLightCount++;
                    LightCount++;
                }
            }
            
            DataBuffer.SetData(m_DataArray);
        }

        #region 

        public void RegisterLight(Light light)
        {
            m_Lights.Add(light);
        }

        public void UnregisterLight(Light light)
        {
            m_Lights.Remove(light);
        }



        #endregion
    }
}