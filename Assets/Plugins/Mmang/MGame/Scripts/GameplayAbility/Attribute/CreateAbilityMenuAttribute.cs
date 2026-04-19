using System;

namespace Mmang.Game
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CreateAbilityMenuAttribute : TypeCollectionAttribute
    {
        public override Type BaseType => typeof(GameplayAbility);

        public string AssetName;
        public string Path;
        public int Order;

        public CreateAbilityMenuAttribute(string assetName, string path = null, int order = 0)
        {
            AssetName = assetName;
            Path = path ?? string.Empty;
            Order = order;
        }
    }
}