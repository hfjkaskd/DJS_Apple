using System;
using System.Collections.Generic;

namespace FruitApkEarning
{
    [Serializable]
    public sealed class ApkCashBand
    {
        public long MinimumCents, MaximumCents, RewardCents;
        public ApkCashBand() { }
        public ApkCashBand(long minimum, long maximum, long reward)
        { MinimumCents = minimum; MaximumCents = maximum; RewardCents = reward; }
    }

    [Serializable]
    public sealed class ApkDiamondBand
    {
        public long Balance, Minimum, Maximum;
        public ApkDiamondBand() { }
        public ApkDiamondBand(long balance, long minimum, long maximum)
        { Balance = balance; Minimum = minimum; Maximum = maximum; }
    }

    [Serializable]
    public sealed class ApkEarningRules
    {
        // All diamond values use hundredths, including balances and thresholds.
        public const long DiamondScale = 100;
        public long EarlyFirstCents = 40, EarlySecondCents = 60, TutorialBalanceCents = 50000;
        public int CashMatchFirstLevel = 3, DiamondFirstLevel = 4;
        public long MatchCashCents = 10, MatchDiamonds = 100;
        public int LuckyMatchCount = 4;
        public long LuckyIntervalSeconds = 30;
        public double CashRandomMinimum = 0.8, CashRandomMaximum = 1.2;
        public int BonusMultiplier = 10;
        // The source uses real board indices 22/51; a fruit board is one index.
        public int FirstCashGate = 22, SecondCashGate = 51, DailyWins = 10, CashLoginDays = 5, CashAccountLevel = 31;
        public long[] DiamondTargets = { 5000000, 10000000, 15000000, 20000000 };
        public long[] DiamondPayoutCents = { 50000, 100000, 150000, 200000 };
        public int[] DiamondLoginDays = { 5, 7, 10, 15 };
        public int[] DiamondAccountLevels = { 31, 32, 33, 34 };
        public ApkCashBand[] CashBands;
        public ApkDiamondBand[] DiamondBands;

        public static ApkEarningRules CreateDefault()
        {
            return new ApkEarningRules {
                CashBands = new[] {
                    new ApkCashBand(0,5000,150), new ApkCashBand(5000,10000,120),
                    new ApkCashBand(10000,13000,100), new ApkCashBand(13000,16000,80),
                    new ApkCashBand(16000,19000,60), new ApkCashBand(19000,21500,40),
                    new ApkCashBand(21500,23000,20), new ApkCashBand(23000,24500,10),
                    new ApkCashBand(24500,26000,8), new ApkCashBand(26000,27000,6),
                    new ApkCashBand(27000,28000,4), new ApkCashBand(28000,29000,2),
                    new ApkCashBand(29000,29999,1) },
                DiamondBands = new[] {
                    new ApkDiamondBand(0,8000,12000), new ApkDiamondBand(3200000,6400,9600),
                    new ApkDiamondBand(3400000,4800,7200), new ApkDiamondBand(3600000,3200,4800),
                    new ApkDiamondBand(3800000,2400,3600), new ApkDiamondBand(4000000,1600,2400),
                    new ApkDiamondBand(4200000,800,1200), new ApkDiamondBand(4300000,640,960),
                    new ApkDiamondBand(4400000,480,720), new ApkDiamondBand(4500000,320,480),
                    new ApkDiamondBand(4600000,160,240), new ApkDiamondBand(4700000,120,180),
                    new ApkDiamondBand(4800000,80,120) }
            };
        }

        public void Validate()
        {
            if (EarlyFirstCents <= 0 || EarlySecondCents <= 0 || TutorialBalanceCents < 0 ||
                MatchCashCents < 0 || MatchDiamonds < 0 || CashMatchFirstLevel < 1 || DiamondFirstLevel < 1 ||
                LuckyMatchCount < 1 || LuckyIntervalSeconds < 1 || CashRandomMinimum <= 0 ||
                CashRandomMaximum < CashRandomMinimum || double.IsNaN(CashRandomMinimum) || double.IsNaN(CashRandomMaximum) ||
                double.IsInfinity(CashRandomMaximum) || BonusMultiplier < 1 || DailyWins < 1 ||
                FirstCashGate < 1 || SecondCashGate < FirstCashGate || CashLoginDays < 1 || CashAccountLevel < 1 ||
                CashBands == null || DiamondBands == null || DiamondBands.Length == 0 ||
                DiamondTargets == null || DiamondTargets.Length == 0 || DiamondPayoutCents == null ||
                DiamondLoginDays == null || DiamondAccountLevels == null ||
                DiamondTargets.Length != DiamondPayoutCents.Length || DiamondTargets.Length != DiamondLoginDays.Length ||
                DiamondTargets.Length != DiamondAccountLevels.Length)
                throw new InvalidOperationException("Invalid APK earning configuration.");
            long previous = -1;
            foreach (var band in CashBands) {
                if (band == null || band.MinimumCents < 0 || band.MaximumCents <= band.MinimumCents ||
                    band.MinimumCents < previous || band.RewardCents <= 0) throw new InvalidOperationException("Invalid cash band.");
                previous = band.MaximumCents;
            }
            previous = -1;
            foreach (var band in DiamondBands) {
                if (band == null || band.Balance <= previous || band.Minimum < 0 || band.Maximum < band.Minimum)
                    throw new InvalidOperationException("Invalid diamond band.");
                previous = band.Balance;
            }
            for (int i = 0; i < DiamondTargets.Length; i++)
                if (DiamondTargets[i] <= 0 || DiamondPayoutCents[i] <= 0 || DiamondLoginDays[i] < 1 || DiamondAccountLevels[i] < 1)
                    throw new InvalidOperationException("Invalid diamond withdrawal tier.");
        }
    }

    [Serializable]
    public sealed class ApkReward
    {
        public string Id, Kind;
        public long CashCents, Diamonds, CreatedUtc;
        public int Level, EarlyOrdinal;
    }

    [Serializable]
    public sealed class ApkRecord
    {
        public int Ordinal;
        public long AmountCents, CreatedUtc, SubmittedUtc;
        public bool Archived, Pending;
    }

    [Serializable]
    public sealed class ApkRunCheckpoint
    {
        public string Id;
        public int Level, LastSequence;
        public bool Won;
    }

    [Serializable]
    public sealed class ApkEarningState
    {
        public int Version = 3;
        public long CashCents, Diamonds;
        public int CurrentLevel = 1, HighestClearedLevel, TodayWins, LoginDays, AccountLevel;
        public bool Activated, ActivationPending, DiamondGuideShown, WelcomeShown;
        // Native IsGuideGold defaults false; true is an explicit account guide request.
        public bool CashGuidanceComplete = true;
        public int ActivationStep, LuckyMatches, RewardVideos;
        public bool ActivationBalanceApplied;
        // MARK stays unknown until an actual configured account target is supplied.
        public long CashTargetCents;
        public long LastUtcDay = -1, LastLuckyUtc, NextRewardSequence;
        // Local civil-date ordinal; UTC timestamps above remain unchanged.
        // -1 with a known LastUtcDay identifies a pre-local-calendar save.
        public long LastLocalDay = -1;
        public List<ApkReward> PendingRewards = new List<ApkReward>();
        public List<ApkRecord> Records = new List<ApkRecord>();
        public List<ApkRunCheckpoint> Runs = new List<ApkRunCheckpoint>();
    }

    public sealed class ApkRequirement
    {
        public string Key;
        public long Current, Target;
        public bool Known = true, Waived;
        public bool Complete { get { return Known && (Waived || Current >= Target); } }
    }

    public sealed class ApkRequirements
    {
        public readonly List<ApkRequirement> Items = new List<ApkRequirement>(7);
        public bool Eligible { get { foreach (var item in Items) if (!item.Complete) return false; return true; } }
        public string FirstIncomplete { get { foreach (var item in Items) if (!item.Complete) return item.Key; return "ready"; } }
    }

    // Pure local accounting. Persistence must commit the entire resulting state atomically.
    // No method grants a real payment, invents an account rank, or imposes a fake review timer.
    public sealed class ApkEarningEngine
    {
        public ApkEarningRules Rules { get; private set; }
        public ApkEarningState State { get; private set; }
        private readonly Func<double> random;
        private readonly Func<long, long> localDay;
        private readonly Dictionary<string, ApkRunCheckpoint> runs = new Dictionary<string, ApkRunCheckpoint>(StringComparer.Ordinal);

        public ApkEarningEngine(ApkEarningRules rules, ApkEarningState state, Func<double> random = null, Func<long, long> localDay = null)
        {
            if (rules == null || state == null) throw new ArgumentNullException();
            rules.Validate(); Rules = rules; State = state;
            var generator = new Random(); this.random = random ?? generator.NextDouble;
            this.localDay = localDay ?? LocalCalendarDay;
            if (state.CashCents < 0 || state.Diamonds < 0 || state.CashTargetCents < 0 || state.CurrentLevel < 1 ||
                state.HighestClearedLevel < 0 || state.TodayWins < 0 || state.LoginDays < 0 || state.AccountLevel < 0 ||
                state.LuckyMatches < 0 || state.NextRewardSequence < 0 || state.LastUtcDay < -1 || state.LastLocalDay < -1 ||
                state.LastLuckyUtc < 0 || state.ActivationStep < 0 || state.ActivationStep > 6 || state.RewardVideos < 0 || state.Runs == null ||
                state.PendingRewards == null || state.Records == null) throw new InvalidOperationException("Invalid earning state.");
            foreach (var run in state.Runs) {
                if (run == null || string.IsNullOrEmpty(run.Id) || run.Level < 1 || run.LastSequence < 0)
                    throw new InvalidOperationException("Invalid run checkpoint.");
                runs.Add(run.Id, run);
            }
            var rewardIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var reward in state.PendingRewards) {
                if (reward == null || string.IsNullOrEmpty(reward.Id) || !rewardIds.Add(reward.Id) ||
                    (reward.Kind != "level" && reward.Kind != "lucky") || reward.Level < 1 ||
                    reward.CashCents < 0 || reward.Diamonds < 0 || (reward.CashCents == 0 && reward.Diamonds == 0) ||
                    (reward.EarlyOrdinal > 0 && reward.CashCents == 0) || reward.EarlyOrdinal < 0 || reward.EarlyOrdinal > 2 ||
                    reward.CreatedUtc <= 0) throw new InvalidOperationException("Invalid pending reward.");
            }
            var ordinals = new HashSet<int>();
            foreach (var record in state.Records) {
                if (record == null || record.Ordinal < 1 || record.Ordinal > 2 || !ordinals.Add(record.Ordinal) ||
                    record.AmountCents <= 0 || record.CreatedUtc < 0 || record.SubmittedUtc < 0 ||
                    (record.Pending && (!record.Archived || record.SubmittedUtc <= 0)))
                    throw new InvalidOperationException("Invalid early reward history.");
            }
        }

        public bool Tick(long utc)
        {
            RequireTime(utc);
            bool changed = false;
            long day = localDay(utc);
            if (day < 0) throw new InvalidOperationException("Invalid local calendar day.");
            if (State.LastLocalDay < 0) {
                // Old files counted UTC dates, without recording their time zone.
                // Establish today's local baseline once, retaining their counters;
                // guessing a conversion can otherwise award the same login twice.
                State.LastLocalDay = day; State.LoginDays = Math.Max(1, State.LoginDays); changed = true;
            }
            else if (day > State.LastLocalDay) {
                State.LoginDays = checked(State.LoginDays + 1); State.TodayWins = 0; State.LastLocalDay = day; changed = true;
            }
            if (changed) State.LastUtcDay = Math.Max(State.LastUtcDay, utc / 86400);
            if (State.LastLuckyUtc <= 0) { State.LastLuckyUtc = utc; changed = true; }
            return changed;
        }

        public bool NeedsTick(long utc)
        { return State.LastLocalDay < 0 || localDay(utc) > State.LastLocalDay || State.LastLuckyUtc <= 0; }

        public static long LocalCalendarDay(long utc)
        { return DateTimeOffset.FromUnixTimeSeconds(utc).ToLocalTime().Date.Ticks / TimeSpan.TicksPerDay; }

        public bool BeginRun(string id, int initialSequence, int level, long utc)
        {
            RequireRun(id, initialSequence, level); RequireTime(utc);
            ApkRunCheckpoint existing;
            if (runs.TryGetValue(id, out existing)) {
                if (existing.Level != level) throw new InvalidOperationException("A run cannot change level.");
                return Tick(utc);
            }
            Tick(utc);
            var checkpoint = new ApkRunCheckpoint { Id = id, Level = level, LastSequence = initialSequence };
            runs.Add(id, checkpoint); State.Runs.Add(checkpoint);
            State.CurrentLevel = Math.Max(State.CurrentLevel, level);
            return true;
        }

        public bool RecordMatch(string id, int sequence, int level, long utc, bool last = false)
        {
            RequireRun(id, sequence, level); RequireTime(utc);
            ApkRunCheckpoint checkpoint;
            if (!runs.TryGetValue(id, out checkpoint) || checkpoint.Level != level || checkpoint.Won || sequence <= checkpoint.LastSequence) return false;
            // One callback represents one actual triple, never an inferred historical gap.
            long cash = checked(State.CashCents + (level >= Rules.CashMatchFirstLevel ? Rules.MatchCashCents : 0));
            long diamonds = checked(State.Diamonds + (level >= Rules.DiamondFirstLevel ? Rules.MatchDiamonds : 0));
            int luckyCount = checked(State.LuckyMatches + 1);
            Tick(utc);
            State.CashCents = cash; State.Diamonds = diamonds; State.LuckyMatches = luckyCount; checkpoint.LastSequence = sequence;
            if (!last && level >= Rules.CashMatchFirstLevel && State.PendingRewards.Count == 0 &&
                (luckyCount >= Rules.LuckyMatchCount || utc - State.LastLuckyUtc >= Rules.LuckyIntervalSeconds)) {
                long amount = CalculateCash();
                if (amount <= 0) throw new InvalidOperationException("Lucky reward must have a positive amount.");
                long serial = checked(State.NextRewardSequence + 1);
                State.PendingRewards.Add(new ApkReward { Id = "lucky-" + serial.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Kind = "lucky", CashCents = amount, Level = level, CreatedUtc = utc });
                State.NextRewardSequence = serial; State.LuckyMatches = 0; State.LastLuckyUtc = utc;
            }
            return true;
        }

        public bool RecordWin(string id, int level, long utc)
        {
            RequireRun(id, 0, level); RequireTime(utc);
            ApkRunCheckpoint checkpoint;
            if (!runs.TryGetValue(id, out checkpoint) || checkpoint.Level != level || checkpoint.Won) return false;
            int next = checked(level + 1);
            long cash = level == 1 ? Rules.EarlyFirstCents : level == 2 ? Rules.EarlySecondCents : CalculateCash();
            long diamonds = level >= Rules.DiamondFirstLevel ? CalculateDiamonds() : 0;
            if (cash == 0 && diamonds == 0) throw new InvalidOperationException("A settlement must contain an earned reward.");
            Tick(utc);
            // Tick may reset the old day's count before this clear is recorded.
            int daily = checked(State.TodayWins + 1);
            State.PendingRewards.Add(new ApkReward { Id = id + ":level", Kind = "level", CashCents = cash,
                Diamonds = diamonds, Level = level, EarlyOrdinal = level <= 2 && FindRecord(level) == null ? level : 0, CreatedUtc = utc });
            checkpoint.Won = true; State.TodayWins = daily;
            State.CurrentLevel = Math.Max(State.CurrentLevel, next); State.HighestClearedLevel = Math.Max(State.HighestClearedLevel, level);
            return true;
        }

        public bool Claim(string id, long utc)
        {
            RequireTime(utc);
            int index = RewardIndex(id); if (index < 0) return false;
            ClaimAt(index, utc);
            return true;
        }

        public bool ClaimWithTestVideo(string id, bool completed, long utc)
        {
            RequireTime(utc);
            if (!completed) return false;
            int index = RewardIndex(id);
            if (index < 0) return false;
            ApkReward reward = State.PendingRewards[index];
            if (reward.EarlyOrdinal != 0 || reward.Level <= 2 ||
                (reward.Kind != "level" && reward.Kind != "lucky")) return false;
            ClaimAt(index, utc, Rules.BonusMultiplier, true);
            return true;
        }

        /// <summary>
        /// Completes already-earned rewards from a previous session without
        /// reopening their presentation. The caller saves this candidate before
        /// publishing it; this never creates a reward or changes a run's Won flag.
        /// </summary>
        public bool ResolveRestoredRewards(long utc)
        {
            RequireTime(utc);
            bool changed = State.PendingRewards.Count > 0;
            // Remove from the end so recovering a long saved queue stays linear.
            for (int i = State.PendingRewards.Count - 1; i >= 0; i--) ClaimAt(i, utc);
            return changed;
        }

        private void ClaimAt(int index, long utc, int multiplier = 1, bool testVideo = false)
        {
            var reward = State.PendingRewards[index];
            long cash = checked(State.CashCents + checked(reward.CashCents * multiplier));
            long diamonds = checked(State.Diamonds + checked(reward.Diamonds * multiplier));
            int videos = testVideo ? checked(State.RewardVideos + 1) : State.RewardVideos;
            if (reward.EarlyOrdinal > 0 && FindRecord(reward.EarlyOrdinal) != null)
                throw new InvalidOperationException("Early reward was already claimed.");
            Tick(utc); State.CashCents = cash; State.Diamonds = diamonds; State.RewardVideos = videos;
            if (reward.EarlyOrdinal > 0) State.Records.Add(new ApkRecord { Ordinal = reward.EarlyOrdinal,
                AmountCents = reward.CashCents, CreatedUtc = utc });
            State.PendingRewards.RemoveAt(index);
        }

        public bool ArchiveEarlyReward(int ordinal, long utc)
        {
            RequireOrdinal(ordinal); RequireTime(utc);
            var record = FindRecord(ordinal);
            if (record == null || record.Archived || State.CashCents < record.AmountCents) return false;
            State.CashCents = checked(State.CashCents - record.AmountCents); record.Archived = true;
            var first = FindRecord(1); var second = FindRecord(2);
            State.ActivationPending = !State.Activated && State.CurrentLevel >= 3 && first != null && second != null && first.Archived && second.Archived;
            return true;
        }

        public bool SubmitEarlyRecord(int ordinal, long utc)
        {
            RequireOrdinal(ordinal); RequireTime(utc);
            var record = FindRecord(ordinal);
            if (record == null || !record.Archived || record.Pending) return false;
            record.Pending = true; record.SubmittedUtc = utc; return true;
        }

        public bool SetActivationStep(int completedStep, long utc)
        {
            RequireTime(utc);
            if (completedStep < 0 || completedStep > 6) return false;
            if (!CanActivate() || completedStep != State.ActivationStep + 1) return false;
            // The balance receipt must finish its saved collection before advancing.
            if (completedStep == 4 && !State.ActivationBalanceApplied) return false;
            State.ActivationStep = completedStep; State.ActivationPending = true;
            return true;
        }

        public bool ApplyActivationBalance(long utc)
        {
            RequireTime(utc);
            if (!CanActivate() || State.ActivationStep != 3 || State.ActivationBalanceApplied) return false;
            // The US APK response sets an absolute 500 balance only at this boundary.
            if (State.CurrentLevel == 3) State.CashCents = Math.Max(State.CashCents, Rules.TutorialBalanceCents);
            State.ActivationBalanceApplied = true;
            return true;
        }

        public bool CompleteActivation(long utc)
        {
            RequireTime(utc);
            if (!CanActivate() || State.ActivationStep != 6 || !State.ActivationBalanceApplied) return false;
            State.Activated = true; State.ActivationPending = false;
            return true;
        }

        private bool CanActivate()
        {
            var first = FindRecord(1); var second = FindRecord(2);
            return !State.Activated && State.CurrentLevel >= 3 && first != null && second != null && first.Archived && second.Archived;
        }

        public ApkRequirements GetRequirements(bool diamonds, int tier)
        {
            if (diamonds && (tier < 0 || tier >= Rules.DiamondTargets.Length)) throw new ArgumentOutOfRangeException(nameof(tier));
            var result = new ApkRequirements();
            if (!diamonds) {
                Add(result, "firstLevel", State.HighestClearedLevel, Rules.FirstCashGate);
                Add(result, "secondLevel", State.HighestClearedLevel, Rules.SecondCashGate);
            }
            Add(result, "balance", diamonds ? State.Diamonds : State.CashCents,
                diamonds ? Rules.DiamondTargets[tier] : State.CashTargetCents, diamonds || State.CashTargetCents > 0);
            if (!diamonds) Add(result, "guide", State.CashGuidanceComplete ? 1 : 0, 1);
            int days = diamonds ? Rules.DiamondLoginDays[tier] : Rules.CashLoginDays;
            Add(result, "daily", State.TodayWins, Rules.DailyWins, true, State.LoginDays >= days);
            Add(result, "login", State.LoginDays, days);
            Add(result, "rank", State.AccountLevel, diamonds ? Rules.DiamondAccountLevels[tier] : Rules.CashAccountLevel);
            return result;
        }

        public int GetCashStage()
        {
            if (State.CurrentLevel < 3) return 1;
            if (State.HighestClearedLevel < Rules.FirstCashGate) return 2;
            if (State.HighestClearedLevel < Rules.SecondCashGate) return 3;
            if (State.CashTargetCents <= 0) return 0;
            if (State.CashCents < State.CashTargetCents || !State.CashGuidanceComplete) return 4;
            if (State.LoginDays >= Rules.CashLoginDays) return 7;
            return State.TodayWins >= Rules.DailyWins ? 6 : 5;
        }

        private long CalculateCash()
        {
            long amount = Rules.MatchCashCents;
            foreach (var band in Rules.CashBands)
                if (State.CashCents >= band.MinimumCents && State.CashCents < band.MaximumCents) { amount = band.RewardCents; break; }
            return Rounded(amount * (Rules.CashRandomMinimum + NextRandom() * (Rules.CashRandomMaximum - Rules.CashRandomMinimum)));
        }
        private long CalculateDiamonds()
        {
            var chosen = Rules.DiamondBands[0];
            foreach (var band in Rules.DiamondBands) { if (State.Diamonds < band.Balance) break; chosen = band; }
            return Rounded(chosen.Minimum + NextRandom() * (chosen.Maximum - chosen.Minimum));
        }
        private double NextRandom()
        {
            double value = random();
            if (double.IsNaN(value) || value < 0 || value > 1) throw new InvalidOperationException("Random value must be in [0,1].");
            return value;
        }
        private static long Rounded(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value >= long.MaxValue) throw new OverflowException();
            return checked((long)Math.Round(value, MidpointRounding.AwayFromZero));
        }
        private ApkRecord FindRecord(int ordinal)
        { foreach (var record in State.Records) if (record.Ordinal == ordinal) return record; return null; }
        private int RewardIndex(string id)
        { for (int i = 0; i < State.PendingRewards.Count; i++) if (string.Equals(State.PendingRewards[i].Id, id, StringComparison.Ordinal)) return i; return -1; }
        private static void Add(ApkRequirements requirements, string key, long current, long target, bool known = true, bool waived = false)
        { requirements.Items.Add(new ApkRequirement { Key = key, Current = current, Target = target, Known = known, Waived = waived }); }
        private static void RequireTime(long utc) { if (utc <= 0) throw new ArgumentOutOfRangeException(nameof(utc)); }
        private static void RequireOrdinal(int ordinal) { if (ordinal < 1 || ordinal > 2) throw new ArgumentOutOfRangeException(nameof(ordinal)); }
        private static void RequireRun(string id, int sequence, int level)
        { if (string.IsNullOrEmpty(id) || id.Length > 160 || sequence < 0 || level < 1) throw new ArgumentException("Invalid gameplay event."); }
    }
}
