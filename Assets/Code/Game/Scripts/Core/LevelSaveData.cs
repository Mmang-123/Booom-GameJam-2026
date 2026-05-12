using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface ILevelSavable
    {
        public string GUID { get; }
        public bool DontSave { get; }
        public string SaveJson();
        public void LoadJson(string json);
#if UNITY_EDITOR
        public void Editor_SetGUID(string newGUID);
#endif
    }

    public class LevelSaveData
    {
        private Dictionary<string, string> m_Map = new();

        public void Save(ILevelSavable savable)
        {
            if (savable == null || savable.DontSave)
                return;

            if (!m_Map.ContainsKey(savable.GUID))
                m_Map.Add(savable.GUID, string.Empty);
            
            string json = savable.SaveJson();
            m_Map[savable.GUID] = json;
        }

        public bool Load(ILevelSavable savable)
        {
            if (m_Map.ContainsKey(savable.GUID))
            {
                savable.LoadJson(m_Map[savable.GUID]);
                return true;
            }
            return false;
        }
    }
}