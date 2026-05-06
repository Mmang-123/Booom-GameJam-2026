using UnityEngine;

namespace Game
{
    public interface IChargable
    {
        public PowerSourceHandler PowerSourceHandler { get; }
        public bool IsPowered { get; }

        public void SetChargeComplete(bool init);
    }
}