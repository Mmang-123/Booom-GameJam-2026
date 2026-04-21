using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{
    [SOComponent("Attribute"), System.Serializable]
    public class EntityAttributeConfig : EntityConfigComponent, IEntityInitConfig
    {
        public int InitOrder => ECCInitOrder.Attribute;

        [SerializeReference, VariableType] private List<GameplayAttributeModel> m_Attributes = new();
        public IReadOnlyList<GameplayAttributeModel> Attributes => m_Attributes.MAsReadOnly();
    
        public void OnEntityInit(IEntity entity)
        {
            foreach (var model in m_Attributes)
            {
                var instance = model.CreateAttribute();
                entity.GameplayAttributes.AddAttriute(instance);
            }
        }
    }

}