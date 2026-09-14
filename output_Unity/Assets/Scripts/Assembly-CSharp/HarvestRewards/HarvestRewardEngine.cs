using System;

/// <summary>Pure domain logic. UTC is supplied by the host; all values are simulated units.</summary>
public sealed class HarvestRewardEngine
{
    public HarvestRewardSettings Config { get; private set; }
    public HarvestRewardState State { get; private set; }
    public event Action Changed;
    private readonly Func<double> random;
    private string adRequestId;
    private string adRewardId;
    private long adRewardCents;
    private int adKind;

    public HarvestRewardEngine(HarvestRewardSettings settings, HarvestRewardState state = null, Func<double> random = null)
    {
        if (settings == null) throw new ArgumentNullException("settings");
        settings.Validate();
        Config = settings;
        State = state ?? new HarvestRewardState();
        HarvestRewardStorage.Validate(State, settings);
        Random generator = new Random();
        this.random = random ?? generator.NextDouble;
    }

    public string CurrentRunId { get { return State.ActiveRun == null ? null : State.ActiveRun.RunId; } }
    public bool HasInGameReward { get { return State.PendingInGameReward != null; } }
    public bool HasWinReward { get { return State.PendingWinReward != null; } }
    public bool CanClaimInGameBase { get { return CanClaim(State.PendingInGameReward); } }
    public bool CanClaimWinBase { get { return CanClaim(State.PendingWinReward); } }
    public bool HasPendingAd { get { return adRequestId != null; } }

    public string BeginRun(string runId, long utcNow)
    {
        Tick(utcNow);
        if (string.IsNullOrEmpty(runId)) runId = Guid.NewGuid().ToString("N");
        if (CurrentRunId == runId) return runId;
        // Continuing/trying again always preserves earned base rewards.
        AutoClaimPending();
        State.ActiveRun = new HarvestRunState { RunId = runId, LastOfferEffectiveSeconds = State.EffectivePlaySeconds };
        Notify();
        return runId;
    }

    public bool RecordTriple(string runId, int eventIndex, bool isTutorial, long utcNow, bool isFinalGroup = false)
    {
        Tick(utcNow);
        HarvestRunState run = State.ActiveRun;
        if (run == null || run.RunId != runId || run.WinRecorded || eventIndex < 0 || eventIndex <= run.LastTripleIndex) return false;
        run.LastTripleIndex = eventIndex;
        run.AwaitingWin = isFinalGroup;
        if (!isTutorial)
        {
            run.TriplesSinceOffer++;
            Credit(TripleRewardCents());
            AdvanceTask(HarvestStageSettings.Triples);
            EvaluateProgress();
        }
        Notify();
        return true;
    }

    public bool RecordWin(string runId, bool isTutorial, long utcNow)
    {
        Tick(utcNow);
        HarvestRunState run = State.ActiveRun;
        if (run == null || run.RunId != runId || run.WinRecorded) return false;
        run.WinRecorded = true;
        if (State.PendingInGameReward != null)
        {
            ClaimOffer(State.PendingInGameReward, State.PendingInGameReward.DisplayCents, false);
            State.PendingInGameReward = null;
        }
        if (isTutorial)
        {
            if (!State.TutorialGiftClaimed)
            {
                State.TutorialGiftClaimed = true;
                Credit(Config.TutorialGiftCents);
            }
        }
        else
        {
            AdvanceTask(HarvestStageSettings.Wins);
            EvaluateProgress();
        }
        if (State.PendingWinReward != null) ClaimOffer(State.PendingWinReward, State.PendingWinReward.DisplayCents, false);
        State.PendingWinReward = CreateOffer(HarvestRewardOffer.Win, runId);
        CancelRewardedClaim();
        Notify();
        return true;
    }

    public bool ClaimInGameBase(string runId, long utcNow)
    {
        Tick(utcNow);
        HarvestRewardOffer offer = State.PendingInGameReward;
        if (offer == null || (runId != null && offer.RunId != runId) || !CanClaim(offer) || adRewardId == offer.RewardId) return false;
        ClaimOffer(offer, offer.DisplayCents, false);
        State.PendingInGameReward = null;
        Notify();
        return true;
    }

    public bool ClaimWinBase(string runId, long utcNow)
    {
        Tick(utcNow);
        HarvestRewardOffer offer = State.PendingWinReward;
        if (offer == null || (runId != null && offer.RunId != runId) || !CanClaim(offer) || adRewardId == offer.RewardId) return false;
        ClaimOffer(offer, offer.DisplayCents, false);
        State.PendingWinReward = null;
        Notify();
        return true;
    }

    public long GetInGameRewardPreviewCents() { return Preview(State.PendingInGameReward); }
    public long GetWinRewardPreviewCents() { return Preview(State.PendingWinReward); }
    private static long Preview(HarvestRewardOffer offer) { return offer == null ? 0 : offer.DisplayCents; }

    public bool TryApply(int tierIndex, long utcNow)
    {
        Tick(utcNow);
        if (State.ActiveRequest != null || tierIndex < 0 || tierIndex >= Config.ThresholdCents.Length || State.CompletedTierIndexes.Contains(tierIndex)) return false;
        long amount = Config.ThresholdCents[tierIndex];
        if (State.AvailableCents < amount) return false;
        State.AvailableCents -= amount;
        State.FrozenCents = amount;
        State.ActiveRequest = new HarvestRequestState
        {
            RequestId = Guid.NewGuid().ToString("N"), TierIndex = tierIndex, AmountCents = amount,
            Status = HarvestRequestState.StageActive, StageIndex = 0, StageProgress = 0, StageStartedUtcSeconds = State.LastUtcSeconds
        };
        Notify();
        return true;
    }

    public bool Tick(long utcNow)
    {
        if (utcNow < 0) throw new ArgumentOutOfRangeException("utcNow");
        bool changed = false;
        if (utcNow < State.LastUtcSeconds)
        {
            if (!State.ClockRollbackDetected) { State.ClockRollbackDetected = true; changed = true; }
        }
        else
        {
            State.LastUtcSeconds = utcNow;
            if (State.ClockRollbackDetected) { State.ClockRollbackDetected = false; changed = true; }
        }
        changed |= EvaluateProgress();
        if (changed) Notify();
        return changed;
    }

    public long GetRemainingSeconds()
    {
        HarvestRequestState request = State.ActiveRequest;
        if (request == null) return 0;
        long due = request.Status == HarvestRequestState.SettlementWait
            ? request.SettlementStartedUtcSeconds + Config.SettlementWaitSeconds
            : request.StageStartedUtcSeconds + Config.Stages[request.StageIndex].WaitSeconds;
        return Math.Max(0, due - State.LastUtcSeconds);
    }

    /// <summary>Host calls only while a non-tutorial board is interactive; background time never counts.</summary>
    public void AdvanceEffectivePlay(double seconds, long utcNow)
    {
        Tick(utcNow);
        if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0 || seconds > Config.MaxEffectivePlayDeltaSeconds) return;
        if (State.ActiveRun == null || State.ActiveRun.WinRecorded || State.ActiveRun.AwaitingWin || HasPendingAd) return;
        State.EffectivePlaySeconds += seconds;
        bool changed = false;
        if (!State.ReachedFirstTarget)
        {
            for (int i = 0; i < Config.Milestones.Length; i++)
            {
                if (State.ReachedFirstTarget) break;
                int bit = 1 << i;
                if ((State.MilestoneClaimMask & bit) == 0 && State.EffectivePlaySeconds >= Config.Milestones[i].EffectiveSeconds)
                {
                    State.MilestoneClaimMask |= bit;
                    Credit(Config.Milestones[i].BonusCents);
                    changed = true;
                }
            }
        }
        // Offers are evaluated on interactive play time, after win detection has had priority.
        HarvestRunState run = State.ActiveRun;
        if (State.PendingInGameReward == null && (run.TriplesSinceOffer >= Config.InGameOfferImmediateTriples ||
            (run.TriplesSinceOffer >= Config.InGameOfferTriples && State.EffectivePlaySeconds - run.LastOfferEffectiveSeconds >= Config.InGameOfferEffectiveSeconds)))
        {
            State.PendingInGameReward = CreateOffer(HarvestRewardOffer.InGame, run.RunId);
            run.TriplesSinceOffer = 0;
            run.LastOfferEffectiveSeconds = State.EffectivePlaySeconds;
            changed = true;
        }
        if (changed) Notify();
    }

    /// <summary>Creates a single-use request for a real rewarded SDK adapter. Never call completion on button click.</summary>
    public bool TryBeginRewardedClaim(int kind, out string requestId, out long rewardCents)
    {
        requestId = null;
        rewardCents = 0;
        HarvestRewardOffer offer = kind == HarvestRewardOffer.InGame ? State.PendingInGameReward : kind == HarvestRewardOffer.Win ? State.PendingWinReward : null;
        if (offer == null || offer.IsFirstFree || HasPendingAd) return false;
        adRequestId = Guid.NewGuid().ToString("N");
        adRewardId = offer.RewardId;
        adKind = kind;
        adRewardCents = checked(offer.BaseCents * (kind == HarvestRewardOffer.InGame ? GetInGameMultiplier() : RollWinMultiplier()));
        requestId = adRequestId;
        rewardCents = adRewardCents;
        return true;
    }

    public bool CompleteRewardedClaim(string requestId, bool completed, long utcNow)
    {
        Tick(utcNow);
        if (requestId == null || requestId != adRequestId) return false;
        HarvestRewardOffer offer = adKind == HarvestRewardOffer.InGame ? State.PendingInGameReward : State.PendingWinReward;
        bool valid = completed && offer != null && offer.RewardId == adRewardId;
        long reward = adRewardCents;
        CancelRewardedClaim();
        if (!valid) return false;
        ClaimOffer(offer, reward, true);
        if (offer.Kind == HarvestRewardOffer.InGame) State.PendingInGameReward = null;
        else State.PendingWinReward = null;
        Notify();
        return true;
    }

    public void CancelRewardedClaim()
    {
        adRequestId = null;
        adRewardId = null;
        adKind = 0;
        adRewardCents = 0;
    }

    public double GetWinHighMultiplierProbability()
    {
        long threshold = Config.ThresholdCents[Config.ThresholdCents.Length - 1];
        for (int i = 0; i < Config.ThresholdCents.Length; i++)
            if (!State.CompletedTierIndexes.Contains(i)) { threshold = Config.ThresholdCents[i]; break; }
        double ratio = Math.Min(1, Math.Max(0, (double)State.AvailableCents / threshold));
        HarvestProbabilityPoint[] points = Config.WinProbabilities;
        if (ratio <= points[0].BalanceRatio) return points[0].ForCount(State.WinAdSuccessCount);
        for (int i = 1; i < points.Length; i++)
        {
            if (ratio > points[i].BalanceRatio) continue;
            double fraction = (ratio - points[i - 1].BalanceRatio) / (points[i].BalanceRatio - points[i - 1].BalanceRatio);
            return points[i - 1].ForCount(State.WinAdSuccessCount) + fraction * (points[i].ForCount(State.WinAdSuccessCount) - points[i - 1].ForCount(State.WinAdSuccessCount));
        }
        return points[points.Length - 1].ForCount(State.WinAdSuccessCount);
    }

    private int RollWinMultiplier()
    {
        int[] group = Roll() < GetWinHighMultiplierProbability() ? Config.WinHighMultipliers : Config.WinLowMultipliers;
        return group[Math.Min(group.Length - 1, (int)(Roll() * group.Length))];
    }

    private double Roll() { return Math.Max(0, Math.Min(.999999999, random())); }

    private int GetInGameMultiplier()
    {
        int index = 0;
        for (int i = 1; i < Config.AdCountThresholds.Length; i++) if (State.InGameAdSuccessCount >= Config.AdCountThresholds[i]) index = i;
        return Config.InGameMultipliers[index];
    }

    private long TripleRewardCents()
    {
        if (State.ReachedFirstTarget) return Config.AfterFirstTargetTripleCents;
        HarvestBudgetMilestone[] milestones = Config.Milestones;
        long totalBudget = milestones[milestones.Length - 1].CumulativeTripleBudgetCents;
        if (State.FirstAccelerationPaidCents >= totalBudget) return Config.AfterBudgetTripleCents;
        int index = 0;
        while (index < milestones.Length - 1 && State.EffectivePlaySeconds >= milestones[index].EffectiveSeconds) index++;
        long cap = milestones[index].CumulativeTripleBudgetCents;
        long remaining = cap - State.FirstAccelerationPaidCents;
        if (remaining <= 0) return 0;
        double previousSeconds = index == 0 ? 0 : milestones[index - 1].EffectiveSeconds;
        long previousBudget = index == 0 ? 0 : milestones[index - 1].CumulativeTripleBudgetCents;
        double fraction = Math.Min(1, Math.Max(0, (State.EffectivePlaySeconds - previousSeconds) / (milestones[index].EffectiveSeconds - previousSeconds)));
        double pacedBudget = previousBudget + (cap - previousBudget) * fraction;
        long wanted = (long)Math.Floor(pacedBudget - State.FirstAccelerationPaidCents);
        long result = Math.Min(remaining, Math.Max(Config.FirstTripleMinCents, Math.Min(Config.FirstTripleMaxCents, wanted)));
        State.FirstAccelerationPaidCents += result;
        return result;
    }

    private HarvestRewardOffer CreateOffer(int kind, string runId)
    {
        bool first = kind == HarvestRewardOffer.InGame ? !State.FirstInGameClaimed : !State.FirstWinClaimed;
        long baseCents = kind == HarvestRewardOffer.InGame ? Config.InGameBaseCents : Config.WinBaseCents;
        int multiplier = kind == HarvestRewardOffer.InGame ? Config.FirstInGameMultiplier : Config.FirstWinMultiplier;
        return new HarvestRewardOffer
        {
            RewardId = "harvest_reward_" + State.NextRewardSequence++, RunId = runId, Kind = kind,
            BaseCents = baseCents, DisplayCents = checked(baseCents * (first ? multiplier : 1)), IsFirstFree = first,
            CreatedUtcSeconds = State.LastUtcSeconds
        };
    }

    private bool CanClaim(HarvestRewardOffer offer)
    {
        return offer != null && adRewardId != offer.RewardId && (offer.IsFirstFree || State.LastUtcSeconds - offer.CreatedUtcSeconds >= Config.BaseClaimDelaySeconds);
    }

    private void AutoClaimPending()
    {
        CancelRewardedClaim();
        if (State.PendingInGameReward != null) ClaimOffer(State.PendingInGameReward, State.PendingInGameReward.DisplayCents, false);
        if (State.PendingWinReward != null) ClaimOffer(State.PendingWinReward, State.PendingWinReward.DisplayCents, false);
        State.PendingInGameReward = null;
        State.PendingWinReward = null;
    }

    private void ClaimOffer(HarvestRewardOffer offer, long amount, bool adSuccess)
    {
        Credit(amount);
        if (offer.Kind == HarvestRewardOffer.InGame)
        {
            State.FirstInGameClaimed = true;
            if (adSuccess && State.InGameAdSuccessCount < int.MaxValue) State.InGameAdSuccessCount++;
        }
        else
        {
            State.FirstWinClaimed = true;
            if (adSuccess && State.WinAdSuccessCount < int.MaxValue) State.WinAdSuccessCount++;
        }
    }

    private void Credit(long cents)
    {
        if (cents <= 0) return;
        State.AvailableCents = checked(State.AvailableCents + cents);
        State.LifetimeEarnedCents = checked(State.LifetimeEarnedCents + cents);
        if (!State.ReachedFirstTarget && State.LifetimeEarnedCents >= Config.ThresholdCents[0]) State.ReachedFirstTarget = true;
    }

    private void AdvanceTask(int taskKind)
    {
        HarvestRequestState request = State.ActiveRequest;
        if (request == null || request.Status != HarvestRequestState.StageActive) return;
        HarvestStageSettings stage = Config.Stages[request.StageIndex];
        if (stage.TaskKind == taskKind && request.StageProgress < stage.Target)
        {
            request.StageProgress++;
            if (request.StageProgress == stage.Target) request.StageTaskCompletedUtcSeconds = State.LastUtcSeconds;
        }
    }

    private bool EvaluateProgress()
    {
        HarvestRequestState request = State.ActiveRequest;
        if (request == null) return false;
        if (request.Status == HarvestRequestState.SettlementWait)
        {
            if (GetRemainingSeconds() > 0) return false;
            CompleteSettlement(request);
            return true;
        }
        HarvestStageSettings stage = Config.Stages[request.StageIndex];
        if (request.StageProgress < stage.Target || GetRemainingSeconds() > 0) return false;
        long completedUtcSeconds = Math.Max(request.StageStartedUtcSeconds + stage.WaitSeconds, request.StageTaskCompletedUtcSeconds);
        if (request.StageIndex == Config.Stages.Length - 1)
        {
            request.Status = HarvestRequestState.SettlementWait;
            request.SettlementStartedUtcSeconds = completedUtcSeconds;
            if (GetRemainingSeconds() == 0) CompleteSettlement(request);
        }
        else
        {
            request.StageIndex++;
            request.StageProgress = 0;
            request.StageStartedUtcSeconds = completedUtcSeconds;
            request.StageTaskCompletedUtcSeconds = 0;
        }
        return true;
    }

    private void CompleteSettlement(HarvestRequestState request)
    {
        State.Settlements.Add(new HarvestSettlementRecord { RequestId = request.RequestId, TierIndex = request.TierIndex, AmountCents = request.AmountCents, CompletedUtcSeconds = request.SettlementStartedUtcSeconds + Config.SettlementWaitSeconds });
        State.CompletedTierIndexes.Add(request.TierIndex);
        State.FrozenCents = 0;
        State.ActiveRequest = null;
    }

    private void Notify() { Action handler = Changed; if (handler != null) handler(); }
}
