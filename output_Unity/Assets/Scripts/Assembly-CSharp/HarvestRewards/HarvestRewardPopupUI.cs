using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>A single reward card. It never changes the wallet or request flow.</summary>
public sealed class HarvestRewardPopupUI : BaseUI
{
    public const string Path = "harvest/HarvestRewardPopupUI";
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI claimText;
    [SerializeField] private TextMeshProUGUI adText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button adButton;
    [SerializeField] private Button closeButton;
    [SerializeField, Min(0.1f)] private float refreshIntervalSeconds = 0.25f;
    [SerializeField] private string fruitTitle = "Harvest bonus";
    [SerializeField] private string winTitle = "Level complete";
    [SerializeField] private string welcomeTitle = "Welcome gift";
    [SerializeField] private string welcomeHint = "Added to your harvest balance.";
    [SerializeField] private string rewardHint = "Collect your harvest reward and keep playing.";
    [SerializeField] private string unavailableMessage = "Rewards are paused. Keep your save data and restart the game.";

    private static int requestedKind;
    private static string requestedRewardId;
    private HarvestRewardService service;
    private int kind;
    private string rewardId;
    private string runId;
    private bool subscribed;
    private bool adRequested;
    private float nextRefresh;

    public override PAIEAGDLCBJ Layer { get { return PAIEAGDLCBJ.Top; } }
    public static bool OpenInGame() { return OpenReward(HarvestRewardOffer.InGame); }
    public static bool OpenWin() { return OpenReward(HarvestRewardOffer.Win); }
    public static bool OpenWelcome() { return OpenReward(0); }

    private static bool OpenReward(int rewardKind)
    {
        MgrUI manager = MgrUI.Instance;
        HarvestRewardService rewards = HarvestRewardService.Instance;
        if (manager == null || !rewards.IsAvailable || rewards.IsRewardedAdPending || MCCIJBJGMCK.IsLock()
            || manager.IsOpenning(Path)) return false;
        HarvestRewardOffer offer = rewardKind == HarvestRewardOffer.InGame ? rewards.State.PendingInGameReward
            : rewardKind == HarvestRewardOffer.Win ? rewards.State.PendingWinReward : null;
        if (rewardKind == 0 ? !rewards.State.TutorialGiftClaimed : offer == null) return false;
        requestedKind = rewardKind;
        requestedRewardId = offer != null ? offer.RewardId : null;
        manager.Open(Path);
        return manager.IsOpenning(Path);
    }

    protected override void Init()
    {
        service = HarvestRewardService.Instance;
        claimButton.onClick.AddListener(Claim);
        adButton.onClick.AddListener(WatchAd);
        closeButton.onClick.AddListener(Close);
    }

    protected override void BeforeOpen()
    {
        kind = requestedKind;
        rewardId = requestedRewardId;
        HarvestRewardOffer offer = CurrentOffer();
        runId = offer != null ? offer.RunId : null;
        adRequested = false;
        if (!subscribed)
        {
            service.Changed += Refresh;
            subscribed = true;
        }
        Refresh();
    }

    protected override void AfterOpen() { Refresh(); }
    protected override void BeforeClose() { Unsubscribe(); }
    private void OnDestroy() { Unsubscribe(); }
    private void Unsubscribe()
    {
        if (subscribed && service != null) service.Changed -= Refresh;
        subscribed = false;
    }

    private void Update()
    {
        if (!IsOpening || Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + refreshIntervalSeconds;
        Refresh();
    }

    private HarvestRewardOffer CurrentOffer()
    {
        if (service == null || !service.IsAvailable) return null;
        return kind == HarvestRewardOffer.InGame ? service.State.PendingInGameReward
            : kind == HarvestRewardOffer.Win ? service.State.PendingWinReward : null;
    }

    private bool MatchesOffer(HarvestRewardOffer offer)
    {
        return offer != null && offer.RewardId == rewardId && offer.RunId == runId;
    }

    private void Refresh()
    {
        if (!service.IsAvailable)
        {
            amountText.text = "--";
            balanceText.text = "Balance: --";
            hintText.text = unavailableMessage;
            claimButton.interactable = false;
            adButton.interactable = false;
            closeButton.interactable = true;
            return;
        }
        titleText.text = kind == 0 ? welcomeTitle : kind == HarvestRewardOffer.Win ? winTitle : fruitTitle;
        balanceText.text = "Balance: " + HarvestRewardsUI.FormatAmount(service.State.AvailableCents);
        if (kind == 0)
        {
            amountText.text = "+" + HarvestRewardsUI.FormatAmount(service.Config.TutorialGiftCents);
            hintText.text = welcomeHint;
            claimText.text = "Continue";
            claimButton.interactable = true;
            adButton.gameObject.SetActive(false);
            closeButton.interactable = true;
            return;
        }
        HarvestRewardOffer offer = CurrentOffer();
        if (!MatchesOffer(offer))
        {
            // BeforeOpen runs before BaseUI.IsOpening; close only once the opening has completed.
            if (IsOpening) MgrUI.Instance.Close(Path, false);
            return;
        }
        bool pending = service.IsRewardedAdPending;
        bool canClaim = kind == HarvestRewardOffer.InGame ? service.CanClaimInGameBase : service.CanClaimWinBase;
        amountText.text = "+" + HarvestRewardsUI.FormatAmount(offer.DisplayCents);
        claimText.text = canClaim ? "Collect" : "Ready in a moment";
        claimButton.interactable = canClaim && !pending;
        closeButton.interactable = !pending;
        adButton.gameObject.SetActive(!offer.IsFirstFree);
        adButton.interactable = !pending && service.IsRewardedAvailable(kind);
        adText.text = pending ? "Ad in progress" : adButton.interactable ? "Watch ad for a bonus" : "Ad unavailable";
        hintText.text = pending ? "Finish the ad to collect the bonus."
            : adRequested ? "No ad bonus was received. You can collect the base reward."
            : rewardHint;
    }

    private void Claim()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable || service.IsRewardedAdPending) return;
        if (kind == 0) { Close(); return; }
        if (!MatchesOffer(CurrentOffer())) { Refresh(); return; }
        bool claimed = kind == HarvestRewardOffer.InGame ? service.ClaimInGameBase(runId) : service.ClaimWinBase(runId);
        if (claimed && IsOpening) MgrUI.Instance.Close(Path, false);
        else Refresh();
    }

    private void WatchAd()
    {
        if (MCCIJBJGMCK.IsLock() || !service.IsAvailable || !MatchesOffer(CurrentOffer())) return;
        adRequested = true;
        service.RequestRewardedReward(kind);
        Refresh();
    }

    private void Close()
    {
        if (MCCIJBJGMCK.IsLock() || (service.IsAvailable && service.IsRewardedAdPending)) return;
        // Dismissal keeps the unclaimed offer intact. BeginRun retains its base reward.
        MgrUI.Instance.Close(Path, false);
    }
}
