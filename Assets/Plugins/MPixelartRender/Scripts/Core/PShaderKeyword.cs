using UnityEngine.Rendering;

namespace Mmang.PixelartRender
{
    public static class PShaderKeyword
    {
        public static GlobalKeyword Pixelart => GlobalKeyword.Create("_PIXELART");
        public static GlobalKeyword InEditor => GlobalKeyword.Create("_IN_EDITOR");
        public static GlobalKeyword DebugLUT => GlobalKeyword.Create("_DEBUG_LUT");

        public static GlobalKeyword Cloud => GlobalKeyword.Create("_CLOUD");
    }
}