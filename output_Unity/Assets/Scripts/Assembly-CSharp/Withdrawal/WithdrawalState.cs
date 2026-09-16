using System;
using System.Collections.Generic;

namespace FruitWithdrawal
{
    [Serializable]
    public sealed class WithdrawalRun
    {
        public string RunId;
        public int LastTripleSequence;
        public bool Won;
    }

    [Serializable]
    public sealed class WithdrawalReward
    {
        public string RewardId;
        public string RunId;
        public WithdrawalRewardKind Kind;
        public long CashCents;
        public long DiamondUnits;
        public long CreatedUtcSeconds;
    }

    [Serializable]
    public sealed class WithdrawalRequest
    {
        public string RequestId;
        public string TierId;
        public string ChannelId;
        public string ChannelLabel;
        public WithdrawalTierRule Terms;
        public WithdrawalRequestStatus Status;
        public int CompletedConditionsMask;
        public long CreatedUtcSeconds;
        public long ReviewStartedUtcSeconds;
        public long ResolvedUtcSeconds;
        public bool IsActive { get { return Status == WithdrawalRequestStatus.ConditionsPending || Status == WithdrawalRequestStatus.ReviewPending; } }
        public bool IsSimulation { get { return true; } }
    }

    public sealed class WithdrawalConditionProgress
    {
        public WithdrawalConditionKind Kind;
        public string Label;
        public long Current;
        public long Target;
        public bool Complete;
    }

    [Serializable]
    public sealed class WithdrawalState
    {
        public long CashCents;
        public long DiamondUnits;
        public long FrozenCashCents;
        public long FrozenDiamondUnits;
        public int TotalWins;
        public int HighestClearedLevel;
        public int TodayWins;
        public int QualifiedCheckInDays;
        public long CurrentUtcDay = -1;
        public long LastQualifiedUtcDay = -1;
        public long LastUtcSeconds;
        public bool ClockRollbackDetected;
        public bool FirstWinBonusGranted;
        public long NextRewardNumber = 1;
        public long NextRequestNumber = 1;
        public string ActiveRunId = "";
        public readonly List<WithdrawalRun> Runs = new List<WithdrawalRun>();
        public readonly List<WithdrawalReward> PendingRewards = new List<WithdrawalReward>();
        public readonly List<WithdrawalRequest> Requests = new List<WithdrawalRequest>();

        public WithdrawalRequest ActiveRequest
        {
            get { for (int i = Requests.Count - 1; i >= 0; i--) if (Requests[i].IsActive) return Requests[i]; return null; }
        }

        public void Validate()
        {
            WithdrawalRules.CheckUnits(CashCents); WithdrawalRules.CheckUnits(DiamondUnits);
            WithdrawalRules.CheckUnits(FrozenCashCents); WithdrawalRules.CheckUnits(FrozenDiamondUnits);
            WithdrawalRules.CheckUnits(checked(CashCents + FrozenCashCents));
            WithdrawalRules.CheckUnits(checked(DiamondUnits + FrozenDiamondUnits));
            if (TotalWins < 0 || HighestClearedLevel < 0 || TodayWins < 0 || QualifiedCheckInDays < 0 || TodayWins > TotalWins ||
                CurrentUtcDay < -1 || LastQualifiedUtcDay < -1 || LastQualifiedUtcDay > CurrentUtcDay || LastUtcSeconds < 0 || LastUtcSeconds > 253402300799L ||
                NextRewardNumber < 1 || NextRequestNumber < 1 || Runs.Count > 100000 || PendingRewards.Count > 100000 || Requests.Count > 10000)
                throw new InvalidOperationException("Invalid simulation state.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < Runs.Count; i++) {
                WithdrawalRun run = Runs[i];
                if (run == null || run.LastTripleSequence < 0) throw new InvalidOperationException("Invalid run checkpoint.");
                WithdrawalRules.RequireId(run.RunId);
                if (!ids.Add(run.RunId)) throw new InvalidOperationException("Duplicate run checkpoint.");
            }
            if (!string.IsNullOrEmpty(ActiveRunId) && !ids.Contains(ActiveRunId)) throw new InvalidOperationException("Missing active run checkpoint.");
            ids.Clear();
            for (int i = 0; i < PendingRewards.Count; i++) {
                WithdrawalReward reward = PendingRewards[i];
                if (reward == null || reward.CreatedUtcSeconds < 0 || (reward.Kind != WithdrawalRewardKind.Triple && reward.Kind != WithdrawalRewardKind.Win))
                    throw new InvalidOperationException("Invalid pending reward.");
                WithdrawalRules.RequireId(reward.RewardId); WithdrawalRules.RequireId(reward.RunId);
                WithdrawalRules.CheckUnits(reward.CashCents); WithdrawalRules.CheckUnits(reward.DiamondUnits);
                if (!ids.Add(reward.RewardId)) throw new InvalidOperationException("Duplicate reward.");
            }
            ids.Clear();
            int active = 0;
            long reservedCash = 0, reservedDiamonds = 0;
            for (int i = 0; i < Requests.Count; i++) {
                WithdrawalRequest request = Requests[i];
                if (request == null || request.Terms == null || request.Status < WithdrawalRequestStatus.ConditionsPending ||
                    request.Status > WithdrawalRequestStatus.Cancelled || request.CreatedUtcSeconds < 0 || request.ReviewStartedUtcSeconds < 0 ||
                    request.ResolvedUtcSeconds < 0 || request.CompletedConditionsMask < 0 || request.CompletedConditionsMask > 127)
                    throw new InvalidOperationException("Invalid simulated request.");
                WithdrawalRules.RequireId(request.RequestId); WithdrawalRules.RequireId(request.ChannelId);
                request.Terms.Validate();
                if (request.TierId != request.Terms.Id || !ids.Add(request.RequestId)) throw new InvalidOperationException("Invalid request terms.");
                if ((request.Status == WithdrawalRequestStatus.ReviewPending || request.Status == WithdrawalRequestStatus.SimulatedCompleted) &&
                    (request.CompletedConditionsMask != 127 || request.ReviewStartedUtcSeconds < request.CreatedUtcSeconds))
                    throw new InvalidOperationException("Review is missing its completed published conditions.");
                if (request.IsActive) {
                    active++;
                    if (request.Terms.Currency == WithdrawalCurrency.Cash) reservedCash += request.Terms.RequiredBalanceUnits;
                    else reservedDiamonds += request.Terms.RequiredBalanceUnits;
                }
            }
            if (active > 1 || reservedCash != FrozenCashCents || reservedDiamonds != FrozenDiamondUnits)
                throw new InvalidOperationException("Simulation reservation does not match its active request.");
        }
    }
}
