using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.PixelartRender
{
    public class LightingManager : SingletonMono<LightingManager>
    {
        [SerializeField] private List<LightData2D> m_Data = new();

        public List<LightData2D> Data => m_Data;
    }
}