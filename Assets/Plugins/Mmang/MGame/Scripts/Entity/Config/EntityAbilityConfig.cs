using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine;

namespace Mmang.Game
{
    [SOComponent("Ability")]
    public class EntityAbilityConfig : EntityConfigComponent, IEntityInitConfig
    {
        public int InitOrder => ECCInitOrder.Ability;

        [SerializeField] private List<InterfaceObject<IGameplayAbility>> m_Abilities = new();

        public void OnEntityInit(IEntity entity)
        {
            foreach (var abilityObj in m_Abilities)
            {
                if (abilityObj.Value != null)
                {
                    var spec = abilityObj.Value.CreateSpec(entity);
                    entity.AddAbilitySpec(spec);
                }
            }
        }
    }

}