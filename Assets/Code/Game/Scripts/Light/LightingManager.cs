using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

using Light = Mmang.PixelartRender.MLight;
using PointLight = Mmang.PixelartRender.MPointLight;
using SpotLight = Mmang.PixelartRender.MSpotPointLight;

namespace Mmang.PixelartRender
{
    [ExecuteAlways]
    public class LightingManager : SingletonMono<LightingManager>
    {
        //
        public const int MAX_LIGHT_COUNT = 16;

        private List<Light> m_Lights = new();

        private LightData2D[] m_DataArray = new LightData2D[MAX_LIGHT_COUNT];
        public int LightCount { get; private set; }
        public int PointLightCount { get; private set; }
        public int SpotLightCount { get; private set; }

        public ComputeBuffer DataBuffer { get; private set; }

        private List<PointLight> m_PointLightsCache = new();
        private List<SpotLight> m_SpotLightsCache = new();

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
            PointLightCount = 0;
            LightCount = 0;
            SpotLightCount = 0;

            m_PointLightsCache.Clear();
            m_SpotLightsCache.Clear();

            foreach (var light in m_Lights)
            {
                if (LightCount >= MAX_LIGHT_COUNT)
                {
                    break;
                }
                if (light is PointLight pointLight)
                {
                    /*
                    LightData2D data = new()
                    {
                        color = new Vector4(pointLight.Color.r, pointLight.Color.g, pointLight.Color.b, pointLight.Intensity),
                        position = new Vector4(pointLight.Position.x, pointLight.Position.y, pointLight.Position.z, pointLight.Radius)
                    };

                    m_DataArray[LightCount] = data;
                    */

                    m_PointLightsCache.Add(pointLight);

                    PointLightCount++;
                    LightCount++;
                }
                else if (light is SpotLight spotLight)
                {
                    m_SpotLightsCache.Add(spotLight);

                    SpotLightCount++;
                    LightCount++;
                }
            }
            
            // PointLight
            for (int i = 0; i < PointLightCount; i++)
            {
                var pointLight = m_PointLightsCache[i];
                LightData2D data = new()
                {
                    color = new Vector4(pointLight.Color.r, pointLight.Color.g, pointLight.Color.b, pointLight.Intensity),
                    position = new Vector4(pointLight.Position.x, pointLight.Position.y, pointLight.Position.z, pointLight.Radius)
                };

                m_DataArray[i] = data;
            }

            // SpotLight
            for (int i = PointLightCount; i < LightCount; i++)
            {
                var spotLight = m_SpotLightsCache[i - PointLightCount];
                Vector2 direction = spotLight.GetDirection();
                Vector2 scaleOffset = spotLight.GetScaleOffset();
                LightData2D data = new()
                {
                    color = new Vector4(spotLight.Color.r, spotLight.Color.g, spotLight.Color.b, spotLight.Intensity),
                    position = new Vector4(spotLight.Position.x, spotLight.Position.y, spotLight.Position.z, spotLight.Radius),
                    spotLightParams = new(direction.x, direction.y, scaleOffset.x, scaleOffset.y),
                };

                m_DataArray[i] = data;
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