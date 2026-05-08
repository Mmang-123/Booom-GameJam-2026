
using Mmang.Util;

namespace Game
{
    [System.Serializable]
    public struct FishSaveData
    {
        public bool Exist;

        public void Clear()
        {
            Exist = false;
        }
    }
}