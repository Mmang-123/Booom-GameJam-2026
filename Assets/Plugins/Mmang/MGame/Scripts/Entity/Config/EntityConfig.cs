using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{
    [CreateAssetMenu(fileName = nameof(EntityConfig), menuName = "Gameplay/Entity Config")]
    public class EntityConfig : SOComponentContainer
    {
        [SerializeField] private uint m_ID;
        [SerializeField] private string m_EntityName;
        [SerializeField] private GameplayTagContainer m_EntityTags;
        public uint ID => m_ID;
        public string EntityName => m_EntityName;
        public GameplayTagContainer EntityTags => m_EntityTags;

        [SerializeReference] private List<EntityConfigComponent> m_Components = new();
        public IReadOnlyList<EntityConfigComponent> EComponents => m_Components.MAsReadOnly();
        public override IReadOnlyList<SOComponent> SOComponents => m_Components.MAsReadOnly();
        public override string SOComponentsFieldName => nameof(m_Components);
        

        [ContextMenu("Add Test Components")]
        private void AddTestComponents()
        {
            m_Components.Clear();
            m_Components.Add(new EntityAttributeConfig());
            m_Components.Add(new EntityPlayableConfig());
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Editor_RegisterToCollection();   
            }
#endif
        }


#if UNITY_EDITOR
        public void Editor_RegisterToCollection()
        {
            if (m_ID == 0)
                return;
            
            var configCollection = GlobalConfigAssets.GetConfigInstance<EntityConfigCollection>();
            if (configCollection.IsError()
            || configCollection.Contains(this))
                return;
            
            if (!configCollection.ContainsID(m_ID))
            {
                configCollection.Editor_AddConfig(this); 
            }
        }

        public void Editor_OnIDChanged(int oldID)
        {
            var configCollection = GlobalConfigAssets.GetConfigInstance<EntityConfigCollection>();
            if (oldID != 0)
            {
                configCollection.Editor_RemoveConfig(this);
            }

            Editor_RegisterToCollection();
        }
#endif
        
    }
}