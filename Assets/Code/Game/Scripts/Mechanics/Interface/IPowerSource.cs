
namespace Game
{
    public interface IPowerSource
    {
        public bool PowerOn { get; }
        public event System.Action<bool> OnPowerChanged;
    }
}