using UnityEngine;
using UnityEngine.UI;
using WZSDK;

/// <summary>
/// Updates only the additional labels in the approved Apple skin. All task,
/// reward, payment-simulation and transition decisions remain source-owned.
/// References are authored into prefabs; no hierarchy construction or searches.
/// </summary>
[DisallowMultipleComponent]
public sealed class Framework2AppleRewardPresenter : MonoBehaviour
{
    public enum Page
    {
        Other = 0, AdMission = 1, Feedback = 2, StageProgress = 3,
        Slot = 4, BonusIntro = 5, Compensation = 6,
        FinalProgress = 7, BonusWheel = 8
    }

    [SerializeField] private Page page;
    [SerializeField] private Text rewardAmount;
    [SerializeField] private Text currentBalance;
    [SerializeField] private Text targetAmount;
    [SerializeField] private Text remainingAmount;
    [SerializeField] private Text progressPercent;
    [SerializeField] private Text missionCount;
    [SerializeField] private Image missionFill;
    [SerializeField] private ProgressView stageProgress;
    [SerializeField] private SlotView slot;
    [SerializeField] private BloatProgressView finalProgress;
    [SerializeField] private BonusWheelView bonusWheel;
    [SerializeField] private WithdrawFeedbackView feedback;
    [SerializeField] private Sprite clearIcon, undoIcon;

    public void ConfigurePropIcons(Sprite clear, Sprite undo) { clearIcon = clear; undoIcon = undo; }

    private float nextRefresh;
    private bool dirty = true;
    private double lastAmount = double.NaN;
    private double lastBalance = double.NaN;
    private double lastTarget = double.NaN;
    private double lastRemaining = double.NaN;
    private int lastPercent = -1;
    private int lastMissionCurrent = -1;
    private int lastMissionTarget = -1;
    private GameManagerWZ lastManager;

    // Called by the Editor prefab authoring pass, not by runtime initialization.
    public void Configure(Page configuredPage, Text reward, Text balance, Text target,
        Text remaining, Text percent, Text count, Image fill, ProgressView sourceProgress,
        SlotView sourceSlot, BloatProgressView sourceFinal, BonusWheelView sourceWheel,
        WithdrawFeedbackView sourceFeedback)
    {
        page = configuredPage;
        rewardAmount = reward;
        currentBalance = balance;
        targetAmount = target;
        remainingAmount = remaining;
        progressPercent = percent;
        missionCount = count;
        missionFill = fill;
        stageProgress = sourceProgress;
        slot = sourceSlot;
        finalProgress = sourceFinal;
        bonusWheel = sourceWheel;
        feedback = sourceFeedback;
    }

    private void OnEnable()
    {
        dirty = true;
        nextRefresh = 0f;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + .15f;
        RefreshFromSource();
    }

    public void Invalidate()
    {
        dirty = true;
        nextRefresh = 0f;
    }

    private void RefreshFromSource()
    {
        GameManagerWZ manager = GameManagerWZ.instance;
        if (manager == null) return;
        if (manager != lastManager)
        {
            lastManager = manager;
            dirty = true;
        }
        double balance = manager.currentCoin;
        double target = ReadTarget(manager, 5);
        switch (page)
        {
            case Page.AdMission:
                WithdrawalTaskPresentation mission = WithdrawalTaskPresentation.Read(manager, 3);
                int required = Mathf.Max(1, (int)mission.Target);
                int completed = Mathf.Clamp((int)mission.Current, 0, required);
                if (dirty || completed != lastMissionCurrent || required != lastMissionTarget)
                {
                    if (missionCount != null) missionCount.text = completed + "/" + required;
                    if (missionFill != null) missionFill.fillAmount = (float)completed / required;
                    lastMissionCurrent = completed;
                    lastMissionTarget = required;
                }
                break;
            case Page.Feedback:
                UpdateMoney(rewardAmount, feedback != null ? feedback.AppleSkinSavedAmount : 0d,
                    ref lastAmount, manager, "");
                break;
            case Page.StageProgress:
                UpdateMoney(rewardAmount, stageProgress != null ? stageProgress.AppleSkinRewardAmount : 0d,
                    ref lastAmount, manager, "");
                int percent = stageProgress != null ? Mathf.RoundToInt(stageProgress.AppleSkinProgress * 100f) : 0;
                if (progressPercent != null && (dirty || percent != lastPercent))
                {
                    progressPercent.text = percent + "%";
                    lastPercent = percent;
                }
                break;
            case Page.Slot:
                UpdateMoney(rewardAmount, slot != null ? slot.AppleSkinRewardAmount : 0d,
                    ref lastAmount, manager, "");
                break;
            case Page.BonusIntro:
                UpdateMoney(targetAmount, target, ref lastTarget, manager, "");
                UpdateMoney(currentBalance, balance, ref lastBalance, manager, "");
                break;
            case Page.Compensation:
                UpdateMoney(currentBalance, balance, ref lastBalance, manager, "Balance: ");
                break;
            case Page.FinalProgress:
                if (finalProgress != null) target = finalProgress.AppleSkinTargetAmount;
                UpdateMoney(currentBalance, balance, ref lastBalance, manager, "");
                UpdateMoney(remainingAmount, System.Math.Max(0d, target - balance),
                    ref lastRemaining, manager, "");
                break;
            case Page.BonusWheel:
                UpdateMoney(currentBalance, bonusWheel != null ? bonusWheel.AppleSkinBalanceBeforeSpin : balance,
                    ref lastBalance, manager, "Balance before: ");
                break;
        }
        dirty = false;
    }

    private static double ReadTarget(GameManagerWZ manager, int stage)
    {
        int type;
        int value;
        return manager.TryGetStageRequirement(stage, out type, out value) && type == 2
            ? value : GameManagerWZ.Stage5CompensationTargetCoin;
    }

    private void UpdateMoney(Text text, double value, ref double previous, GameManagerWZ manager, string prefix)
    {
        if (text == null || (!dirty && previous == value)) return;
        text.text = prefix + manager.FormatRegionalValue(value);
        previous = value;
    }

    public Sprite ResolveRewardSprite(ELuckySpinRewardType type, bool large)
    {
        switch (type)
        {
            case ELuckySpinRewardType.Money:
                return Framework2AppleSkinAssets.LoadSprite(large ? "cashpile" : "money");
            case ELuckySpinRewardType.LuckyWalletMoney:
                return Framework2AppleSkinAssets.LoadSprite("wallet");
            case ELuckySpinRewardType.Gem:
                return Framework2AppleSkinAssets.LoadSprite("diamond");
            case ELuckySpinRewardType.Clear:
            case ELuckySpinRewardType.Prompt:
                return clearIcon;
            case ELuckySpinRewardType.Undo:
                return undoIcon;
            default:
                return null;
        }
    }
}
