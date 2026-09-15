using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Wallet, confirmation and challenge steps are separately authored Prefab pages.</summary>
public sealed class HarvestRewardsUI : BaseUI
{
    public const string Path = "harvest/HarvestRewardsUI";
    private const string AcknowledgedCompletionKey = "harvest.ui.completed_request.v1";
    private enum Page { Wallet, Confirmation, Progress, Settlement, Completed, History, Rules }

    [SerializeField] private GameObject walletRoot;
    [SerializeField] private GameObject confirmationRoot;
    [SerializeField] private GameObject progressRoot;
    [SerializeField] private GameObject settlementRoot;
    [SerializeField] private GameObject completedRoot;
    [SerializeField] private GameObject historyRoot;
    [SerializeField] private GameObject rulesRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button rulesButton;
    [SerializeField] private Button historyButton;
    [SerializeField] private TextMeshProUGUI availableText;
    [SerializeField] private TextMeshProUGUI frozenText;
    [SerializeField] private TextMeshProUGUI walletHintText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button[] tierButtons;
    [SerializeField] private TextMeshProUGUI[] tierLabels;
    [SerializeField] private GameObject[] tierSelectedMarkers;
    [SerializeField] private Button applyButton;
    [SerializeField] private TextMeshProUGUI applyText;
    [SerializeField] private Button pendingRewardButton;
    [SerializeField] private TextMeshProUGUI pendingRewardText;
    [SerializeField] private TextMeshProUGUI confirmationAmountText;
    [SerializeField] private TextMeshProUGUI confirmationSummaryText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI stageTitleText;
    [SerializeField] private TextMeshProUGUI stageTaskText;
    [SerializeField] private TextMeshProUGUI stageCountText;
    [SerializeField] private TextMeshProUGUI stageTimerText;
    [SerializeField] private TextMeshProUGUI stageHintText;
    [SerializeField] private Slider stageProgressBar;
    [SerializeField] private GameObject[] stageCompleteMarks;
    [SerializeField] private GameObject[] stageCurrentMarks;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI settlementAmountText;
    [SerializeField] private TextMeshProUGUI settlementTimerText;
    [SerializeField] private Button settlementCloseButton;
    [SerializeField] private TextMeshProUGUI completedAmountText;
    [SerializeField] private TextMeshProUGUI completedDetailText;
    [SerializeField] private Button completedContinueButton;
    [SerializeField] private TextMeshProUGUI historyText;
    [SerializeField] private TextMeshProUGUI rulesText;
    [SerializeField, Min(0.25f)] private float refreshIntervalSeconds = 1f;
    [SerializeField, TextArea] private string confirmationSummaryFormat = "{0} will move into your challenge balance.\n\nComplete {1} stages, each with a new task and its own wait, then wait {2} hours. Minimum {3} days in total.\n\nOne request at a time. Each tier can be completed once.";
    [SerializeField, TextArea] private string unavailableMessage = "Rewards are temporarily unavailable. Keep your save data and restart the game. Contact support if this continues.";

    private HarvestRewardService service;
    private Page page;
    private Page rulesReturnPage;
    private Page historyReturnPage;
    private int selectedTier;
    private float nextRefreshTime;
    private bool subscribed;
    private long minimumWaitSeconds;

    public override PAIEAGDLCBJ Layer { get { return PAIEAGDLCBJ.Top; } }

    public static void Open()
    {
        if (MgrUI.Instance != null && !MCCIJBJGMCK.IsLock()) MgrUI.Instance.Open(Path);
    }

    protected override void Init()
    {
        service = HarvestRewardService.Instance;
        closeButton.onClick.AddListener(Close);
        backButton.onClick.AddListener(Back);
        rulesButton.onClick.AddListener(OpenRules);
        historyButton.onClick.AddListener(OpenHistory);
        applyButton.onClick.AddListener(ReviewApplication);
        if (pendingRewardButton != null) pendingRewardButton.onClick.AddListener(CollectPendingReward);
        confirmButton.onClick.AddListener(ConfirmApplication);
        cancelButton.onClick.AddListener(BackToWallet);
        playButton.onClick.AddListener(ReturnToGame);
        settlementCloseButton.onClick.AddListener(Close);
        completedContinueButton.onClick.AddListener(AcknowledgeCompletion);
        for (int i = 0; i < tierButtons.Length; i++)
        {
            int index = i;
            tierButtons[i].onClick.AddListener(() => SelectTier(index));
        }
        if (service.IsAvailable) rulesText.text = CreateRulesText(service.Config, out minimumWaitSeconds);
    }

    protected override void BeforeOpen()
    {
        if (!subscribed)
        {
            service.Changed += Refresh;
            subscribed = true;
        }
        feedbackText.text = "";
        if (service.IsAvailable)
        {
            HarvestRequestState request = service.State.ActiveRequest;
            if (request != null) selectedTier = request.TierIndex;
            else selectedTier = FirstUnfinishedTier(service.Config, service.State);
            ShowPage(request != null ? RequestPage(request) :
                HasUnacknowledgedCompletion(service.State) ? Page.Completed : Page.Wallet);
        }
        else ShowPage(Page.Wallet);
        Refresh();
    }

    protected override void BeforeClose() { Unsubscribe(); }
    private void OnDestroy() { Unsubscribe(); }
    private void Unsubscribe()
    {
        if (subscribed && service != null) service.Changed -= Refresh;
        subscribed = false;
    }

    private void Update()
    {
        if (!IsOpening || Time.unscaledTime < nextRefreshTime) return;
        nextRefreshTime = Time.unscaledTime + refreshIntervalSeconds;
        Refresh();
    }

    private void ShowPage(Page target)
    {
        page = target;
        walletRoot.SetActive(target == Page.Wallet);
        confirmationRoot.SetActive(target == Page.Confirmation);
        progressRoot.SetActive(target == Page.Progress);
        settlementRoot.SetActive(target == Page.Settlement);
        completedRoot.SetActive(target == Page.Completed);
        historyRoot.SetActive(target == Page.History);
        rulesRoot.SetActive(target == Page.Rules);
        backButton.gameObject.SetActive(target != Page.Wallet && target != Page.Completed);
        rulesButton.gameObject.SetActive(target != Page.Rules);
        historyButton.gameObject.SetActive(target != Page.History && target != Page.Confirmation && target != Page.Rules);
    }

    private void Refresh()
    {
        if (!service.IsAvailable)
        {
            ShowUnavailable();
            return;
        }
        HarvestRewardState state = service.State;
        HarvestRequestState request = state.ActiveRequest;
        // Details remain readable; an active progress page follows real stage transitions.
        if (page == Page.Progress || page == Page.Settlement)
            ShowPage(request != null ? RequestPage(request) : state.Settlements.Count > 0 ? Page.Completed : Page.Wallet);
        else if (page == Page.Confirmation && request != null) ShowPage(RequestPage(request));

        availableText.text = FormatAmount(state.AvailableCents);
        frozenText.text = "In challenge: " + FormatAmount(state.FrozenCents);
        for (int i = 0; i < tierButtons.Length; i++)
        {
            bool exists = i < service.Config.ThresholdCents.Length;
            tierButtons[i].gameObject.SetActive(exists);
            if (!exists) continue;
            tierButtons[i].interactable = true;
            tierLabels[i].text = FormatAmount(service.Config.ThresholdCents[i])
                + (state.CompletedTierIndexes.Contains(i) ? "\nCompleted" : "");
            if (i < tierSelectedMarkers.Length) tierSelectedMarkers[i].SetActive(i == selectedTier);
        }
        bool allDone = state.CompletedTierIndexes.Count >= service.Config.ThresholdCents.Length;
        bool selectedCompleted = state.CompletedTierIndexes.Contains(selectedTier);
        long missing = Math.Max(0, service.Config.ThresholdCents[selectedTier] - state.AvailableCents);
        walletHintText.text = request != null ? "Your current challenge is in progress."
            : allDone ? "All withdrawal tiers are complete."
            : selectedCompleted ? "This tier is complete. Select another tier to continue."
            : missing == 0 ? "This tier is ready. Continue to review your request."
            : FormatAmount(missing) + " more to reach this tier.";
        applyButton.interactable = request != null || (!allDone && !selectedCompleted);
        applyText.text = request != null ? "View challenge" : allDone ? "Plan completed"
            : state.CompletedTierIndexes.Contains(selectedTier) ? "Tier completed" : missing > 0 ? "Keep harvesting" : "Continue";
        confirmButton.interactable = CanApply();
        if (pendingRewardButton != null)
        {
            bool pending = service.HasWinReward || service.HasInGameReward;
            pendingRewardButton.gameObject.SetActive(pending);
            pendingRewardButton.interactable = pending && !service.IsRewardedAdPending;
            if (pendingRewardText != null) pendingRewardText.text = service.HasWinReward ? "Collect level reward" : "Collect harvest reward";
        }
        confirmationAmountText.text = FormatAmount(service.Config.ThresholdCents[selectedTier]);
        confirmationSummaryText.text = string.Format(CultureInfo.InvariantCulture, confirmationSummaryFormat,
            FormatAmount(service.Config.ThresholdCents[selectedTier]), service.Config.Stages.Length,
            FormatHours(service.Config.SettlementWaitSeconds), FormatDays(minimumWaitSeconds));
        if (request != null) RefreshRequest(request);
        if (page == Page.Completed || page == Page.History) RefreshHistory(allDone);
    }

    private void RefreshRequest(HarvestRequestState request)
    {
        long remaining = service.GetRemainingSeconds();
        bool settling = request.Status == HarvestRequestState.SettlementWait;
        for (int i = 0; i < stageCompleteMarks.Length; i++)
            stageCompleteMarks[i].SetActive(settling || i < request.StageIndex);
        for (int i = 0; i < stageCurrentMarks.Length; i++)
            stageCurrentMarks[i].SetActive(!settling && i == request.StageIndex);
        settlementAmountText.text = FormatAmount(request.AmountCents);
        settlementTimerText.text = FormatDuration(remaining);
        if (settling) return;
        HarvestStageSettings stage = service.Config.Stages[request.StageIndex];
        stageTitleText.text = "Harvest stage " + (request.StageIndex + 1) + " / " + service.Config.Stages.Length;
        stageTaskText.text = CreateTaskText(stage);
        stageCountText.text = request.StageProgress + " / " + stage.Target;
        stageProgressBar.SetValueWithoutNotify(Mathf.Clamp01((float)request.StageProgress / stage.Target));
        stageTimerText.text = FormatDuration(remaining);
        stageHintText.text = service.State.ClockRollbackDetected ? "Device time moved back. The timer will resume when it catches up."
            : remaining == 0 ? "Time complete. Finish this task to unlock the next stage."
            : request.StageProgress >= stage.Target ? "Task complete. The next stage opens when the timer ends."
            : "Finish the task and waiting time to unlock the next stage.";
    }

    private void RefreshHistory(bool allDone)
    {
        var records = service.State.Settlements;
        if (records.Count == 0)
        {
            historyText.text = "No completed requests yet.";
            completedAmountText.text = "0.00";
            completedDetailText.text = "Complete a harvest challenge to see your result here.";
            return;
        }
        HarvestSettlementRecord latest = records[records.Count - 1];
        completedAmountText.text = FormatAmount(latest.AmountCents);
        completedDetailText.text = allDone ? "All harvest withdrawal tiers are complete. No cash was paid."
            : "Simulated settlement complete. No cash was paid. You can start another tier.";
        var text = new StringBuilder(512);
        for (int i = records.Count - 1; i >= 0; i--)
        {
            HarvestSettlementRecord record = records[i];
            if (text.Length > 0) text.Append("\n\n");
            text.Append(FormatAmount(record.AmountCents)).Append("  -  Completed\n")
                .Append(DateTimeOffset.FromUnixTimeSeconds(record.CompletedUtcSeconds).UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture));
        }
        historyText.text = text.ToString();
    }

    private bool CanApply()
    {
        return service.IsAvailable && selectedTier >= 0 && selectedTier < service.Config.ThresholdCents.Length
            && service.State.ActiveRequest == null && !service.State.CompletedTierIndexes.Contains(selectedTier)
            && service.State.AvailableCents >= service.Config.ThresholdCents[selectedTier];
    }

    private void ShowUnavailable()
    {
        ShowPage(Page.Wallet);
        availableText.text = "--";
        frozenText.text = "In challenge: --";
        walletHintText.text = unavailableMessage;
        feedbackText.text = "";
        applyText.text = "Temporarily unavailable";
        applyButton.interactable = false;
        confirmButton.interactable = false;
        if (pendingRewardButton != null) pendingRewardButton.gameObject.SetActive(false);
        rulesButton.interactable = false;
        historyButton.interactable = false;
        for (int i = 0; i < tierButtons.Length; i++) tierButtons[i].interactable = false;
    }

    private void SelectTier(int index)
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable || index < 0 || index >= service.Config.ThresholdCents.Length) return;
        selectedTier = index;
        feedbackText.text = "";
        Refresh();
    }

    private void ReviewApplication()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable) return;
        if (service.State.ActiveRequest != null) ShowPage(RequestPage(service.State.ActiveRequest));
        else if (CanApply()) ShowPage(Page.Confirmation);
        else { ReturnToGame(); return; }
        Refresh();
    }

    private void ConfirmApplication()
    {
        if (MCCIJBJGMCK.IsLock() || !CanApply()) return;
        if (service.TryApply(selectedTier))
        {
            feedbackText.text = "";
            ShowPage(RequestPage(service.State.ActiveRequest));
        }
        else feedbackText.text = "The request could not start. Please review your balance.";
        Refresh();
    }

    private void CollectPendingReward()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable || service.IsRewardedAdPending) return;
        bool hasWin = service.HasWinReward;
        if (!hasWin && !service.HasInGameReward) { Refresh(); return; }
        MgrUI.Instance.Close(Path, false);
        bool opened = hasWin ? HarvestRewardPopupUI.OpenWin() : HarvestRewardPopupUI.OpenInGame();
        if (!opened) MgrUI.Instance.Open(Path, false);
    }

    private void OpenRules()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable) return;
        rulesReturnPage = page;
        ShowPage(Page.Rules);
    }

    private void OpenHistory()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable) return;
        historyReturnPage = page;
        ShowPage(Page.History);
        Refresh();
    }

    private void Back()
    {
        if (MCCIJBJGMCK.IsLock()) return;
        ShowPage(page == Page.Rules ? rulesReturnPage : page == Page.History ? historyReturnPage : Page.Wallet);
        Refresh();
    }

    private void BackToWallet()
    {
        if (MCCIJBJGMCK.IsLock()) return;
        ShowPage(Page.Wallet);
        Refresh();
    }

    private void AcknowledgeCompletion()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable) return;
        var records = service.State.Settlements;
        if (records.Count > 0)
        {
            PlayerPrefs.SetString(AcknowledgedCompletionKey, records[records.Count - 1].RequestId);
            PlayerPrefs.Save();
        }
        selectedTier = FirstUnfinishedTier(service.Config, service.State);
        BackToWallet();
    }

    private void ReturnToGame()
    {
        if (MCCIJBJGMCK.IsLock()) return;
        Close();
        MgrUI manager = MgrUI.Instance;
        // Home has no live board. Existing gameplay and Win pages stay exactly where they are.
        if (manager != null && manager.GetTopUI() is HomeUI) manager.Open("gameloading/GameLoading", false);
    }

    private void Close()
    {
        if (!MCCIJBJGMCK.IsLock()) MgrUI.Instance.Close(Path, false);
    }

    private static Page RequestPage(HarvestRequestState request)
    {
        return request.Status == HarvestRequestState.SettlementWait ? Page.Settlement : Page.Progress;
    }

    private static bool HasUnacknowledgedCompletion(HarvestRewardState state)
    {
        return state.Settlements.Count > 0 && state.Settlements[state.Settlements.Count - 1].RequestId
            != PlayerPrefs.GetString(AcknowledgedCompletionKey, "");
    }

    public static int FirstUnfinishedTier(HarvestRewardSettings config, HarvestRewardState state)
    {
        for (int i = 0; i < config.ThresholdCents.Length; i++) if (!state.CompletedTierIndexes.Contains(i)) return i;
        return config.ThresholdCents.Length - 1;
    }

    public static string CreateRulesText(HarvestRewardSettings config, out long minimumWaitSeconds)
    {
        var rules = new StringBuilder(650);
        minimumWaitSeconds = config.SettlementWaitSeconds;
        for (int i = 0; i < config.Stages.Length; i++)
        {
            HarvestStageSettings stage = config.Stages[i];
            minimumWaitSeconds += stage.WaitSeconds;
            rules.Append(i + 1).Append(". ").Append(CreateTaskText(stage))
                .Append(" + wait ").Append(FormatHours(stage.WaitSeconds)).Append("h\n");
        }
        rules.Append("Then wait ").Append(FormatHours(config.SettlementWaitSeconds)).Append("h; minimum ")
            .Append(FormatDays(minimumWaitSeconds)).Append(" days.\n\n")
            .Append("Each stage needs BOTH its task and timer. Only new play in the active stage counts.\n\n")
            .Append("The request amount is frozen. One active request; each tier once. Simulation only; no cash.");
        return rules.ToString();
    }

    public static string CreateTaskText(HarvestStageSettings stage)
    {
        return stage.TaskKind == HarvestStageSettings.Wins ? "Win " + stage.Target + " main levels" : "Make " + stage.Target + " fruit triples";
    }
    public static string FormatAmount(long cents) { return (cents / 100m).ToString("N2", CultureInfo.InvariantCulture); }
    public static string FormatHours(long seconds) { return (seconds / 3600m).ToString("0.##", CultureInfo.InvariantCulture); }
    private static string FormatDays(long seconds) { return (seconds / 86400m).ToString("0.##", CultureInfo.InvariantCulture); }
    public static string FormatDuration(long seconds)
    {
        seconds = Math.Max(0, seconds);
        return string.Format(CultureInfo.InvariantCulture, "{0}d {1:00}:{2:00}:{3:00}", seconds / 86400, seconds % 86400 / 3600, seconds % 3600 / 60, seconds % 60);
    }
}
