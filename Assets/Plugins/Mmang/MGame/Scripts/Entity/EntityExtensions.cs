using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Mmang.Game
{
    public static class EntityExtensions
    {
        #region Playable

        public static PlayableAsset GetPlayableAsset(this IPlayableEntity entity, string assetName)
        {
            var playableConfig = entity.GetEntityConfigComponent<EntityPlayableConfig>();
            if (playableConfig.Assets.TryGetValue(assetName, out var result))
            {
                return result;
            }
            return null;
        }

        public static bool TryGetPlayableAsset(this IPlayableEntity entity, string assetName, out PlayableAsset outAsset)
        {
            var playableConfig = entity.GetEntityConfigComponent<EntityPlayableConfig>();
            return playableConfig.Assets.TryGetValue(assetName, out outAsset);
        }

        public static void StartPlayable(this IPlayableEntity entity, string assetName)
        {
            Debug.Log(entity + " " + entity.GetEntityConfig());
            var playableConfig = entity.GetEntityConfigComponent<EntityPlayableConfig>();
            if (playableConfig.Assets.TryGetValue(assetName, out var playableAsset))
            {
                entity.PlayableDirector.playableAsset = playableAsset;
                entity.PlayableDirector.Play();
            }
        }

        public static void StopPlayable(this IPlayableEntity entity)
        {
            entity.PlayableDirector.Stop();
        }

        #endregion
    

    }
}