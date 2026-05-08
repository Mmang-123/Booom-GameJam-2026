using UnityEngine;
using Mmang;
using System.Collections.Generic;

namespace Game
{
    [System.Serializable]
    public class LevelData
    {
        public string LevelName;
        public LevelRoot Prefab;   
    }


    [MGlobalConfig]
    [CreateAssetMenu(menuName = "Create Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private List<LevelData> m_DataList = new();

        // Runtime
        [System.NonSerialized] private bool m_Inited = false;               
        [System.NonSerialized] private Dictionary<string, LevelData> m_Map;

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            m_Map ??= new();
            m_Map.Clear();

            foreach (var data in m_DataList)
            {
                m_Map.Add(data.LevelName, data);
            }
        }

        public static LevelData GetLevel(string levelName)
        {
            var instance = GlobalConfigAssets.GetConfigInstance<LevelConfig>();
            instance.Init();
            if (instance.m_Map.TryGetValue(levelName, out var result))
                return result;
            return null;
        }
    }
}