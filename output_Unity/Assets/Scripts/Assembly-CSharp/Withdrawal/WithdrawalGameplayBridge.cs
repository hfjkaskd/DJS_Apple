using System;
using CorePlay;
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
            WithdrawalService.Instance.Init();
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
            if (!initialized || !WithdrawalService.Instance.IsAvailable) return false;
            string accepted = WithdrawalService.Instance.BeginRun(levelData.withdrawalRunId);
            if (accepted != levelData.withdrawalRunId) return false;
            persistedRunId = accepted;
            return true;
        }

        public static bool RecordTriple(LevelData levelData, int levelNumber)
        {
            return CanRecord(levelData) && WithdrawalService.Instance.RecordTriple(
                persistedRunId, levelData.elimGroupCount, levelNumber == 1);
        }

        public static bool RecordWin(LevelData levelData, int wonLevel)
        {
            return CanRecord(levelData) && WithdrawalService.Instance.RecordWin(
                persistedRunId, wonLevel, wonLevel == 1);
        }

        public static void Flush()
        {
            if (initialized && WithdrawalService.Instance.IsAvailable)
                WithdrawalService.Instance.Flush();
        }

        private static bool CanRecord(LevelData levelData)
        {
            return initialized && levelData != null && !string.IsNullOrEmpty(persistedRunId)
                && levelData.withdrawalRunId == persistedRunId && WithdrawalService.Instance.IsAvailable;
        }
    }
}
