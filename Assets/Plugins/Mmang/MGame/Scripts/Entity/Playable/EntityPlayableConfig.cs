using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Playables;

namespace Mmang.Game
{
    [System.Serializable]
    [SOComponent("Playable Asset")]
    public class EntityPlayableConfig : EntityConfigComponent
    {
        [SerializeField] private SerializableDictionary<string, PlayableAsset> m_Assets = new();
        public IReadOnlyDictionary<string, PlayableAsset> Assets => m_Assets;
    }
}