using UnityEngine;

namespace Game
{
    public interface IChargable
    {
        public void StartCharge(IPowerSource powerSource);
        public void StopCharge(IPowerSource powerSource);
    }

    public abstract class ChargableMono : MonoBehaviour, IChargable
    {
        public abstract void StartCharge(IPowerSource powerSource);
        public abstract void StopCharge(IPowerSource powerSource);
    }
}