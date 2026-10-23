namespace MahjongSlide.Withdrawal
{
    public sealed partial class MahjongFramework2Runtime
    {
        public void OnLevelStarted(int level)
        {
            if (Ready && gameManager != null) gameManager.SetHostGameplayLevel(level);
        }
    }
}
