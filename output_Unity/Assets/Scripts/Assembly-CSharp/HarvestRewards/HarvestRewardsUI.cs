using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Prefab-authored simulation ledger, reward claims and disclosed challenge rules.</summary>
public sealed class HarvestRewardsUI : BaseUI
{
    public const string Path = "harvest/HarvestRewardsUI";

    [SerializeField] private TextMeshProUGUI availableText;
    [SerializeField] private TextMeshProUGUI frozenText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI rulesText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button inGameRewardButton;
    [SerializeField] private TextMeshProUGUI inGameRewardText;
    [SerializeField] private Button winRewardButton;
    [SerializeField] private TextMeshProUGUI winRewardText;
    [SerializeField] private Button inGameAdButton;
    [SerializeField] private TextMeshProUGUI inGameAdText;
    [SerializeField] private Button winAdButton;
    [SerializeField] private TextMeshProUGUI winAdText;
    [SerializeField] private Button applyButton;
    [SerializeField] private TextMeshProUGUI applyText;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject confirmationRoot;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button[] tierButtons;
    [SerializeField] private TextMeshProUGUI[] tierLabels;
    [SerializeField, Min(0.25f)] private float refreshIntervalSeconds = 1f;

    private HarvestRewardService service;
    private int selectedTier;
    private float nextRefreshTime;
    private bool subscribed;
    private long minimumWaitSeconds;
    private string inGameOfferId;
    private string winOfferId;

    public override PAIEAGDLCBJ Layer { get { return PAIEAGDLCBJ.Top; } }

    public static void Open()
    {
        if (MgrUI.Instance != null && !MCCIJBJGMCK.IsLock())
            MgrUI.Instance.Open(Path);
    }

    protected override void Init()
    {
        service = HarvestRewardService.Instance;
        closeButton.onClick.AddListener(Close);
        inGameRewardButton.onClick.AddListener(ClaimInGame);
        winRewardButton.onClick.AddListener(ClaimWin);
        inGameAdButton.onClick.AddListener(WatchFruitAd);
        winAdButton.onClick.AddListener(WatchWinAd);
        applyButton.onClick.AddListener(ReviewApplication);
        confirmButton.onClick.AddListener(ConfirmApplication);
        cancelButton.onClick.AddListener(CancelApplication);
        for (int i = 0; i < tierButtons.Length; i++)
        {
            int tierIndex = i;
            tierButtons[i].onClick.AddListener(() => SelectTier(tierIndex));
        }
        if (service.IsAvailable) BuildRules();
    }

    protected override void BeforeOpen()
    {
        if (!subscribed)
        {
            service.Changed += Refresh;
            subscribed = true;
        }
        confirmationRoot.SetActive(false);
        if (service.IsAvailable)
        {
            if (service.State.ActiveRequest != null)
                selectedTier = service.State.ActiveRequest.TierIndex;
            selectedTier = Mathf.Clamp(selectedTier, 0, service.Config.ThresholdCents.Length - 1);
        }
        feedbackText.text = "No payment details are collected. No real payout is available.";
        Refresh();
    }

    protected override void BeforeClose()
    {
        Unsubscribe();
        confirmationRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (subscribed && service != null)
        {
            service.Changed -= Refresh;
            subscribed = false;
        }
    }

    private void Update()
    {
        if (!IsOpening || Time.unscaledTime < nextRefreshTime)
            return;
        nextRefreshTime = Time.unscaledTime + refreshIntervalSeconds;
        Refresh();
    }

    private void BuildRules()
    {
        rulesText.text = CreateRulesText(service.Config, out minimumWaitSeconds);
    }

    public static string CreateRulesText(HarvestRewardSettings config, out long minimumWaitSeconds)
    {
        var rules = new StringBuilder(650);
        minimumWaitSeconds = config.SettlementWaitSeconds;
        for (int i = 0; i < config.Stages.Length; i++)
        {
            HarvestStageSettings stage = config.Stages[i];
            minimumWaitSeconds += stage.WaitSeconds;
            rules.Append(i + 1).Append(". ").Append(TaskDescription(stage))
                .Append(" + wait ").Append(Hours(stage.WaitSeconds)).Append("h\n");
        }
        rules.Append("Then wait ").Append(Hours(config.SettlementWaitSeconds)).Append("h; minimum ")
            .Append(Days(minimumWaitSeconds)).Append(" days.\n")
            .Append("Each stage needs BOTH its task and timer. Only new play in the active stage counts.\n")
            .Append("The request amount is frozen. One active request; each tier once. Simulation only; no cash.");
        return rules.ToString();
    }

    private void Refresh()
    {
        if (!service.IsAvailable)
        {
            ShowUnavailable();
            return;
        }
        HarvestRewardState state = service.State;
        availableText.text = Amount(state.AvailableCents);
        frozenText.text = Amount(state.FrozenCents);
        inGameOfferId = state.PendingInGameReward != null ? state.PendingInGameReward.RunId : null;
        winOfferId = state.PendingWinReward != null ? state.PendingWinReward.RunId : null;
        inGameRewardButton.interactable = service.CanClaimInGameBase;
        winRewardButton.interactable = service.CanClaimWinBase;
        inGameRewardText.text = RewardLabel("Fruit reward", service.HasInGameReward,
            service.CanClaimInGameBase, service.GetInGameRewardPreviewCents());
        winRewardText.text = RewardLabel("Level reward", service.HasWinReward,
            service.CanClaimWinBase, service.GetWinRewardPreviewCents());
        RefreshAdButton(inGameAdButton, inGameAdText, state.PendingInGameReward, HarvestRewardOffer.InGame);
        RefreshAdButton(winAdButton, winAdText, state.PendingWinReward, HarvestRewardOffer.Win);
        for (int i = 0; i < tierButtons.Length; i++)
        {
            bool exists = i < service.Config.ThresholdCents.Length;
            tierButtons[i].gameObject.SetActive(exists);
            if (!exists) continue;
            bool completed = state.CompletedTierIndexes.Contains(i);
            string tierState = completed ? (i == selectedTier ? "DONE / SELECTED" : "COMPLETED")
                : i == selectedTier ? "SELECTED" : "";
            tierLabels[i].text = Amount(service.Config.ThresholdCents[i])
                + (tierState.Length > 0 ? "\n" + tierState : "");
        }

        bool eligible = CanApply();
        applyButton.interactable = eligible;
        confirmButton.interactable = eligible;
        applyText.text = state.ActiveRequest != null ? "Challenge in progress" :
            state.CompletedTierIndexes.Contains(selectedTier) ? "This tier is completed" :
            eligible ? "Review simulation request" : "Keep playing to reach this tier";
        progressText.text = BuildProgressText();
    }

    private void ShowUnavailable()
    {
        inGameRewardButton.interactable = false;
        winRewardButton.interactable = false;
        inGameAdButton.interactable = false;
        winAdButton.interactable = false;
        applyButton.interactable = false;
        confirmButton.interactable = false;
        for (int i = 0; i < tierButtons.Length; i++) tierButtons[i].interactable = false;
        confirmationRoot.SetActive(false);
        availableText.text = "--";
        frozenText.text = "--";
        applyText.text = "Rewards temporarily unavailable";
        progressText.text = "REWARDS PAUSED\nYour saved rewards could not be loaded or saved safely.\nKeep your save data and restart the game. Contact support if this continues.";
        feedbackText.text = "Reward operations are paused. Do not delete your save data.";
    }

    private void RefreshAdButton(Button button, TextMeshProUGUI label, HarvestRewardOffer offer, int kind)
    {
        bool ready = offer != null && !offer.IsFirstFree && service.IsRewardedAvailable(kind);
        button.interactable = ready;
        label.text = service.IsRewardedAdPending ? "Ad in progress" :
            ready ? "Watch ad - bonus reward" : "Watch ad - unavailable";
    }

    private string BuildProgressText()
    {
        HarvestRewardState state = service.State;
        HarvestRequestState request = state.ActiveRequest;
        if (request == null)
        {
            if (state.CompletedTierIndexes.Count >= service.Config.ThresholdCents.Length)
                return "HARVEST PLAN COMPLETED\nAll simulated withdrawal tiers are complete.\nNo cash was paid. You can keep playing the fruit game.";
            if (state.Settlements.Count > 0)
            {
                HarvestSettlementRecord last = state.Settlements[state.Settlements.Count - 1];
                return "SIMULATION COMPLETED: " + Amount(last.AmountCents) + " units\n"
                    + state.Settlements.Count + " challenge(s) completed. No cash was paid.\n"
                    + "Select another tier to start a new harvest challenge.";
            }
            long missing = Math.Max(0, service.Config.ThresholdCents[selectedTier] - state.AvailableCents);
            return "YOUR NEXT HARVEST CHALLENGE\n"
                + (missing == 0 ? "This tier is ready to apply for." : Amount(missing) + " more simulated units to reach this tier.")
                + "\nAll six stages and waiting times begin after applying.";
        }

        long remaining = service.GetRemainingSeconds();
        string clockNote = state.ClockRollbackDetected ? "\nDevice clock moved back; time progress is paused." : "";
        if (request.Status == HarvestRequestState.SettlementWait)
            return "FINAL SIMULATION WAIT\n" + Amount(request.AmountCents)
                + " units in challenge. All six stages completed.\n"
                + "Time left: " + Duration(remaining) + ". No cash payout follows." + clockNote;
        HarvestStageSettings stage = service.Config.Stages[request.StageIndex];
        return "STAGE " + (request.StageIndex + 1) + " / " + service.Config.Stages.Length
            + " - " + Amount(request.AmountCents) + " simulated units\n"
            + TaskDescription(stage) + ": " + request.StageProgress + " / " + stage.Target
            + "\nTime left: " + Duration(remaining)
            + (remaining == 0 && request.StageProgress < stage.Target ? " - finish the task to continue." : " - task and timer must both finish.")
            + clockNote;
    }

    private bool CanApply()
    {
        return service.IsAvailable && service.State.ActiveRequest == null
            && !service.State.CompletedTierIndexes.Contains(selectedTier)
            && service.State.AvailableCents >= service.Config.ThresholdCents[selectedTier];
    }

    private void SelectTier(int tierIndex)
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable || tierIndex >= service.Config.ThresholdCents.Length) return;
        selectedTier = tierIndex;
        confirmationRoot.SetActive(false);
        Refresh();
    }

    private void ReviewApplication()
    {
        if (MCCIJBJGMCK.IsLock() || !CanApply()) return;
        confirmationText.text = "Apply for " + Amount(service.Config.ThresholdCents[selectedTier]) + " simulated units.\n\n"
            + "This amount moves from available to in-challenge balance.\n\n"
            + "All six stages and the final " + Hours(service.Config.SettlementWaitSeconds)
            + "h wait still apply. Minimum " + Days(minimumWaitSeconds) + " days, longer if tasks are unfinished.\n\n"
            + "There is no real payment or cash withdrawal.";
        confirmationRoot.SetActive(true);
    }

    private void ConfirmApplication()
    {
        if (MCCIJBJGMCK.IsLock()) return;
        bool success = service.TryApply(selectedTier);
        confirmationRoot.SetActive(false);
        feedbackText.text = success ? "Simulation started. Only new progress in the current stage counts."
            : "This request could not be started. Review your balance and active challenge.";
        Refresh();
    }

    private void CancelApplication()
    {
        if (!MCCIJBJGMCK.IsLock()) confirmationRoot.SetActive(false);
    }

    private void ClaimInGame()
    {
        if (MCCIJBJGMCK.IsLock()) return;
        bool claimed = service.ClaimInGameBase(inGameOfferId);
        feedbackText.text = claimed ? "Fruit reward added to your simulated balance." : "No fruit reward is ready to claim yet.";
        Refresh();
    }

    private void ClaimWin()
    {
        if (MCCIJBJGMCK.IsLock()) return;
        bool claimed = service.ClaimWinBase(winOfferId);
        feedbackText.text = claimed ? "Level reward added to your simulated balance." : "No level reward is ready to claim yet.";
        Refresh();
    }

    private void WatchFruitAd()
    {
        RequestRewardedAd(HarvestRewardOffer.InGame);
    }

    private void WatchWinAd()
    {
        RequestRewardedAd(HarvestRewardOffer.Win);
    }

    private void RequestRewardedAd(int kind)
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable) return;
        bool started = service.RequestRewardedReward(kind);
        feedbackText.text = started ? "Complete the ad to receive the simulated bonus."
            : "No ad is available. You can still claim the base reward.";
        Refresh();
    }

    private void Close()
    {
        if (!MCCIJBJGMCK.IsLock()) MgrUI.Instance.Close(Path, false);
    }

    private static string TaskDescription(HarvestStageSettings stage)
    {
        return stage.TaskKind == HarvestStageSettings.Wins
            ? "Win " + stage.Target + " main levels"
            : "Make " + stage.Target + " fruit triples";
    }

    private static string RewardLabel(string title, bool offered, bool claimable, long cents)
    {
        return title + "\n" + (!offered ? "Nothing to claim" : claimable ? "Claim " + Amount(cents) : "Ready in a moment");
    }

    private static string Amount(long cents)
    {
        return (cents / 100m).ToString("N2", CultureInfo.InvariantCulture);
    }

    private static string Hours(long seconds)
    {
        return (seconds / 3600m).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string Days(long seconds)
    {
        return (seconds / 86400m).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string Duration(long seconds)
    {
        seconds = Math.Max(0, seconds);
        return string.Format(CultureInfo.InvariantCulture, "{0}d {1:00}:{2:00}:{3:00}",
            seconds / 86400, seconds % 86400 / 3600, seconds % 3600 / 60, seconds % 60);
    }
}
