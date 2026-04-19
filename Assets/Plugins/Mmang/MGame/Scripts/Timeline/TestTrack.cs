using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Mmang.Test
{
    [TrackBindingType(typeof(GameObject))]
    [TrackClipType(typeof(HitboxClip))]
    public class TestTrack : TrackAsset
    {

    }
}