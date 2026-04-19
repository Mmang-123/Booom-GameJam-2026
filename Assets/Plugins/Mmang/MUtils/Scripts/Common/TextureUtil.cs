using UnityEngine;

namespace Mmang.Util
{
    public static class TextureUtil
    {
        /// <summary>
        /// 填充颜色
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="color"></param>
        public static void Fill(this Texture2D texture2D, Color color)
        {
            if (texture2D.width <= 0 || texture2D.height <= 0)
                return;
            int len = texture2D.width * texture2D.height;
            Color[] colors = new Color[len];
            for (int i = 0; i < len; i++)
                colors[i] = color;
            texture2D.SetPixels(0, 0, texture2D.width, texture2D.height, colors);
        }
    }
}