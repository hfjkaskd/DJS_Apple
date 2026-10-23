using System;
using CorePlay;
using FruitApkEarning;
using FruitEarningVariants;
using UnityEngine;

namespace FruitWithdrawal
{
    /// <summary>Forwards committed gameplay events to the local withdrawal simulation.</summary>
    public static class WithdrawalGameplayBridge
    {
        private static bool initialized;
        private static string persistedRunId;
        private static int bLastElimGroupCount;
        private static bool bWinRecorded;

        public static void Initialize()
        {
            persistedRunId = null;
            bLastElimGroupCount = 0;
            bWinRecorded = false;
            initialized = true;
            if (!EarningVariantSelection.IsB)
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
            if (!initialized) return false;
            if (EarningVariantSelection.IsB)
            {
                var runtime = EarningVariantController.Instance != null ? EarningVariantController.Instance.Runtime : null;
                if (runtime == null || !runtime.Ready) return false;
                bLastElimGroupCount = levelData.elimGroupCount;
                bWinRecorded = false;
                persistedRunId = levelData.withdrawalRunId;
                runtime.OnBegin(runtime.LinearLevelNumber);
                return true;
            }
            if (!ApkEarningService.Instance.Available) return false;
            if (!ApkEarningService.Instance.BeginRun(levelData.withdrawalRunId, levelData.elimGroupCount,
                JEFOMCDAPGK.Instance.CurMainLevelIndex)) return false;
            persistedRunId = levelData.withdrawalRunId;
            return true;
        }

        public static bool RecordTriple(LevelData levelData, int levelNumber)
        {
            if (EarningVariantSelection.IsB)
            {
                if (!CanRecord(levelData) || bWinRecorded || levelData.elimGroupCount <= bLastElimGroupCount) return false;
                // A restored board starts at its saved count. Only a newly committed
                // Apple triple reports one source elimination; retries cannot grant it twice.
                bLastElimGroupCount = levelData.elimGroupCount;
                EarningVariantController.Instance.Runtime.OnTriple();
                return true;
            }
            return CanRecord(levelData) && ApkEarningService.Instance.RecordMatch(
                persistedRunId, levelData.elimGroupCount, levelNumber,
                levelData.totalItemCount > 0 && levelData.eliminatedItemCount >= levelData.totalItemCount);
        }

        public static bool RecordWin(LevelData levelData, int wonLevel)
        {
            if (EarningVariantSelection.IsB)
            {
                if (!CanRecord(levelData) || bWinRecorded) return false;
                bWinRecorded = true;
                // LDNJKLHLHAH owns source settlement after the elimination animation.
                // Do not credit A or advance Framework2 a second time here.
                return true;
            }
            return CanRecord(levelData) && ApkEarningService.Instance.RecordWin(persistedRunId, wonLevel);
        }

        public static void Flush()
        {
            if (EarningVariantSelection.IsB)
            {
                EarningVariantController.Instance?.Runtime?.Save();
                return;
            }
            if (initialized && ApkEarningService.Instance.Available)
                ApkEarningService.Instance.Flush();
        }

        private static bool CanRecord(LevelData levelData)
        {
            if (!initialized || levelData == null || string.IsNullOrEmpty(persistedRunId)
                || levelData.withdrawalRunId != persistedRunId) return false;
            if (!EarningVariantSelection.IsB) return ApkEarningService.Instance.Available;
            var runtime = EarningVariantController.Instance != null ? EarningVariantController.Instance.Runtime : null;
            return runtime != null && runtime.Ready;
        }
    }
}
