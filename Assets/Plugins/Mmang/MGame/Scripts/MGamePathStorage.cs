
namespace Mmang
{
    public static class MGamePathStorage
    {
#if UNITY_EDITOR
        public static readonly string RootAssetPath = "Assets/Plugins/Mmang/MGame";
        public static readonly string StyleSheetsFolderPath = RootAssetPath + "/Editor/StyleSheets";
#endif


#if UNITY_EDITOR

        public static string GetStyleSheetPath(string assetName)
        {
            return StyleSheetsFolderPath + "/" + assetName + ".uss";
        }

#endif
    }
}