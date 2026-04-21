using UnityEngine;

namespace Mmang.Game
{
    // 用于实现属性的计算与序列化
    [System.Serializable]
    [VariableTypeDefine(CanBeNull = false, RequireFlag = true)]
    public abstract class GameplayAttributeModel
    {
        public abstract float ComputeValue(GameplayAttribute attribute, GameplayAttributeCollection collection);
    
        public abstract GameplayAttribute CreateAttribute();
    }

    [System.Serializable]
    public abstract class GameplayAttributeModel<T> : GameplayAttributeModel where T : GameplayAttribute, new()
    {
        public abstract float ComputeValue(T attribute, GameplayAttributeCollection collection);

        public sealed override float ComputeValue(GameplayAttribute attribute, GameplayAttributeCollection collection)
            => ComputeValue(attribute as T, collection);

        public virtual T CreateTAttribute()
        {
            var instance = GameplayAttribute.Create(this);
            InitAttribute(instance);
            return instance;
        }

        public override GameplayAttribute CreateAttribute()
        {
            return CreateTAttribute();
        }

        public virtual void InitAttribute(T instance) { }
    }

    [System.Serializable]
    public class GameplayValueAttributeModel : GameplayAttributeModel<GameplayValueAttribute>
    {
        public float RawValue;

        public override float ComputeValue(GameplayValueAttribute attribute, GameplayAttributeCollection collection)
        {
            return attribute.RawValue;
        }

        public override void InitAttribute(GameplayValueAttribute instance)
        {
            base.InitAttribute(instance);
            instance.RawValue = RawValue;
        }
    }
}