using UnityEngine;
using System.Collections.Generic;

namespace Mmang.Game
{
    [MGlobalConfig(configName = "Gameplay Tags")]
    public class GameplayTagsSettings : ScriptableObject
    {
        [SerializeField] private List<SerializableGameplayTagNode> m_Nodes = new();
        
        private static bool s_Inited = false;
        private readonly static GameplayTagTree s_Tree = new();
        public static IGameplayTagTree Tree
        {
            get
            {
                Init();
                return s_Tree;
            }
        }
        
        private static void Init()
        {
            if (s_Inited)
                return;
            s_Inited = true;

            var instance = GlobalConfigAssets.GetConfigInstance<GameplayTagsSettings>();
            //s_Tree.BuildWithRawTags(instance.TagNames);
            s_Tree.BuildWithNodes(instance.m_Nodes);
        }

        public static void Refresh()
        {
            s_Inited = false;
        }

#if UNITY_EDITOR

        public void Editor_AddNewTag(string tag)
        {
            string[] nodes = tag.Split('.');
            int nodeCount = nodes.Length;

            void Update(List<SerializableGameplayTagNode> rawNodes, int index)
            {
                if (index >= nodeCount)
                    return;

                SerializableGameplayTagNode nextNode = null;
                if (rawNodes != null)
                {
                    nextNode = rawNodes.Find(o => o.NodeName == nodes[index]);
                }

                if (nextNode == null)
                {
                    nextNode = new(nodes[index]);   
                    rawNodes.Add(nextNode);
                }

                Update(nextNode.Children, index + 1);
            }

            Update(m_Nodes, 0);
            Refresh();
        }

        public List<SerializableGameplayTagNode> Editor_GetNodes()
        {
            return m_Nodes;
        }
#endif
    }
}