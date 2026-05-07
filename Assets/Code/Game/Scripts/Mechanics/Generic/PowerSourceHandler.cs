
using System.Collections.Generic;

namespace Game
{
    public class PowerSourceHandler
    {
        private Dictionary<IPowerSource, int> m_PowerSlotMap = new();
        private Dictionary<int, int> m_SlotPowerCounts = new();
        private int m_ActivePowerCount = 0;

        public bool IsPowered(int requirePowerSourceCount = 1)
        {
            return m_ActivePowerCount >= requirePowerSourceCount;
        }

        public bool IsPowered(List<int> targetSlots)
        {
            foreach (var slot in targetSlots)
            {
                if (!GetSlotActive(slot))
                    return false;
            }
            return true;
        }

        public bool GetSlotActive(int slot)
        {
            if (m_SlotPowerCounts.TryGetValue(slot, out var count)
            && count > 0)
                return true;
            return false;
        }

        public void AddPowerSource(IPowerSource source, int slot = 0)
        {
            if (!m_PowerSlotMap.ContainsKey(source))
            {
                m_PowerSlotMap.Add(source, slot);
                if (source.PowerOn)
                    AddPowerCount(source);
                source.OnPowerChanged += OnPowerChanged;
            }
        }

        public void RemovePowerSource(IPowerSource source)
        {
            if (m_PowerSlotMap.ContainsKey(source))
            {
                if (source.PowerOn)
                    RemovePowerCount(source);
                m_PowerSlotMap.Remove(source);
                source.OnPowerChanged -= OnPowerChanged;   
            }
        }

        private void AddPowerCount(IPowerSource source)
        {
            m_ActivePowerCount++;
            int slot = m_PowerSlotMap[source];
            if (!m_SlotPowerCounts.ContainsKey(slot))
                m_SlotPowerCounts.Add(slot, 0);
            m_SlotPowerCounts[slot]++;
        }

        private void RemovePowerCount(IPowerSource source)
        {
            m_ActivePowerCount--;
            int slot = m_PowerSlotMap[source];
            if (m_SlotPowerCounts.ContainsKey(slot))
            {
                m_SlotPowerCounts[slot]--;
            }
        }

        private void OnPowerChanged(IPowerSource source, bool isOn)
        {
            if (isOn)
                AddPowerCount(source);
            else
                RemovePowerCount(source);
        }
    }
}