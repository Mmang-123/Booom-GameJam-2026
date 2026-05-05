using Mmang.PixelartRender;
using Mmang.Util;
using UnityEngine;

namespace Game
{
    public class DarkSightManager : SingletonMono<DarkSightManager>
    {
        [SerializeField] private Vector2 m_CenterPosition;
        [SerializeField] private float m_Radius;
    
        public (Vector2 uv, Vector2 radiusRatio) GetParams()
        {
            var camera = Camera.main;
            var pixelartCamera = camera.GetComponent<PixelartCamera>();
            Vector2 resolution = pixelartCamera.CameraData.SourceResolution;
            float orthoSize = camera.orthographicSize;
            Vector2 cameraPos = camera.transform.position;

            float aspect = resolution.x / resolution.y;
            float worldHeight = orthoSize * 2f;
            float worldWidth = worldHeight * aspect;

            // 2. 计算相对相机的偏移
            Vector2 delta = m_CenterPosition - cameraPos;

            // 3. 映射到 0~1 的 UV 空间
            Vector2 uv = new Vector2(
                (delta.x / worldWidth) + 0.5f,
                (delta.y / worldHeight) + 0.5f
            );

            // 4. 计算半径的屏幕长宽占比
            Vector2 radiusRatio = new Vector2(
                m_Radius / worldWidth,   // 占屏幕宽度的比例
                m_Radius / worldHeight   // 占屏幕高度的比例
            );

            return new(uv, radiusRatio);
        }
    }
}