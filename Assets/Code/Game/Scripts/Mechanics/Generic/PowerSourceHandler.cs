
using System.Collections.Generic;

namespace Game
{
    public class PowerSourceHandler
    {
        private HashSet<IPowerSource> m_Set = new();
        private int m_ActivePowerCount = 0;

        public bool IsPowered(int requirePowerSourceCount = 1)
        {
            return m_ActivePowerCount >= requirePowerSourceCount;
        }

        public void AddPowerSource(IPowerSource source)
        {
            if (!m_Set.Contains(source))
            {
                m_Set.Add(source);
                if (source.PowerOn)
                    m_ActivePowerCount++;
                source.OnPowerChanged += OnPowerChanged;
            }
        }

        public void RemovePowerSource(IPowerSource source)
        {
            if (m_Set.Contains(source))
            {
                m_Set.Remove(source);
                if (source.PowerOn)
                    m_ActivePowerCount--;
                source.OnPowerChanged -= OnPowerChanged;   
            }
        }

        private void OnPowerChanged(bool isOn)
        {
            if (isOn)
                m_ActivePowerCount++;
            else
                m_ActivePowerCount--;
        }
    }
}