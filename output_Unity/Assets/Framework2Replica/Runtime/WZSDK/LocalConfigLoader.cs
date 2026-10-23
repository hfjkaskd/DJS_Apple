using System;
using System.Threading.Tasks;

namespace WZSDK
{
    // Effective source LocalSettings values are authored in Framework2Economy.
    // JSON remains an imported reference asset, never a second runtime authority.
    public static class LocalConfigLoader
    {
        public static Task LoadAsync()
        {
            var economy = Framework2Host.Settings.economy;
            economy.ValidateOrThrow();
            GameDefines.LevelTimerConfig.Clear();
            foreach (var row in economy.adShow)
                if (row != null && row.level > 0) GameDefines.LevelTimerConfig[row.level] = row.time;
            GameDefines.ApplyConfiguredLuckyRewardLevels(economy.luckyRewardLevels);
            GameDefines.LevelMapConfig.Clear();
            foreach (var row in economy.levelMap)
                if (row != null && row.level > 0 && row.mappedLevel > 0)
                    GameDefines.LevelMapConfig[row.level] = row.mappedLevel;
            return Task.CompletedTask;
        }
    }
}
