using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>A small, serialized HUD. No game objects or visual styles are generated at runtime.</summary>
public sealed class HarvestRewardsHud : MonoBehaviour
{
    [SerializeField] private Button walletButton;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private Slider goalProgressBar;
    [SerializeField] private GameObject stageTrackRoot;
    [SerializeField] private GameObject[] completedMarks;
    [SerializeField] private GameObject[] currentMarks;
    [SerializeField, Min(0.25f)] private float refreshIntervalSeconds = 1f;
    private HarvestRewardService service;
    private float nextRefresh;
    private bool bound;
    private bool subscribed;

    private void OnEnable()
    {
        if (!bound)
        {
            walletButton.onClick.AddListener(HarvestRewardsUI.Open);
            bound = true;
        }
        service = HarvestRewardService.Instance;
        if (!subscribed)
        {
            service.Changed += Refresh;
            subscribed = true;
        }
        Refresh();
    }

    private void OnDisable() { Unsubscribe(); }
    private void OnDestroy() { Unsubscribe(); }
    private void Unsubscribe()
    {
        if (subscribed && service != null) service.Changed -= Refresh;
        subscribed = false;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + refreshIntervalSeconds;
        Refresh();
    }

    private void Refresh()
    {
        if (!service.IsAvailable)
        {
            balanceText.text = "--";
            goalText.text = "Rewards unavailable";
            goalProgressBar.SetValueWithoutNotify(0);
            stageTrackRoot.SetActive(false);
            return;
        }
        HarvestRewardState state = service.State;
        HarvestRequestState request = state.ActiveRequest;
        balanceText.text = HarvestRewardsUI.FormatAmount(state.AvailableCents);
        stageTrackRoot.SetActive(request != null);
        for (int i = 0; i < completedMarks.Length; i++)
            completedMarks[i].SetActive(request != null && (request.Status == HarvestRequestState.SettlementWait || i < request.StageIndex));
        for (int i = 0; i < currentMarks.Length; i++)
            currentMarks[i].SetActive(request != null && request.Status == HarvestRequestState.StageActive && i == request.StageIndex);
        if (request == null)
        {
            if (state.CompletedTierIndexes.Count >= service.Config.ThresholdCents.Length)
            {
                goalText.text = "All harvest challenges completed";
                goalProgressBar.SetValueWithoutNotify(1);
                return;
            }
            int tier = HarvestRewardsUI.FirstUnfinishedTier(service.Config, state);
            long target = service.Config.ThresholdCents[tier];
            long missing = Math.Max(0, target - state.AvailableCents);
            goalText.text = missing == 0 ? "Ready to withdraw " + HarvestRewardsUI.FormatAmount(target)
                : "Earn " + HarvestRewardsUI.FormatAmount(missing) + " more to apply";
            goalProgressBar.SetValueWithoutNotify(Mathf.Clamp01((float)((double)state.AvailableCents / target)));
            return;
        }
        if (request.Status == HarvestRequestState.SettlementWait)
        {
            goalText.text = "Final wait: " + HarvestRewardsUI.FormatDuration(service.GetRemainingSeconds());
            goalProgressBar.SetValueWithoutNotify(1);
            return;
        }
        HarvestStageSettings stage = service.Config.Stages[request.StageIndex];
        string unit = stage.TaskKind == HarvestStageSettings.Wins ? " levels" : " triples";
        goalText.text = "Stage " + (request.StageIndex + 1) + ": " + request.StageProgress + " / " + stage.Target + unit;
        goalProgressBar.SetValueWithoutNotify(Mathf.Clamp01((float)request.StageProgress / stage.Target));
    }
}
