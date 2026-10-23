using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WZSDK;

namespace MahjongSlide.Withdrawal
{
    // The visuals, reward arithmetic and stage/wheel/withdrawal transitions belong
    // to the imported WZSDK views. This component only connects their host boundary.
    [DefaultExecutionOrder(-1000)]
    public sealed partial class MahjongFramework2Runtime : MonoBehaviour
    {
        private Framework2ProductConfig settings;
        [SerializeField] private WZSDK.UIManager uiManager;
        [SerializeField] private WZSDK.GameManagerWZ gameManager;
        [SerializeField] private WZSDK.GameSceneWZ gameScene;
        private Action rewardCompleted;
        private bool rewardFlowPending;
        private bool settlementFlowPending;
        private Action queuedSettlementCompleted;
        private bool onboardingFlowPending;
        internal const string LegacyGameplayWithdrawalGuideKey = "MahjongGameplayWithdrawalGuide.v1";
        private const string GameplayGuideRewardClaimedKey = "MahjongGameplayGuideRewardClaimed.v1";
        private bool sourceNextRound;
        private bool activeGameplay;
        private int lifetime;
        private long lastBalance = long.MinValue;
        private int pendingSourceTransitions;
        private long lastLoginCheckDate;
        private readonly Dictionary<ERewardType, Vector3> flightTargets = new Dictionary<ERewardType, Vector3>();
        private MahjongFramework2CurrencyHud currencyHud;
        public bool UsesNativeCurrencyHud => currencyHud != null && currencyHud.isActiveAndEnabled;
        public float GetGameplayTopInset(float canvasHeight) =>
            currencyHud != null ? currencyHud.GetGameplayTopInset(canvasHeight) : 0f;
        public event Action<long> CurrencyChanged;
        public event Action<int> HintGranted;
        public event Action OnboardingCompleted;
        public bool Ready { get; private set; }
        public int LinearLevelNumber => gameManager != null ? Math.Max(1, gameManager.currentLv) : 1;
        public bool LegacyTutorialLevelRemoved => gameManager != null && gameManager.LegacyTutorialLevelRemoved;
        public string LinearLevelTitleFormat => settings.localizationDatabase != null
            ? settings.localizationDatabase.Get("1004", Framework2Host.Currency?.Language ?? "en")
            : settings.linearLevelTitleFormat;
        public long GuideCompletionRewardMinorUnits => settings != null && settings.economy != null ? settings.economy.gameplayGuideRewardMinorUnits : 0;
        public long BalanceMinorUnits
        {
            get
            {
                if (gameManager == null || settings == null || settings.minorUnitsPerCoin <= 0) return 0;
                double minor = Math.Round((double)gameManager.GetCoin() * settings.minorUnitsPerCoin, MidpointRounding.AwayFromZero);
                if (double.IsNaN(minor) || minor <= 0) return 0;
                return minor >= long.MaxValue ? long.MaxValue : (long)minor;
            }
        }
        public bool HasOpenPanels => onboardingFlowPending || rewardFlowPending ||
            Tutorial.IsPresentationPending || (uiManager != null && uiManager.HasHostBlockingViews);
        public bool HasPendingGameplayPresentation => HasOpenPanels || pendingSourceTransitions > 0 ||
            (gameManager != null && (gameManager.HasHostPendingFlow || gameManager.HasHostPendingPairReward));
        public int GameplayPresentationQuietFrames => settings.chainQuietFrames;
        public bool IsForcedGuideActive => onboardingFlowPending || Tutorial.IsPresentationPending ||
            (uiManager != null && uiManager.HasActiveView(EUIType.Tutorial));
        public bool UsesMahjongInGameRewardTrigger => settings != null && settings.useMahjongInGameRewardTrigger;
        internal bool IsHostInGameRewardFlow => rewardFlowPending && !settlementFlowPending;
        internal bool OwnsCompletionFlow => settlementFlowPending || onboardingFlowPending;
        public bool InGameRewardReady => Ready && !HasOpenPanels && gameManager.CanGrantCoinRewardForCurrentLevel() &&
            (UsesMahjongInGameRewardTrigger || gameManager.IsTimedRewardReadyForTesting());
        public bool IsInGameRewardEligible(float elapsedSeconds, int eliminatedPairs, int initialPairs)
        {
            if (!Ready || !gameManager.CanGrantCoinRewardForCurrentLevel()) return false;
            // Native mode is opened by the source post-elimination callback,
            // not the host's once-per-round progress/deadlock reward path.
            if (!UsesMahjongInGameRewardTrigger) return false;
            float progress = Mathf.Clamp01((float)eliminatedPairs / Mathf.Max(1, initialPairs));
            return progress >= settings.inGameRewardForcedProgressRatio ||
                (progress >= settings.inGameRewardProgressRatio && elapsedSeconds >= settings.inGameRewardMinimumRoundSeconds);
        }

        public async Task InitializeAsync(Transform uiParent, Framework2ProductConfig config,
            Action<string, Action<bool>> rewarded, Action<string, Action<bool>> interstitial,
            Func<DateTime> localNow, Func<string> region, Func<bool> gameplayActive,
            RectTransform cashTarget, Button withdrawButton, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Ready) return;
            if (config != null) settings = config;
            if (settings == null || uiManager == null || gameManager == null || gameScene == null)
                throw new InvalidOperationException("Framework 2 runtime prefab/configuration references are missing.");
            if (settings.minorUnitsPerCoin <= 0 || settings.chainQuietFrames < 1)
                throw new InvalidOperationException("Framework 2 currency scale and chain quiet frames must be positive.");
            if (settings.economy == null)
                throw new InvalidOperationException("Framework 2 product configuration requires its inline economy data.");
            if (settings.currencyDatabase == null || settings.currencyDatabase.DefaultProfile == null ||
                settings.localizationDatabase == null)
                throw new InvalidOperationException("Framework 2 product configuration requires its inline region and localization data with a US fallback.");
            settings.economy.ValidateOrThrow();
            lifetime++;
            int version = lifetime;
            Framework2Host.Runtime = this;
            Framework2Host.ApplySettings(settings);
            AdsControl.Instance?.ApplyProductConfiguration();
            Framework2Host.Rewarded = rewarded;
            Framework2Host.Interstitial = interstitial;
            Framework2Host.Clock = localNow;
            Framework2Host.Region = region;
            Framework2Host.GameplayActive = gameplayActive ?? (() => activeGameplay);
            SetHudTargets(cashTarget, withdrawButton);
            gameManager.uiManager = uiManager;
            gameManager.GameScene = gameScene;
            GameApp.Instance.Init();
            await gameManager.InitializeForMahjongAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null || version != lifetime) return;
            await InitializeSourceCurrencyHudAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null || version != lifetime) return;
            BindHostEffects();
            Ready = true;
            lastLoginCheckDate = Framework2Host.LocalNow.Date.Ticks;
            NotifyCurrencyChanged();
        }

        private void Awake()
        {
            settings = Apple.Framework2.AppleF2Runtime.StartupConfiguration;
            Framework2Host.ApplySettings(settings);
            Framework2Host.Runtime = this;
        }

        private void BindHostEffects()
        {
            FacadeGamePlayExtend.GetGamePlayMoneyRectHandle = GetCashTarget;
            FacadeGamePlayExtend.GetGamePlayDiamondRectHandle = GetGemTarget;
            FacadeGamePlayExtend.GetFlyObjGoldHandle = GetFlightTargets;
        }

        private async Task InitializeSourceCurrencyHudAsync()
        {
            BaseView sourceView = await uiManager.ShowViewAsync(EUIType.GamePlayerView);
            currencyHud = sourceView != null ? sourceView.GetComponent<MahjongFramework2CurrencyHud>() : null;
            if (currencyHud == null || !currencyHud.HasRequiredBindings)
                throw new InvalidOperationException("Framework 2 source currency HUD prefab bindings are missing.");
            SetHudTargets(currencyHud.CashTarget, currencyHud.CashButton);
        }

        public void SetGameplayTutorialActive(bool active) { if (currencyHud != null) currencyHud.gameObject.SetActive(!active); }

        private RectTransform GetCashTarget() => Framework2Host.CashTarget;
        private RectTransform GetGemTarget() => currencyHud != null ? currencyHud.GemTarget : Framework2Host.CashTarget;
        private Dictionary<ERewardType, Vector3> GetFlightTargets()
        {
            Vector3 destination = Framework2Host.CashTarget != null ? Framework2Host.CashTarget.position : Vector3.zero;
            flightTargets[ERewardType.Money] = destination;
            RectTransform gemTarget = GetGemTarget();
            flightTargets[ERewardType.Gem] = gemTarget != null ? gemTarget.position : destination;
            flightTargets[ERewardType.AddBottle] = destination;
            flightTargets[ERewardType.Clear] = destination;
            flightTargets[ERewardType.Undo] = destination;
            flightTargets[ERewardType.Prompt] = destination;
            flightTargets[ERewardType.Guid] = destination;
            flightTargets[ERewardType.LuckyWalletMoney] = destination;
            return flightTargets;
        }
        public void SetHudTargets(RectTransform cashTarget, Button withdrawButton)
        {
            Framework2Host.CashTarget = currencyHud != null ? currencyHud.CashTarget : cashTarget;
            Framework2Host.WithdrawButton = currencyHud != null ? currencyHud.CashButton : withdrawButton;
            GetFlightTargets();
            if (Ready) BindHostEffects();
        }

        public void TickGameplay(float deltaTime)
        {
            activeGameplay = deltaTime > 0f;
            if (Ready)
            {
                long date = Framework2Host.LocalNow.Date.Ticks;
                if (date != lastLoginCheckDate)
                {
                    lastLoginCheckDate = date;
                    gameManager.RefreshDailyLoginForTesting();
                }
            }
            // Source STimer is ticked once by its original manager. The host predicate
            // excludes advertisements, background time, guides and Mahjong popups.
        }

        public void OnPairEliminated(Vector2? screenPosition = null)
        {
            if (!Ready) return;
            // Match ArrowManager.OnArrowEscaped, not its unused GM-only spin counter.
            float cash = GameDefines.ifIAA ? GameDefines.IAA_Elimination_Money : GameDefines.Elimination_Money;
            if (cash > 0f && gameManager.CanGrantCoinRewardForCurrentLevel())
            {
                gameManager.AddCoin(cash);
                UIEffectView effect = uiManager.GetView<UIEffectView>(EUIType.UIEffectView);
                if (screenPosition.HasValue && effect != null)
                    effect.PlayHostEliminationMoney(screenPosition.Value, GameDefines.Elimination_FlyMoneyCount, cash);
                else
                    FacadeEffectExtend.PlayFlyMoneyHandle?.Invoke(null, GameDefines.Elimination_FlyMoneyCount, cash, null, ERewardType.Money);
            }
            if (!UsesMahjongInGameRewardTrigger)
                gameManager.OnPourCompleteForLuckyReward();
            gameManager.AddExp(GameDefines.EXP_Single);
        }

        public void OnRoundCompleted(bool won, int round)
        {
            // The source completion view advances source currentLv exactly once.
            // A Mahjong failure does not advance its withdrawal mission level.
            if (Ready && !won && !rewardFlowPending) gameManager.ResetHostRound();
        }

        public bool TryShowInGameReward(Action completed)
        {
            if (!InGameRewardReady) return false;
            rewardFlowPending = true;
            rewardCompleted = completed;
            sourceNextRound = false;
            gameManager.StopLevelTimer();
            StartCoroutine(InGameRewardRoutine(lifetime));
            return true;
        }

        public void ShowInGameReward(Action completed)
        {
            if (!TryShowInGameReward(completed)) completed?.Invoke();
        }

        private IEnumerator InGameRewardRoutine(int version)
        {
            Task<BaseView> show = uiManager.ShowViewAsync(EUIType.RewardView);
            while (!show.IsCompleted) yield return null;
            if (version != lifetime) yield break;
            if (show.IsFaulted || show.IsCanceled || show.Result == null)
                Debug.LogError("Framework 2 source in-game reward could not open.");
            yield return WaitForSourceChain(version, false);
            if (version != lifetime) yield break;
            gameManager.RestartLevelTimer();
            CompleteRewardFlow();
        }

        public void ShowSettlementReward(Action completed)
        {
            if (!Ready) { Debug.LogError("Framework 2 settlement requested before initialization."); return; }
            if (rewardFlowPending || onboardingFlowPending)
            {
                // Never mark an unshown settlement as claimed when another
                // source popup owns the screen. Drain it after that flow closes.
                if (!settlementFlowPending) queuedSettlementCompleted = completed;
                return;
            }
            settlementFlowPending = true;
            rewardFlowPending = true;
            rewardCompleted = completed;
            sourceNextRound = false;
            StartCoroutine(SettlementRoutine(lifetime));
        }

        private IEnumerator SettlementRoutine(int version)
        {
            gameManager.PrepareHostCompletion();
            // Source stage transitions can first open their withdrawal tutorial.
            yield return WaitForSourceChain(version, false);
            if (version != lifetime) yield break;
            if (gameManager.HasPendingHostStageTutorial && !sourceNextRound)
            {
                Debug.LogError("Framework 2 withdrawal tutorial has not finished; its checkpoint was retained for resume.");
                rewardFlowPending = false;
                settlementFlowPending = false;
                rewardCompleted = null;
                yield break;
            }
            gameManager.CompleteHostStageTutorial();
            if (!settings.iaaMode && gameManager.currentLv <= GameDefines.CloseInt)
            {
                CompleteRewardFlow();
                yield break;
            }
            Task<BaseView> show = gameManager.ShowHostCompletionAsync();
            while (!show.IsCompleted) yield return null;
            if (version != lifetime) yield break;
            if (show.IsFaulted || show.IsCanceled || show.Result == null)
            {
                Debug.LogError("Framework 2 source settlement view could not open.");
                sourceNextRound = true;
            }
            yield return WaitForSourceChain(version, true);
            if (version == lifetime) CompleteRewardFlow();
        }

        private IEnumerator WaitForSourceChain(int version, bool needsNextRound)
        {
            int quietFrames = 0;
            while (version == lifetime)
            {
                yield return null;
                UIEffectView effect = uiManager.GetView<UIEffectView>(EUIType.UIEffectView);
                bool busy = pendingSourceTransitions > 0 || Tutorial.IsPresentationPending ||
                    uiManager.HasHostBlockingViews || gameManager.HasHostPendingFlow ||
                    (effect != null && effect.HasPendingRewardEffect()) || (needsNextRound && !sourceNextRound);
                quietFrames = busy ? 0 : quietFrames + 1;
                // The imported completion intentionally queues the wheel on its
                // next frame. Wait through that boundary and asynchronous loads.
                if (quietFrames >= settings.chainQuietFrames) yield break;
            }
        }

        internal void NotifySourceNextRound() { sourceNextRound = true; }
        internal Coroutine RunSourceFlow(IEnumerator routine)
        {
            if (routine == null) return null;
            return StartCoroutine(TrackSourceFlow(routine));
        }
        private IEnumerator TrackSourceFlow(IEnumerator routine)
        {
            pendingSourceTransitions++;
            try { yield return routine; }
            finally { pendingSourceTransitions--; }
        }
        internal void NotifyCurrencyChanged()
        {
            if (!Ready) return;
            long balance = BalanceMinorUnits;
            if (balance == lastBalance) return;
            lastBalance = balance;
            CurrencyChanged?.Invoke(balance);
        }
        internal void GrantImportedHint() { HintGranted?.Invoke(1); }

        private void CompleteRewardFlow()
        {
            if (settlementFlowPending) gameManager.ResetHostRound();
            uiManager.GetView<GamePlayerView>(EUIType.GamePlayerView)?.RefreshStageUI();
            settlementFlowPending = false;
            rewardFlowPending = false;
            Action completed = rewardCompleted;
            rewardCompleted = null;
            NotifyCurrencyChanged();
            completed?.Invoke();
            DrainQueuedSettlement();
        }

        private void DrainQueuedSettlement()
        {
            if (queuedSettlementCompleted == null || rewardFlowPending || onboardingFlowPending) return;
            Action completed = queuedSettlementCompleted;
            queuedSettlementCompleted = null;
            ShowSettlementReward(completed);
        }

        public void OpenWithdrawal()
        {
            if (!Ready || IsForcedGuideActive) return;
            gameScene.withDraw(gameManager.currentLv - 1, gameManager.GetCoin(), 1, ViewType.WithDraw, string.Empty);
        }
        // Gameplay teaching only unlocks entry to formal Level 1. Withdrawal
        // tutorials are created by actual source stage completion, never by
        // synthesizing a completed level when the teaching reward is claimed.
        public void BeginWithdrawalGuide()
        {
            if (!Ready || onboardingFlowPending || rewardFlowPending) return;
            if (!gameManager.HasPendingHostStageTutorial)
            {
                OnboardingCompleted?.Invoke();
                return;
            }
            onboardingFlowPending = true;
            sourceNextRound = false;
            StartCoroutine(ResumeStageTutorialRoutine(lifetime));
        }

        private IEnumerator ResumeStageTutorialRoutine(int version)
        {
            Task<bool> presentation = gameManager.ResumeHostStageTutorialAsync();
            while (!presentation.IsCompleted) yield return null;
            if (version != lifetime) yield break;
            if ((presentation.IsFaulted || presentation.IsCanceled || !presentation.Result) &&
                !sourceNextRound && !uiManager.HasHostBlockingViews)
            {
                onboardingFlowPending = false;
                Debug.LogError("Framework 2 stage tutorial could not resume; its checkpoint was retained.");
                yield break;
            }
            yield return WaitForSourceChain(version, false);
            if (version != lifetime) yield break;
            if (!sourceNextRound)
            {
                onboardingFlowPending = false;
                Debug.LogError("Framework 2 stage tutorial did not finish; its checkpoint was retained.");
                yield break;
            }
            gameManager.CompleteHostStageTutorial();
            gameManager.ResetHostRound();
            onboardingFlowPending = false;
            OnboardingCompleted?.Invoke();
            DrainQueuedSettlement();
        }
        public bool GrantGuideCompletionReward()
        {
            if (!Ready) return false;
            if (WZSDK.PlayerPrefs.GetInt(GameplayGuideRewardClaimedKey) != 0) return true;
            // Original economy has no additional Mahjong teaching gift. A zero
            // amount still completes the host checkpoint, without a money flight.
            if (GuideCompletionRewardMinorUnits > 0 && !AddCurrency(GuideCompletionRewardMinorUnits)) return false;
            WZSDK.PlayerPrefs.SetInt(GameplayGuideRewardClaimedKey, 1);
            WZSDK.PlayerPrefs.Save();
            if (GuideCompletionRewardMinorUnits > 0)
                FacadeEffectExtend.PlayFlyMoneyHandle?.Invoke(null, GameDefines.GRE_FlyMoneyCount,
                    (float)GuideCompletionRewardMinorUnits / settings.minorUnitsPerCoin, null, ERewardType.Money);
            return true;
        }

        public bool AddCurrency(long minor)
        {
            if (!Ready || minor <= 0 || settings.minorUnitsPerCoin <= 0) return false;
            double nextBalance = gameManager.currentCoin + (double)minor / settings.minorUnitsPerCoin;
            if (double.IsNaN(nextBalance) || double.IsInfinity(nextBalance) || nextBalance > float.MaxValue ||
                (double)(float)nextBalance * settings.minorUnitsPerCoin >= long.MaxValue) return false;
            // GM additions intentionally bypass source level 1/2 reward eligibility.
            gameManager.currentCoin = (float)nextBalance;
            gameManager.SaveCoinData();
            gameManager.UpdateCoinUI();
            gameManager.RefreshHostCoinProgress();
            return true;
        }
        public string FormatMoney(long minor) => gameManager != null
            ? gameManager.FormatRegionalValue((double)minor / settings.minorUnitsPerCoin)
            : ((double)minor / settings.minorUnitsPerCoin).ToString("F2", CultureInfo.InvariantCulture);
        public void RefreshRegion()
        {
            if (!Ready) return;
            UserDataManager.Instance.RefreshHostRegion();
            uiManager.RefreshRuntimeGmPresentation();
            NotifyCurrencyChanged();
        }

#if DEVELOPMENT_BUILD || (UNITY_EDITOR && MAHJONG_GM)
        public void GmRefreshBusinessTime()
        {
            if (!Ready) return;
            lastLoginCheckDate = Framework2Host.LocalNow.Date.Ticks;
            gameManager.RefreshDailyLoginForTesting();
            uiManager.RefreshRuntimeGmPresentation();
            WZSDK.PlayerPrefs.Save();
            NotifyCurrencyChanged();
        }
#endif

        public bool TryAutoClaimReward()
        {
            if (!Ready || IsForcedGuideActive) return false;
            RewardView inGame = uiManager.GetView<RewardView>(EUIType.RewardView);
            if (uiManager.HasActiveView(EUIType.RewardView) && inGame != null) return Click(inGame.OnlyBtn);
            ChallangeSuccessView settlement = uiManager.GetView<ChallangeSuccessView>(EUIType.ChallangeSuccessView);
            if (uiManager.HasActiveView(EUIType.ChallangeSuccessView) && settlement != null) return Click(settlement.HostOnlyButton);
            LuckySuccessView lucky = uiManager.GetView<LuckySuccessView>(EUIType.LuckySuccessView);
            if (uiManager.HasActiveView(EUIType.LuckySuccessView) && lucky != null) return Click(lucky.HostOnlyButton);
            LuckySpinView spin = uiManager.GetView<LuckySpinView>(EUIType.LuckySpinView);
            if (uiManager.HasActiveView(EUIType.LuckySpinView) && spin != null) return Click(spin.HostSpinButton);
            BonusWheelView bonus = uiManager.GetView<BonusWheelView>(EUIType.BonusWheelView);
            if (uiManager.HasActiveView(EUIType.BonusWheelView) && bonus != null) return Click(bonus.HostSpinButton);
            BloatView wheelEntry = uiManager.GetView<BloatView>(EUIType.BloatView);
            if (uiManager.HasActiveView(EUIType.BloatView) && wheelEntry != null) return Click(wheelEntry.HostContinueButton);
            return false;
        }
        private static bool Click(Button button)
        {
            if (button == null || !button.gameObject.activeInHierarchy || !button.IsInteractable()) return false;
            button.onClick.Invoke();
            return true;
        }

        public void CloseAll()
        {
            lifetime++;
            StopAllCoroutines();
            pendingSourceTransitions = 0;
            Tutorial.CancelPendingPresentation();
            uiManager?.CloseAllViews();
            rewardFlowPending = false;
            settlementFlowPending = false;
            queuedSettlementCompleted = null;
            rewardCompleted = null;
            onboardingFlowPending = false;
        }
        public void Shutdown()
        {
            if (!Ready && Framework2Host.Runtime != this) return;
            uiManager?.Shutdown();
            CloseAll();
            Ready = false;
            if (gameManager != null)
            {
                gameManager.StopLevelTimer();
                gameManager.StopAllCoroutines();
                gameManager.enabled = false;
            }
            if (Framework2Host.Runtime == this)
            {
                STimerManager.Instance.ClearSTimer();
                Framework2Host.Runtime = null;
                Framework2Host.Rewarded = null;
                Framework2Host.Interstitial = null;
                Framework2Host.GameplayActive = null;
                Framework2Host.CashTarget = null;
                Framework2Host.WithdrawButton = null;
                Framework2Host.Clock = null;
                Framework2Host.Region = null;
                Framework2Host.ApplySettings(null);
                FacadeGamePlayExtend.GetGamePlayMoneyRectHandle = null;
                FacadeGamePlayExtend.GetGamePlayDiamondRectHandle = null;
                FacadeGamePlayExtend.GetFlyObjGoldHandle = null;
            }
            WZSDK.SPlayerPrefs.Save();
            settings = null;
        }
        private void OnDestroy() { Shutdown(); }
    }
}
