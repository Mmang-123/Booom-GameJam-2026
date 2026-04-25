
using UnityEngine.Rendering;

namespace Mmang.PixelartRender
{
    public static class PShaderTag
    {
        public static ShaderTagId Pixelart => new("Pixelart");
        public static ShaderTagId MForward => new("MForward");
        public static ShaderTagId Preview => new("Preview");
    }
}