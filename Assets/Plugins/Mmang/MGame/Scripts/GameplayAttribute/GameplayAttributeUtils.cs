
namespace Mmang.Game
{
    public static class GameplayAttributeUtils
    {
        public static float GetValue(this IGameplayAttributeOwner owner, GameplayAttribute attribute)
        {
            return owner.GameplayAttributes.GetValue(attribute);
        }

        public static float GetValue(this IGameplayAttributeOwner owner, GameplayAttributeIdentifier id)
        {
            return owner.GameplayAttributes.GetValue(id);
        }

        public static float GetValue<T>(this IGameplayAttributeOwner owner, string additionalInfo = null) where T : GameplayAttributeModel
        {
            return owner.GameplayAttributes.GetValue<T>(additionalInfo);
        }
    }
}