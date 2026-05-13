
namespace Game
{
    public class BackToTitleTrigger : PlayerTrigger
    {
        protected override void Trigger(Fish fish)
        {
            base.Trigger(fish);
            GameManager.Instance.Restart(LevelConfig.GetTitleLevelName());
        }
    }
}