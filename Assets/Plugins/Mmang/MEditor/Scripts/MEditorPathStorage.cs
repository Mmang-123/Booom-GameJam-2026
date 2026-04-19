
namespace Mmang
{
    public static class MEditorPathStorage
    {
#if UNITY_EDITOR
        public static readonly string RootAssetPath = "Assets/Plugins/Mmang/MEditor";
        public static readonly string ResourcesFolderPath = RootAssetPath + "/Resources";
        public static readonly string GlobalConfigAssetsPath = ResourcesFolderPath + "/GlobalConfigAssets.asset";

        public static readonly string StyleSheetsFolderPath = RootAssetPath + "/StyleSheets";
        public static readonly string ImageResourceFolderPath = RootAssetPath + "/Editor/ImageResources";
#endif

        public static readonly string GlobalConfigResourcesPath = "GlobalConfigAssets";

#if UNITY_EDITOR

        public static string GetStyleSheetPath(string assetName)
        {
            return StyleSheetsFolderPath + "/" + assetName + ".uss";
        }

        public static string GetImageResourcePath(string assetName, string extension = ".png")
        {
            return ImageResourceFolderPath + "/" + assetName + extension;
        }

#endif
    }
}