using UnityEngine;
using Mmang;
using System.Collections.Generic;

namespace Game
{
    [MGlobalConfig]
    [CreateAssetMenu(menuName = "Create Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private List<LevelRoot> m_LevelRootList = new();
        [SerializeField] private List<Passage> m_PassageList = new();

        // Runtime
        [System.NonSerialized] private bool m_Inited = false;               
        [System.NonSerialized] private Dictionary<string, LevelRoot> m_LevelRootMap;
        [System.NonSerialized] private Dictionary<string, Passage> m_PassageMap = new();

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            m_LevelRootMap ??= new();
            m_LevelRootMap.Clear();
            m_PassageMap ??= new();
            m_PassageMap.Clear();

            foreach (var data in m_LevelRootList)
            {
                m_LevelRootMap.Add(data.LevelName, data);
            }

            foreach (var data in m_PassageList)
            {
                m_PassageMap.Add(data.PassageName, data);
            }
        }

        public static LevelRoot GetLevel(string levelName)
        {
            var instance = GlobalConfigAssets.GetConfigInstance<LevelConfig>();
            instance.Init();
            if (instance.m_LevelRootMap.TryGetValue(levelName, out var result))
                return result;
            return null;
        }

        public static Passage GetPassage(string passageName)
        {
            var instance = GlobalConfigAssets.GetConfigInstance<LevelConfig>();
            instance.Init();
            if (instance.m_PassageMap.TryGetValue(passageName, out var result))
                return result;
            return null;
        }
    }
}