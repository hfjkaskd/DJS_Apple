using System.Threading.Tasks;

namespace WZSDK
{
    public partial class GameManagerWZ
    {
        private Task<bool> hostTutorialPresentation;
        private const string HostStageTutorialKey = "MahjongPendingStageTutorial.v1";
        private const string FormalLevelMigrationKey = "MahjongFormalLevelNumbering.v2";
        private const string LegacyTutorialLevelRemovedKey = "MahjongLegacyTutorialLevelRemoved.v2";
        private const string LegacyStageRewardGrantedKey = "MahjongLegacyStageRewardGranted.v2";
        public bool HasPendingHostStageTutorial => PlayerPrefs.GetInt(HostStageTutorialKey) > 0;
        public bool LegacyTutorialLevelRemoved => PlayerPrefs.GetInt(LegacyTutorialLevelRemovedKey) != 0;

        private void SavePendingHostStageTutorial(int completedStage)
        {
            MahjongProductAnalytics.Track("f2_guide_pending_saved", "completed_stage", completedStage, "level", currentLv);
            PlayerPrefs.SetInt(HostStageTutorialKey, completedStage);
            PlayerPrefs.Save();
        }

        public void CompleteHostStageTutorial()
        {
            MahjongProductAnalytics.Track("f2_guide_pending_completed", "completed_stage", PlayerPrefs.GetInt(HostStageTutorialKey), "level", currentLv);
            PlayerPrefs.DeleteKey(HostStageTutorialKey);
            PlayerPrefs.Save();
        }

        public Task<bool> ResumeHostStageTutorialAsync()
        {
            MahjongProductAnalytics.Track("f2_guide_resumed", "completed_stage", PlayerPrefs.GetInt(HostStageTutorialKey), "level", currentLv);
            currentState = GAME_STATE.FINISH;
            StopLevelTimer();
            int completedStage = PlayerPrefs.GetInt(HostStageTutorialKey);
            if (completedStage == Framework2Host.Settings.gameplayGuideWithdrawalStage)
                return BeginHostGameplayWithdrawalGuideAsync();
            hostTutorialPresentation = Tutorial.ShowStepAsync(Tutorial.TYPE.TYPE3, 3);
            return hostTutorialPresentation;
        }

        public Task<bool> BeginHostGameplayWithdrawalGuideAsync()
        {
            int stageId = Framework2Host.Settings.gameplayGuideWithdrawalStage;
            if (!TryGetStageRequirement(stageId, out int type, out int target) || type != 1)
                return Task.FromException<bool>(new System.InvalidOperationException(
                    "Framework 2 gameplay guide requires a configured level-based withdrawal stage."));
            if (currentLv <= target)
                return Task.FromException<bool>(new System.InvalidOperationException(
                    "Framework 2 withdrawal guide cannot complete an unplayed formal level."));
            // Reopen a pending source flow without granting rewards or advancing
            // the stage/level a second time.
            var data = new DataModel
            {
                HistoryId = string.Empty,
                Level = currentLv - 1,
                Num = stageId,
                Money = GetCoin(),
                CloseType = 1
            };
            hostTutorialPresentation = Tutorial.OpenWithdrawTutorialAsync(data);
            return hostTutorialPresentation;
        }
        public UnityEngine.Coroutine StartSourceFlow(System.Collections.IEnumerator routine)
        {
            return Framework2Host.Runtime.RunSourceFlow(routine);
        }
        public async Task InitializeForMahjongAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await LocalConfigLoader.LoadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null) return;
            ApplyWhitePackageMode();
            await uiManager.InitializeAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null) return;
            await UserDataManager.Instance.InitializeAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null) return;
            UserDataManager.Instance.RefreshLuckyRewardConfig();
            MigrateFormalLevelNumbering();
            InitGame();
            await PreloadHostFlowViewsAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null) return;
            await InitializeGameUiAsync(uiManager);
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null) return;
            startupCompleted = true;
        }

        private async Task PreloadHostFlowViewsAsync()
        {
            // Bounded SO list: cache native prefabs through the same runtime
            // loader, without instantiating hidden windows or warming a catalog.
            var tasks = new System.Collections.Generic.List<Task<UnityEngine.GameObject>>();
            foreach (EUIType type in Framework2Host.Settings.preloadViews)
                tasks.Add(uiManager.LoadPrefabAsync(type));
            foreach (ViewType type in Framework2Host.Settings.preloadWithdrawalViews)
                tasks.Add(GameApp.viewManager.PreloadAsync(type));
            UnityEngine.GameObject[] prefabs = await Task.WhenAll(tasks);
            foreach (UnityEngine.GameObject prefab in prefabs)
                if (prefab == null) throw new System.InvalidOperationException("Framework 2 flow prefab preload failed.");
        }

        private void MigrateFormalLevelNumbering()
        {
            if (PlayerPrefs.GetInt(FormalLevelMigrationKey) != 0) return;
            int legacyLevel = PlayerPrefs.GetInt(FacadePlayerPrefExtend.currentLevel, 1);
            bool countedTeaching = PlayerPrefs.GetInt(
                MahjongSlide.Withdrawal.MahjongFramework2Runtime.LegacyGameplayWithdrawalGuideKey) != 0;
            if (countedTeaching && legacyLevel > 1)
            {
                int formalLevel = legacyLevel - 1;
                PlayerPrefs.SetInt(FacadePlayerPrefExtend.currentLevel, formalLevel);
                PlayerPrefs.SetInt(LegacyTutorialLevelRemovedKey, 1);
                int priorStage = PlayerPrefs.GetInt(FacadePlayerPrefExtend.Stage, 1) - 1;
                if (TryGetStageRequirement(priorStage, out int type, out int target) &&
                    type == 1 && formalLevel <= target)
                {
                    // Only undo the task completion caused by the phantom level.
                    // Retain currency, entered receiver data, orders and any reward
                    // already granted; replaying the task must not grant it twice.
                    bool hasEarnedAdProgress =
                        PlayerPrefs.GetInt(FacadePlayerPrefExtend.StageAdWatchOwnerStage) == priorStage + 1 &&
                        PlayerPrefs.GetInt(FacadePlayerPrefExtend.StageAdWatchCount) > 0;
                    if (!hasEarnedAdProgress)
                    {
                        PlayerPrefs.SetInt(FacadePlayerPrefExtend.Stage, priorStage);
                        PlayerPrefs.SetInt(LegacyStageRewardGrantedKey, priorStage);
                    }
                    if (PlayerPrefs.GetInt(HostStageTutorialKey) == priorStage)
                        PlayerPrefs.DeleteKey(HostStageTutorialKey);
                }
            }
            PlayerPrefs.SetInt(FormalLevelMigrationKey, 1);
            PlayerPrefs.Save();
        }

        private bool ConsumeLegacyStageReward(int stage)
        {
            if (PlayerPrefs.GetInt(LegacyStageRewardGrantedKey) != stage) return false;
            PlayerPrefs.DeleteKey(LegacyStageRewardGrantedKey);
            PlayerPrefs.Save();
            return true;
        }

        public void ResetHostRound()
        {
            isLevelCompletionFlowRunning = false;
            currentBottleFull = 0;
            completedAnimationBottles = 0;
            currentState = GAME_STATE.PLAYING;
            ResetLevelTimerState();
            ResetPouringTimerState();
            StartLevelTimer();
        }

        internal void RefreshHostCoinProgress()
        {
            OnCoinChanged?.Invoke();
            Framework2Host.CurrencyChanged();
        }

        public void PrepareHostCompletion()
        {
            if (isLevelCompletionFlowRunning) return;
            isLevelCompletionFlowRunning = true;
            currentState = GAME_STATE.FINISH;
            StopLevelTimer();
            PrepareLevelCompletion();
        }

        public Task<BaseView> ShowHostCompletionAsync()
        {
            int completedLevel = GetLastCompletedLevel();
            return uiManager.ShowViewAsync(GetSuccessViewTypeForCompletedLevel(completedLevel), completedLevel);
        }

        public bool HasHostPendingFlow => isProcessingUIQueue || isProcessingStage4SlotFlow ||
            (hostTutorialPresentation != null && !hostTutorialPresentation.IsCompleted);
        private int hostPendingPairRewards;
        public bool HasHostPendingPairReward => hostPendingPairRewards > 0;
    }
}
