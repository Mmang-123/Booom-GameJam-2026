using Mmang.Util;

namespace Mmang.Game
{

    public struct GameplayAttributeIdentifier
    {
        public System.Type ModelType;
        public string AdditionalInfo;

        public GameplayAttributeIdentifier(System.Type type, string additionalInfo = null)
        {
            ModelType = type;
            AdditionalInfo = additionalInfo;
        }

        public GameplayAttributeIdentifier(GameplayAttributeModel model, string additionalInfo = null)
            : this(model.GetType(), additionalInfo) { }
    }

    /// <summary>
    /// 属性基类，使用Model进行定义
    /// </summary>
    public class GameplayAttribute : IReference
    {
        public GameplayAttributeModel Model { get; private set; }
        public virtual GameplayAttributeIdentifier ID => new(Model);
        
        public virtual void Clear()
        {
            Model = null;
        }

        protected virtual void Init(GameplayAttributeModel model)
        {
            Model = model;
        }

        public static T Create<T>(GameplayAttributeModel<T> model) where T : GameplayAttribute, new()
        {
            var instance = ReferencePool.Acquire<T>();
            instance.Init(model);
            return instance;
        }
    }

    public class GameplayValueAttribute : GameplayAttribute
    {
        public float RawValue;
    }
}