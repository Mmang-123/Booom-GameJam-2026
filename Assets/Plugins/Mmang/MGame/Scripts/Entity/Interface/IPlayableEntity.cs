using UnityEngine.Playables;

namespace Mmang.Game
{
    public interface IPlayableEntity : IEntity
    {
        public PlayableDirector PlayableDirector { get; }
    }
}