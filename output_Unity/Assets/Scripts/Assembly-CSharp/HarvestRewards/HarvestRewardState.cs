using System;
using System.Collections.Generic;

// Integer cents are simulated reward units only. This ledger never represents money.
[Serializable]
public sealed class HarvestRewardState
{
    public const int CurrentVersion = 1;
    public int Version = CurrentVersion;
    public long AvailableCents;
    public long FrozenCents;
    public long LifetimeEarnedCents;
    public bool TutorialGiftClaimed;
    public bool FirstInGameClaimed;
    public bool FirstWinClaimed;
    public bool ReachedFirstTarget;
    public int InGameAdSuccessCount;
    public int WinAdSuccessCount;
    public double EffectivePlaySeconds;
    public long FirstAccelerationPaidCents;
    public int MilestoneClaimMask;
    public long LastUtcSeconds;
    public bool ClockRollbackDetected;
    public long NextRewardSequence = 1;
    public List<int> CompletedTierIndexes = new List<int>();
    public List<HarvestSettlementRecord> Settlements = new List<HarvestSettlementRecord>();
    public HarvestRunState ActiveRun;
    public HarvestRewardOffer PendingInGameReward;
    public HarvestRewardOffer PendingWinReward;
    public HarvestRequestState ActiveRequest;
}

[Serializable]
public sealed class HarvestRunState
{
    public string RunId;
    public int LastTripleIndex = -1;
    public bool WinRecorded;
    public bool AwaitingWin;
    public int TriplesSinceOffer;
    public double LastOfferEffectiveSeconds;
}

[Serializable]
public sealed class HarvestRewardOffer
{
    public const int InGame = 1;
    public const int Win = 2;
    public string RewardId;
    public string RunId;
    public int Kind;
    public long CreatedUtcSeconds;
    public bool IsFirstFree;
    public long BaseCents;
    public long DisplayCents;
}

[Serializable]
public sealed class HarvestRequestState
{
    public const int StageActive = 1;
    public const int SettlementWait = 2;
    public string RequestId;
    public int TierIndex;
    public long AmountCents;
    public int Status;
    public int StageIndex;
    public int StageProgress;
    public long StageStartedUtcSeconds;
    public long StageTaskCompletedUtcSeconds;
    public long SettlementStartedUtcSeconds;
}

[Serializable]
public sealed class HarvestSettlementRecord
{
    public string RequestId;
    public int TierIndex;
    public long AmountCents;
    public long CompletedUtcSeconds;
}

[Serializable]
public sealed class HarvestStageSettings
{
    public const int Wins = 1;
    public const int Triples = 2;
    public string Id;
    public string DisplayName;
    public int TaskKind;
    public int Target;
    public int WaitSeconds;
}

[Serializable]
public sealed class HarvestBudgetMilestone
{
    public int EffectiveSeconds;
    public long CumulativeTripleBudgetCents;
    public long BonusCents;
}

[Serializable]
public sealed class HarvestProbabilityPoint
{
    public float BalanceRatio;
    public float NoAds;
    public float OneToTwoAds;
    public float ThreeToFiveAds;
    public float SixOrMoreAds;

    public double ForCount(int count)
    {
        if (count <= 0) return NoAds;
        if (count <= 2) return OneToTwoAds;
        if (count <= 5) return ThreeToFiveAds;
        return SixOrMoreAds;
    }
}

[Serializable]
public sealed class HarvestRewardSettings
{
    public string PlanTitle = "丰收奖励";
    public string SimulationNotice = "模拟奖励 · 不可兑换现金";
    public string RulesText = "达到模拟额度后可开启六站挑战。每站任务与24小时等待同时完成后进入下一站，六站完成后等待168小时，最短13天。申请额转入冻结余额，每档完成一次，同时仅一个申请。不涉及现金兑付。";
    public long[] ThresholdCents = { 100000, 150000, 300000, 500000, 800000, 1000000 };
    public HarvestStageSettings[] Stages =
    {
        new HarvestStageSettings { Id = "harvest_01", DisplayName = "初次丰收", TaskKind = HarvestStageSettings.Wins, Target = 10, WaitSeconds = 86400 },
        new HarvestStageSettings { Id = "harvest_02", DisplayName = "收获积累", TaskKind = HarvestStageSettings.Triples, Target = 260, WaitSeconds = 86400 },
        new HarvestStageSettings { Id = "harvest_03", DisplayName = "进阶丰收", TaskKind = HarvestStageSettings.Wins, Target = 10, WaitSeconds = 86400 },
        new HarvestStageSettings { Id = "harvest_04", DisplayName = "持续收获", TaskKind = HarvestStageSettings.Triples, Target = 300, WaitSeconds = 86400 },
        new HarvestStageSettings { Id = "harvest_05", DisplayName = "丰收高手", TaskKind = HarvestStageSettings.Wins, Target = 10, WaitSeconds = 86400 },
        new HarvestStageSettings { Id = "harvest_06", DisplayName = "丰收总挑战", TaskKind = HarvestStageSettings.Triples, Target = 500, WaitSeconds = 86400 }
    };
    public int SettlementWaitSeconds = 604800;
    public long TutorialGiftCents = 2000;
    public long InGameBaseCents = 500;
    public long WinBaseCents = 1000;
    public int FirstInGameMultiplier = 2;
    public int FirstWinMultiplier = 10;
    public int BaseClaimDelaySeconds = 3;
    public long FirstTripleMinCents = 125;
    public long FirstTripleMaxCents = 300;
    public long AfterBudgetTripleCents = 200;
    public long AfterFirstTargetTripleCents = 100;
    public int InGameOfferTriples = 11;
    public int InGameOfferEffectiveSeconds = 60;
    public int InGameOfferImmediateTriples = 17;
    public int[] InGameMultipliers = { 2, 4, 6, 8 };
    public int[] AdCountThresholds = { 0, 1, 3, 6 };
    public int[] WinLowMultipliers = { 2, 3, 4, 5 };
    public int[] WinHighMultipliers = { 6, 7, 8, 9, 10 };
    public HarvestProbabilityPoint[] WinProbabilities =
    {
        new HarvestProbabilityPoint { BalanceRatio = 0, NoAds = .820f, OneToTwoAds = .986f, ThreeToFiveAds = .995f, SixOrMoreAds = .997f },
        new HarvestProbabilityPoint { BalanceRatio = .50f, NoAds = .350f, OneToTwoAds = .896f, ThreeToFiveAds = .956f, SixOrMoreAds = .972f },
        new HarvestProbabilityPoint { BalanceRatio = .80f, NoAds = .120f, OneToTwoAds = .686f, ThreeToFiveAds = .845f, SixOrMoreAds = .897f },
        new HarvestProbabilityPoint { BalanceRatio = .95f, NoAds = .060f, OneToTwoAds = .505f, ThreeToFiveAds = .719f, SixOrMoreAds = .803f }
    };
    public HarvestBudgetMilestone[] Milestones =
    {
        new HarvestBudgetMilestone { EffectiveSeconds = 300, CumulativeTripleBudgetCents = 8000, BonusCents = 3000 },
        new HarvestBudgetMilestone { EffectiveSeconds = 600, CumulativeTripleBudgetCents = 17000, BonusCents = 4000 },
        new HarvestBudgetMilestone { EffectiveSeconds = 900, CumulativeTripleBudgetCents = 27000, BonusCents = 5000 },
        new HarvestBudgetMilestone { EffectiveSeconds = 1200, CumulativeTripleBudgetCents = 37000, BonusCents = 6000 },
        new HarvestBudgetMilestone { EffectiveSeconds = 1500, CumulativeTripleBudgetCents = 48000, BonusCents = 7000 }
    };
    public int SaveIntervalSeconds = 15;
    public int MaxEffectivePlayDeltaSeconds = 5;

    public void Validate()
    {
        if (ThresholdCents == null || ThresholdCents.Length == 0 || ThresholdCents.Length > 32) throw new InvalidOperationException("Harvest thresholds missing.");
        for (int i = 0; i < ThresholdCents.Length; i++)
            if (ThresholdCents[i] <= 0 || (i > 0 && ThresholdCents[i] <= ThresholdCents[i - 1])) throw new InvalidOperationException("Harvest thresholds must increase.");
        if (Stages == null || Stages.Length != 6) throw new InvalidOperationException("Harvest requires six disclosed stages.");
        for (int i = 0; i < Stages.Length; i++)
            if (Stages[i] == null || string.IsNullOrEmpty(Stages[i].Id) || Stages[i].Target < 1 || Stages[i].WaitSeconds < 0 || (Stages[i].TaskKind != HarvestStageSettings.Wins && Stages[i].TaskKind != HarvestStageSettings.Triples)) throw new InvalidOperationException("Invalid harvest stage.");
        if (SettlementWaitSeconds < 0 || TutorialGiftCents < 0 || InGameBaseCents <= 0 || WinBaseCents <= 0 || FirstTripleMinCents <= 0 || FirstTripleMaxCents < FirstTripleMinCents || AfterBudgetTripleCents < 0 || AfterFirstTargetTripleCents < 0 || FirstInGameMultiplier < 1 || FirstWinMultiplier < 1 || BaseClaimDelaySeconds < 0 || SaveIntervalSeconds < 1 || MaxEffectivePlayDeltaSeconds < 1) throw new InvalidOperationException("Invalid harvest reward values.");
        if (InGameOfferTriples < 1 || InGameOfferImmediateTriples < InGameOfferTriples || InGameOfferEffectiveSeconds < 0) throw new InvalidOperationException("Invalid harvest offer thresholds.");
        if (InGameMultipliers == null || AdCountThresholds == null || InGameMultipliers.Length != AdCountThresholds.Length || InGameMultipliers.Length == 0 || AdCountThresholds[0] != 0) throw new InvalidOperationException("Invalid harvest ad brackets.");
        for (int i = 0; i < InGameMultipliers.Length; i++)
            if (InGameMultipliers[i] < 1 || (i > 0 && AdCountThresholds[i] <= AdCountThresholds[i - 1])) throw new InvalidOperationException("Invalid harvest ad bracket.");
        ValidateMultipliers(WinLowMultipliers);
        ValidateMultipliers(WinHighMultipliers);
        if (Milestones == null || Milestones.Length == 0 || Milestones.Length > 30) throw new InvalidOperationException("Invalid harvest milestones.");
        for (int i = 0; i < Milestones.Length; i++)
            if (Milestones[i] == null || Milestones[i].EffectiveSeconds <= 0 || Milestones[i].CumulativeTripleBudgetCents <= 0 || Milestones[i].BonusCents < 0 || (i > 0 && (Milestones[i].EffectiveSeconds <= Milestones[i - 1].EffectiveSeconds || Milestones[i].CumulativeTripleBudgetCents <= Milestones[i - 1].CumulativeTripleBudgetCents))) throw new InvalidOperationException("Invalid harvest milestone order.");
        if (WinProbabilities == null || WinProbabilities.Length == 0) throw new InvalidOperationException("Harvest probabilities missing.");
        for (int i = 0; i < WinProbabilities.Length; i++)
        {
            HarvestProbabilityPoint point = WinProbabilities[i];
            if (point == null || float.IsNaN(point.BalanceRatio) || float.IsInfinity(point.BalanceRatio) || point.BalanceRatio < 0 || point.BalanceRatio > 1 || (i > 0 && point.BalanceRatio <= WinProbabilities[i - 1].BalanceRatio)) throw new InvalidOperationException("Invalid harvest probability ratios.");
            for (int j = 0; j < 4; j++) { double p = point.ForCount(j == 0 ? 0 : j == 1 ? 1 : j == 2 ? 3 : 6); if (double.IsNaN(p) || p < 0 || p > 1) throw new InvalidOperationException("Invalid harvest probability."); }
        }
    }

    private static void ValidateMultipliers(int[] values)
    {
        if (values == null || values.Length == 0) throw new InvalidOperationException("Harvest multiplier set missing.");
        for (int i = 0; i < values.Length; i++) if (values[i] < 1) throw new InvalidOperationException("Invalid harvest multiplier.");
    }
}
