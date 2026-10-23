using System;

namespace WZSDK
{
    public partial class GameManagerWZ
    {
        public void SetHostGameplayLevel(int level)
        {
            level = Math.Max(1, level);
            if (currentLv == level) return;
            currentLv = level;
            PlayerPrefs.SetInt(FacadePlayerPrefExtend.currentLevel, level);
            PlayerPrefs.Save();
            ResetHostRound();
            OnLevelChanged?.Invoke();
        }
    }
}
