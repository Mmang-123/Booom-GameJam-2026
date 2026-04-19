
namespace Mmang.Game
{
    public interface IEntityInitConfig
    {
        public int InitOrder { get; }
        public void OnEntityInit(IEntity entity);
    }

    /// <summary>
    /// 实体配置组件初始化顺序
    /// </summary>
    public static class ECCInitOrder
    {
        public static readonly int Attribute = -100;
        public static readonly int Default = 0;
        public static readonly int Ability = 100;
    }
}