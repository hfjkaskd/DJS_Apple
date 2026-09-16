using System;
using System.Collections.Generic;
using System.Globalization;

namespace FruitWithdrawal
{
    /// <summary>Local simulation only. Contains no payment, account collection or advertisement completion API.</summary>
    public sealed class WithdrawalEngine
    {
        public WithdrawalRules Rules { get; private set; }
        public WithdrawalState State { get; private set; }
        public string LastError { get; private set; }
        private readonly Dictionary<string, WithdrawalRun> runs = new Dictionary<string, WithdrawalRun>(StringComparer.Ordinal);

        public WithdrawalEngine(WithdrawalRules rules, WithdrawalState state = null)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            rules.Validate();
            Rules = rules.Copy();
            State = state ?? new WithdrawalState();
            State.Validate();
            for (int i = 0; i < State.Runs.Count; i++) runs.Add(State.Runs[i].RunId, State.Runs[i]);
            LastError = "";
        }

        public int UserLevel { get { return (int)Math.Min(int.MaxValue, 1L + State.TotalWins / Rules.WinsPerUserLevel); } }

        public bool BeginRun(string runId, long utcSeconds, int initialSequence = 0)
        {
            LastError = "";
            WithdrawalRules.RequireId(runId);
            if (initialSequence < 0) return Fail("Initial match sequence cannot be negative.");
            bool changed = Tick(utcSeconds);
            if (!runs.ContainsKey(runId)) {
                // A migrated gameplay save can contain matches made before this module existed.
                // Only a new run adopts that baseline; resumed simulation checkpoints remain authoritative.
                var run = new WithdrawalRun { RunId = runId, LastTripleSequence = initialSequence };
                State.Runs.Add(run); runs.Add(runId, run); changed = true;
            }
            if (State.ActiveRunId != runId) { State.ActiveRunId = runId; changed = true; }
            return changed;
        }

        public bool RecordTriple(string runId, int sequence, bool tutorial, long utcSeconds)
        {
            LastError = "";
            WithdrawalRun run;
            if (!TryActiveRun(runId, out run)) return false;
            if (sequence <= run.LastTripleSequence || run.Won) return false;
            if (sequence < 1 || sequence - run.LastTripleSequence > 100000) return Fail("Invalid match sequence.");
            Tick(utcSeconds);
            int delta = sequence - run.LastTripleSequence;
            if (!tutorial || Rules.RewardTutorial)
                AddReward(runId, WithdrawalRewardKind.Triple, checked(Rules.TripleCashCents * delta), checked(Rules.TripleDiamondUnits * delta));
            run.LastTripleSequence = sequence;
            return true;
        }

        public bool RecordWin(string runId, int wonLevel, bool tutorial, long utcSeconds)
        {
            LastError = "";
            WithdrawalRun run;
            if (!TryActiveRun(runId, out run)) return false;
            if (run.Won) return false;
            if (wonLevel < 1) return Fail("Invalid cleared level.");
            Tick(utcSeconds);
            State.TotalWins = checked(State.TotalWins + 1);
            State.TodayWins = checked(State.TodayWins + 1);
            State.HighestClearedLevel = Math.Max(State.HighestClearedLevel, wonLevel);
            if (State.TodayWins >= Rules.DailyWinsForCheckIn && State.LastQualifiedUtcDay != State.CurrentUtcDay) {
                State.QualifiedCheckInDays = checked(State.QualifiedCheckInDays + 1);
                State.LastQualifiedUtcDay = State.CurrentUtcDay;
            }
            if (!tutorial || Rules.RewardTutorial) {
                long diamonds = Rules.WinDiamondUnits;
                if (!State.FirstWinBonusGranted) {
                    diamonds = checked(diamonds + Rules.FirstWinBonusDiamondUnits);
                    State.FirstWinBonusGranted = true;
                }
                AddReward(runId, WithdrawalRewardKind.Win, Rules.WinCashCents, diamonds);
            }
            run.Won = true;
            EvaluateActive();
            return true;
        }

        public bool ClaimReward(string rewardId, long utcSeconds)
        {
            LastError = "";
            for (int i = 0; i < State.PendingRewards.Count; i++) {
                WithdrawalReward reward = State.PendingRewards[i];
                if (reward.RewardId != rewardId) continue;
                Tick(utcSeconds);
                long cash = checked(State.CashCents + reward.CashCents), diamonds = checked(State.DiamondUnits + reward.DiamondUnits);
                WithdrawalRules.CheckUnits(checked(cash + State.FrozenCashCents)); WithdrawalRules.CheckUnits(checked(diamonds + State.FrozenDiamondUnits));
                State.CashCents = cash; State.DiamondUnits = diamonds;
                State.PendingRewards.RemoveAt(i);
                EvaluateActive();
                return true;
            }
            return false;
        }

        public bool ClaimAllRewards(long utcSeconds)
        {
            LastError = "";
            if (State.PendingRewards.Count == 0) return false;
            Tick(utcSeconds);
            long cash = State.CashCents, diamonds = State.DiamondUnits;
            for (int i = 0; i < State.PendingRewards.Count; i++) {
                cash = checked(cash + State.PendingRewards[i].CashCents);
                diamonds = checked(diamonds + State.PendingRewards[i].DiamondUnits);
            }
            WithdrawalRules.CheckUnits(checked(cash + State.FrozenCashCents));
            WithdrawalRules.CheckUnits(checked(diamonds + State.FrozenDiamondUnits));
            State.CashCents = cash; State.DiamondUnits = diamonds;
            State.PendingRewards.Clear();
            EvaluateActive();
            return true;
        }

        public bool Apply(string tierId, string testChannelId, long utcSeconds)
        {
            LastError = "";
            Tick(utcSeconds);
            if (State.ActiveRequest != null) return Fail("Finish or cancel the current simulated request first.");
            WithdrawalTierRule tier = FindTier(tierId);
            WithdrawalChannelRule channel = FindChannel(testChannelId);
            if (tier == null || channel == null) return Fail("Select a configured test amount and channel.");
            for (int i = 0; i < State.Requests.Count; i++)
                if (State.Requests[i].TierId == tierId && State.Requests[i].Status == WithdrawalRequestStatus.SimulatedCompleted)
                    return Fail("This test tier is already complete. Select another tier.");
            long balance = tier.Currency == WithdrawalCurrency.Cash ? State.CashCents : State.DiamondUnits;
            if (balance < tier.RequiredBalanceUnits) return Fail("Insufficient test balance. Claim rewards and keep playing.");
            var request = new WithdrawalRequest {
                RequestId = "simulation-" + State.NextRequestNumber.ToString(CultureInfo.InvariantCulture),
                TierId = tier.Id, ChannelId = channel.Id, ChannelLabel = channel.Label, Terms = tier.Copy(),
                Status = WithdrawalRequestStatus.ConditionsPending, CreatedUtcSeconds = State.LastUtcSeconds
            };
            State.NextRequestNumber = checked(State.NextRequestNumber + 1);
            if (tier.Currency == WithdrawalCurrency.Cash) {
                State.CashCents -= tier.RequiredBalanceUnits; State.FrozenCashCents = tier.RequiredBalanceUnits;
            } else {
                State.DiamondUnits -= tier.RequiredBalanceUnits; State.FrozenDiamondUnits = tier.RequiredBalanceUnits;
            }
            State.Requests.Add(request);
            EvaluateActive();
            return true;
        }

        public bool CancelRequest(string requestId, long utcSeconds)
        {
            return CloseSimulation(requestId, WithdrawalRequestStatus.Cancelled, utcSeconds);
        }

        public bool ResolveSimulation(string requestId, bool approved, long utcSeconds)
        {
            return CloseSimulation(requestId, approved ? WithdrawalRequestStatus.SimulatedCompleted : WithdrawalRequestStatus.Rejected, utcSeconds);
        }

        private bool CloseSimulation(string requestId, WithdrawalRequestStatus status, long utcSeconds)
        {
            LastError = "";
            WithdrawalRequest request = State.ActiveRequest;
            if (request == null || request.RequestId != requestId) return false;
            AdvanceClock(utcSeconds);
            if (status == WithdrawalRequestStatus.SimulatedCompleted &&
                (request.Status != WithdrawalRequestStatus.ReviewPending || Remaining(request, State.LastUtcSeconds) > 0))
                return Fail("Complete the published conditions and simulated review countdown first.");
            Finish(request, status);
            return true;
        }

        public bool Tick(long utcSeconds)
        {
            LastError = "";
            bool changed = AdvanceClock(utcSeconds);
            return EvaluateActive() || changed;
        }

        // UI may call once a second; persistence only needs meaningful transitions or a clock checkpoint.
        public bool RequiresTick(long utcSeconds)
        {
            if (utcSeconds < 0) return true;
            if (utcSeconds < State.LastUtcSeconds && !State.ClockRollbackDetected) return true;
            long effective = Math.Max(utcSeconds, State.LastUtcSeconds);
            if (effective / 86400 != State.CurrentUtcDay || effective - State.LastUtcSeconds >= 30) return true;
            WithdrawalRequest active = State.ActiveRequest;
            return active != null && active.Status == WithdrawalRequestStatus.ReviewPending && Remaining(active, effective) == 0;
        }

        public long GetRemainingReviewSeconds(long utcSeconds)
        {
            WithdrawalRequest request = State.ActiveRequest;
            if (request == null || request.Status != WithdrawalRequestStatus.ReviewPending) return 0;
            return Remaining(request, Math.Max(utcSeconds, State.LastUtcSeconds));
        }

        public WithdrawalTierRule FindTier(string tierId)
        {
            for (int i = 0; i < Rules.Tiers.Length; i++) if (Rules.Tiers[i].Id == tierId) return Rules.Tiers[i];
            return null;
        }

        public WithdrawalConditionProgress[] GetConditions(string tierId)
        {
            WithdrawalRequest active = State.ActiveRequest;
            if (active != null && active.TierId != tierId) active = null;
            WithdrawalTierRule tier = active == null ? FindTier(tierId) : active.Terms;
            if (tier == null) return new WithdrawalConditionProgress[0];
            var progress = new WithdrawalConditionProgress[7];
            for (int i = 0; i < progress.Length; i++) {
                var kind = (WithdrawalConditionKind)(i + 1);
                long target = Target(tier, kind), current = Current(tier, kind, active != null);
                bool complete = target == 0 || current >= target || (active != null && (active.CompletedConditionsMask & (1 << i)) != 0);
                progress[i] = new WithdrawalConditionProgress { Kind = kind, Label = ConditionLabel(kind),
                    Current = complete ? Math.Max(current, target) : current, Target = target, Complete = complete };
            }
            return progress;
        }

        public static string ConditionLabel(WithdrawalConditionKind kind)
        {
            switch (kind) {
                case WithdrawalConditionKind.PassLevel: return "Clear fruit levels";
                case WithdrawalConditionKind.PassHideLevel: return "First level milestone";
                case WithdrawalConditionKind.PassStage2Level: return "Final level milestone";
                case WithdrawalConditionKind.GoldTarget: return "Required test balance";
                case WithdrawalConditionKind.LoginPassLevel: return "Win fruit levels in one UTC day";
                case WithdrawalConditionKind.LoginDay: return "Days with the published daily win goal";
                case WithdrawalConditionKind.UserLevel: return "Fruit rank from completed levels";
                default: return "Unknown condition";
            }
        }

        private bool AdvanceClock(long utcSeconds)
        {
            if (utcSeconds < 0 || utcSeconds > 253402300799L) throw new ArgumentOutOfRangeException(nameof(utcSeconds));
            bool changed = false;
            if (utcSeconds < State.LastUtcSeconds) {
                changed = !State.ClockRollbackDetected; State.ClockRollbackDetected = true;
            } else if (utcSeconds != State.LastUtcSeconds) {
                State.LastUtcSeconds = utcSeconds; changed = true;
            }
            long day = State.LastUtcSeconds / 86400;
            if (State.CurrentUtcDay != day) { State.CurrentUtcDay = day; State.TodayWins = 0; changed = true; }
            return changed;
        }

        private bool EvaluateActive()
        {
            WithdrawalRequest request = State.ActiveRequest;
            if (request == null) return false;
            bool changed = false;
            if (request.Status == WithdrawalRequestStatus.ConditionsPending) {
                for (int i = 0; i < 7; i++) {
                    int bit = 1 << i;
                    if ((request.CompletedConditionsMask & bit) != 0) continue;
                    var kind = (WithdrawalConditionKind)(i + 1);
                    if (Current(request.Terms, kind, true) >= Target(request.Terms, kind)) {
                        request.CompletedConditionsMask |= bit; changed = true;
                    }
                }
                if (request.CompletedConditionsMask == 127) {
                    request.Status = WithdrawalRequestStatus.ReviewPending;
                    request.ReviewStartedUtcSeconds = State.LastUtcSeconds; changed = true;
                }
            }
            if (request.Status == WithdrawalRequestStatus.ReviewPending && Remaining(request, State.LastUtcSeconds) == 0) {
                Finish(request, WithdrawalRequestStatus.SimulatedCompleted); changed = true;
            }
            return changed;
        }

        private void Finish(WithdrawalRequest request, WithdrawalRequestStatus status)
        {
            if (status != WithdrawalRequestStatus.SimulatedCompleted) {
                if (request.Terms.Currency == WithdrawalCurrency.Cash) {
                    long refund = checked(State.CashCents + State.FrozenCashCents); WithdrawalRules.CheckUnits(refund); State.CashCents = refund;
                } else {
                    long refund = checked(State.DiamondUnits + State.FrozenDiamondUnits); WithdrawalRules.CheckUnits(refund); State.DiamondUnits = refund;
                }
            }
            State.FrozenCashCents = 0; State.FrozenDiamondUnits = 0;
            request.Status = status; request.ResolvedUtcSeconds = State.LastUtcSeconds;
        }

        private long Current(WithdrawalTierRule tier, WithdrawalConditionKind kind, bool reserved)
        {
            switch (kind) {
                case WithdrawalConditionKind.PassLevel:
                case WithdrawalConditionKind.PassHideLevel:
                case WithdrawalConditionKind.PassStage2Level: return State.HighestClearedLevel;
                case WithdrawalConditionKind.GoldTarget:
                    return tier.Currency == WithdrawalCurrency.Cash ? State.CashCents + (reserved ? State.FrozenCashCents : 0) : State.DiamondUnits + (reserved ? State.FrozenDiamondUnits : 0);
                case WithdrawalConditionKind.LoginPassLevel: return State.TodayWins;
                case WithdrawalConditionKind.LoginDay: return State.QualifiedCheckInDays;
                case WithdrawalConditionKind.UserLevel: return UserLevel;
                default: throw new InvalidOperationException("Invalid simulation condition.");
            }
        }

        private static long Target(WithdrawalTierRule tier, WithdrawalConditionKind kind)
        {
            switch (kind) {
                case WithdrawalConditionKind.PassLevel: return tier.RequiredPassLevels;
                case WithdrawalConditionKind.PassHideLevel: return tier.RequiredHiddenPassLevels;
                case WithdrawalConditionKind.PassStage2Level: return tier.RequiredStage2PassLevels;
                case WithdrawalConditionKind.GoldTarget: return tier.RequiredBalanceUnits;
                case WithdrawalConditionKind.LoginPassLevel: return tier.RequiredDailyWins;
                case WithdrawalConditionKind.LoginDay: return tier.RequiredCheckInDays;
                case WithdrawalConditionKind.UserLevel: return tier.RequiredUserLevel;
                default: throw new InvalidOperationException("Invalid simulation condition.");
            }
        }

        private void AddReward(string runId, WithdrawalRewardKind kind, long cash, long diamonds)
        {
            WithdrawalRules.CheckUnits(cash); WithdrawalRules.CheckUnits(diamonds);
            if (cash == 0 && diamonds == 0) return;
            if (kind == WithdrawalRewardKind.Triple) {
                for (int i = State.PendingRewards.Count - 1; i >= 0; i--) {
                    WithdrawalReward pending = State.PendingRewards[i];
                    if (pending.RunId != runId || pending.Kind != kind) continue;
                    long nextCash = checked(pending.CashCents + cash), nextDiamonds = checked(pending.DiamondUnits + diamonds);
                    WithdrawalRules.CheckUnits(nextCash); WithdrawalRules.CheckUnits(nextDiamonds);
                    pending.CashCents = nextCash; pending.DiamondUnits = nextDiamonds;
                    return;
                }
            }
            State.PendingRewards.Add(new WithdrawalReward { RewardId = "reward-" + State.NextRewardNumber.ToString(CultureInfo.InvariantCulture),
                RunId = runId, Kind = kind, CashCents = cash, DiamondUnits = diamonds, CreatedUtcSeconds = State.LastUtcSeconds });
            State.NextRewardNumber = checked(State.NextRewardNumber + 1);
        }

        private bool TryActiveRun(string runId, out WithdrawalRun run)
        {
            run = null;
            if (string.IsNullOrEmpty(runId) || State.ActiveRunId != runId || !runs.TryGetValue(runId, out run))
                return Fail("A durable current run is required before recording rewards.");
            return true;
        }
        private WithdrawalChannelRule FindChannel(string channelId)
        {
            for (int i = 0; i < Rules.Channels.Length; i++) if (Rules.Channels[i].Id == channelId) return Rules.Channels[i];
            return null;
        }
        private static long Remaining(WithdrawalRequest request, long utcSeconds)
        {
            long elapsed = Math.Max(0, utcSeconds - request.ReviewStartedUtcSeconds);
            return Math.Max(0, request.Terms.ReviewSeconds - elapsed);
        }
        private bool Fail(string error) { LastError = error; return false; }
    }
}
