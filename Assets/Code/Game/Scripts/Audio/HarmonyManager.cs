using Mmang.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class HarmonyManager : SingletonMono<HarmonyManager>
    {
        public float BMP = 120f;
        public float BeatDuration => 60f / BMP;
        public float BarDuration => BeatDuration * 16;
        public bool Active;

        private int m_CurrentNoteIndex;
        public int CurrentNoteIndex => m_CurrentNoteIndex;

        void Update()
        {
            if (!Active) return;

            float time = Time.time;
            int totalnote = Mathf.FloorToInt(time / BeatDuration);

            int totalbar = totalnote / 16;

            m_CurrentNoteIndex = (totalbar % 4) * 4 + (totalnote % 4);
        }
    }
}
