using System;
using CorePlay;
using FruitApkEarning;
using UnityEngine;

namespace FruitWithdrawal
{
    /// <summary>Forwards committed gameplay events to the local withdrawal simulation.</summary>
    public static class WithdrawalGameplayBridge
    {
        private static bool initialized;
        private static string persistedRunId;

        public static void Initialize()
        {
            persistedRunId = null;
            initialized = true;
            ApkEarningService.Instance.Init(JEFOMCDAPGK.Instance.CurMainLevelIndex);
        }

        public static bool BeginRun(LevelData levelData)
        {
            persistedRunId = null;
            if (levelData == null) return false;
            if (string.IsNullOrEmpty(levelData.withdrawalRunId))
            {
                levelData.withdrawalRunId = Guid.NewGuid().ToString("N");
            }
            // Recheck existing IDs too: a previous failed write may only exist in memory.
            if (!JEFOMCDAPGK.Instance.TrySaveToDisk())
            {
                Debug.LogWarning("[Withdrawal] Could not save the gameplay attempt. Rewards are skipped for this attempt.");
                return false;
            }
            if (!initialized || !ApkEarningService.Instance.Available) return false;
            if (!ApkEarningService.Instance.BeginRun(levelData.withdrawalRunId, levelData.elimGroupCount,
                JEFOMCDAPGK.Instance.CurMainLevelIndex)) return false;
            persistedRunId = levelData.withdrawalRunId;
            return true;
        }

        public static bool RecordTriple(LevelData levelData, int levelNumber)
        {
            return CanRecord(levelData) && ApkEarningService.Instance.RecordMatch(
                persistedRunId, levelData.elimGroupCount, levelNumber,
                levelData.totalItemCount > 0 && levelData.eliminatedItemCount >= levelData.totalItemCount);
        }

        public static bool RecordWin(LevelData levelData, int wonLevel)
        {
            return CanRecord(levelData) && ApkEarningService.Instance.RecordWin(persistedRunId, wonLevel);
        }

        public static void Flush()
        {
            if (initialized && ApkEarningService.Instance.Available)
                ApkEarningService.Instance.Flush();
        }

        private static bool CanRecord(LevelData levelData)
        {
            return initialized && levelData != null && !string.IsNullOrEmpty(persistedRunId)
                && levelData.withdrawalRunId == persistedRunId && ApkEarningService.Instance.Available;
        }
    }
}
