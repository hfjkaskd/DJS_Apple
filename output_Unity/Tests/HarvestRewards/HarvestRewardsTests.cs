using System;
using System.IO;

// Compiled against the actual pure domain sources. No Unity runtime, reflection,
// mocked production engine, third-party test runner or network restore is used.
internal static class HarvestRewardsTests
{
    private const long Epoch = 1800000000;
    private const long Day = 86400;
    private static int passed;
    private static int failed;
    private static int runSequence;
    private static string fixtureRoot;

    private static int Main(string[] args)
    {
        fixtureRoot = args.Length > 0 ? args[0] : Path.GetTempPath();
        Run("Default disclosed rules retain six stages and thirteen days", DefaultRules);
        Run("Six concurrent task/timer stages then exactly 168 hours", FullThirteenDayFlow);
        Run("Expired timer alone cannot advance or backdate future stages", TimerWithoutTasks);
        Run("Offline return retains elapsed next-stage and final settlement waits", OfflineCompletedTaskTimers);
        Run("Wrong task type and excess work do not prefill future stages", StageBoundaries);
        Run("Application freezes once and new rewards remain available", FrozenSingleApplication);
        Run("Insufficient and invalid tiers cannot start an application", InvalidApplications);
        Run("Repeated triple and win events cannot pay twice", DuplicateGameplayEvents);
        Run("Tutorial gift and first free win are claimed only once", TutorialAndFirstWin);
        Run("In-game first free reward and three-second base delay", InGameBaseRewards);
        Run("Last triple reserves the win flow and suppresses in-game popup", LastTripleWinPriority);
        Run("Failed rewarded ads preserve base and completed callbacks pay once", RewardedClaims);
        Run("Win probability interpolation preserves combined high-group chance", WinProbabilities);
        Run("First target slowdown survives freeze and engine restart", PermanentSlowdown);
        Run("UTC rollback cannot reduce the persisted high-water time", ClockRollback);
        Run("Save/reload preserves application, reward offer and triple deduplication", PersistedSnapshot);
        Run("Restarted ad request cannot replay a completion", InterruptedAdRestart);
        Run("Corrupt or truncated saves fail without resetting or loading stale backup", CorruptStorage);
        Run("Failed atomic replacement leaves the prior primary save valid", AtomicSaveFailure);
        Run("Invalid configuration is rejected", InvalidConfiguration);
        Console.WriteLine("Harvest rewards regression: " + passed + " passed, " + failed + " failed.");
        return failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            passed++;
            Console.WriteLine("PASS " + name);
        }
        catch (Exception exception)
        {
            failed++;
            Console.Error.WriteLine("FAIL " + name + ": " + exception.Message);
            Console.Error.WriteLine(exception.StackTrace);
        }
    }

    private static HarvestRewardEngine Funded(long cents = 100000)
    {
        return new HarvestRewardEngine(new HarvestRewardSettings(), new HarvestRewardState
        {
            AvailableCents = cents,
            LifetimeEarnedCents = cents,
            ReachedFirstTarget = cents >= 100000
        }, () => 0.0);
    }

    private static string NewRun(HarvestRewardEngine engine, long now)
    {
        string runId = "regression-run-" + (++runSequence);
        engine.BeginRun(runId, now);
        return runId;
    }

    private static void Wins(HarvestRewardEngine engine, int count, long now, bool tutorial = false)
    {
        for (int i = 0; i < count; i++)
            engine.RecordWin(NewRun(engine, now), tutorial, now);
    }

    private static void Triples(HarvestRewardEngine engine, int count, long now, bool tutorial = false)
    {
        string runId = NewRun(engine, now);
        for (int i = 1; i <= count; i++) engine.RecordTriple(runId, i, tutorial, now);
    }

    private static void CompleteCurrentTask(HarvestRewardEngine engine, long now)
    {
        HarvestStageSettings stage = engine.Config.Stages[engine.State.ActiveRequest.StageIndex];
        if (stage.TaskKind == HarvestStageSettings.Wins) Wins(engine, stage.Target, now);
        else Triples(engine, stage.Target, now);
    }

    private static void DefaultRules()
    {
        var settings = new HarvestRewardSettings();
        settings.Validate();
        Equal(6, settings.Stages.Length, "stage count");
        int[] targets = { 10, 260, 10, 300, 10, 500 };
        long total = settings.SettlementWaitSeconds;
        Equal(168 * 3600, settings.SettlementWaitSeconds, "final wait");
        for (int i = 0; i < settings.Stages.Length; i++)
        {
            Equal(targets[i], settings.Stages[i].Target, "stage target " + i);
            Equal(Day, settings.Stages[i].WaitSeconds, "stage wait " + i);
            total += settings.Stages[i].WaitSeconds;
        }
        Equal(13 * Day, total, "minimum elapsed time");
    }

    private static void FullThirteenDayFlow()
    {
        var engine = Funded();
        engine.TryApply(0, Epoch);
        string requestId = engine.State.ActiveRequest.RequestId;
        for (int i = 0; i < 6; i++)
        {
            long stageStart = Epoch + i * Day;
            Equal(i, engine.State.ActiveRequest.StageIndex, "active stage");
            Equal(stageStart, engine.State.ActiveRequest.StageStartedUtcSeconds, "stage start");
            CompleteCurrentTask(engine, stageStart);
            engine.Tick(stageStart + Day - 1);
            Equal(i, engine.State.ActiveRequest.StageIndex, "task completion cannot skip timer");
            Equal(HarvestRequestState.StageActive, engine.State.ActiveRequest.Status, "still in stage");
            engine.Tick(stageStart + Day);
            if (i < 5)
            {
                Equal(i + 1, engine.State.ActiveRequest.StageIndex, "advance one stage");
                Equal(0, engine.State.ActiveRequest.StageProgress, "next task begins empty");
            }
        }
        Equal(HarvestRequestState.SettlementWait, engine.State.ActiveRequest.Status, "final wait status");
        Equal(Epoch + 6 * Day, engine.State.ActiveRequest.SettlementStartedUtcSeconds, "no extra day before final wait");
        engine.Tick(Epoch + 13 * Day - 1);
        NotNull(engine.State.ActiveRequest, "cannot settle a second early");
        Equal(100000L, engine.State.FrozenCents, "funds remain frozen until completion");
        engine.Tick(Epoch + 13 * Day);
        IsNull(engine.State.ActiveRequest, "completed request released");
        Equal(0L, engine.State.FrozenCents, "frozen amount settled");
        Equal(1, engine.State.Settlements.Count, "one completion record");
        Equal(requestId, engine.State.Settlements[0].RequestId, "same request completes");
        Equal(100000L, engine.State.Settlements[0].AmountCents, "settled original amount");
        Equal(Epoch + 13 * Day, engine.State.Settlements[0].CompletedUtcSeconds, "completion UTC");
        Equal(0, engine.State.CompletedTierIndexes[0], "completed tier");
        engine.Tick(Epoch + 100 * Day);
        engine.TryApply(0, Epoch + 100 * Day);
        Equal(1, engine.State.Settlements.Count, "completion is idempotent");
        IsNull(engine.State.ActiveRequest, "same tier cannot repeat");
    }

    private static void TimerWithoutTasks()
    {
        var engine = Funded();
        engine.TryApply(0, Epoch);
        engine.Tick(Epoch + 100 * Day);
        Equal(0, engine.State.ActiveRequest.StageIndex, "offline days cannot complete work");
        Equal(0, engine.State.ActiveRequest.StageProgress, "offline days do not create work");
        Wins(engine, 10, Epoch + 100 * Day);
        Equal(1, engine.State.ActiveRequest.StageIndex, "late task completion advances current stage");
        Equal(Epoch + 100 * Day, engine.State.ActiveRequest.StageStartedUtcSeconds, "future stage starts now");
        engine.Tick(Epoch + 100 * Day);
        Equal(1, engine.State.ActiveRequest.StageIndex, "never fast-forward unopened stages");
    }

    private static void StageBoundaries()
    {
        var engine = Funded(500000);
        Wins(engine, 12, Epoch - 1);
        engine.TryApply(0, Epoch);
        Equal(0, engine.State.ActiveRequest.StageProgress, "pre-application wins excluded");
        Triples(engine, 20, Epoch);
        Equal(0, engine.State.ActiveRequest.StageProgress, "triples excluded from win stage");
        Wins(engine, 13, Epoch);
        Equal(10, engine.State.ActiveRequest.StageProgress, "progress caps at current target");
        engine.Tick(Epoch + Day);
        Equal(0, engine.State.ActiveRequest.StageProgress, "no overflow or earlier triples carried forward");
        Wins(engine, 3, Epoch + Day);
        Equal(0, engine.State.ActiveRequest.StageProgress, "wins excluded from triple stage");
        Triples(engine, 5, Epoch + Day);
        Equal(5, engine.State.ActiveRequest.StageProgress, "current stage triples count");
        Triples(engine, 5, Epoch + Day, true);
        Equal(5, engine.State.ActiveRequest.StageProgress, "tutorial triples excluded");
    }

    private static void OfflineCompletedTaskTimers()
    {
        var engine = Funded();
        engine.TryApply(0, Epoch);
        Wins(engine, 10, Epoch);
        string path = FixturePath("offline-stage");
        HarvestRewardStorage.Save(path, engine.State);
        engine = new HarvestRewardEngine(engine.Config, HarvestRewardStorage.Load(path), () => 0.0);
        engine.Tick(Epoch + 4 * Day);
        Equal(1, engine.State.ActiveRequest.StageIndex, "only stage with completed work advances offline");
        Equal(Epoch + Day, engine.State.ActiveRequest.StageStartedUtcSeconds, "next stage clock starts at previous completed stage due time");
        Equal(0, engine.State.ActiveRequest.StageProgress, "next stage still needs its own task");
        Equal(0L, engine.GetRemainingSeconds(), "already elapsed next stage waiting is retained");
        CompleteCurrentTask(engine, Epoch + 4 * Day);
        Equal(2, engine.State.ActiveRequest.StageIndex, "late next-stage task now completes");
        Equal(Epoch + 4 * Day, engine.State.ActiveRequest.StageStartedUtcSeconds, "late task completion starts following stage now");

        engine = Funded();
        engine.TryApply(0, Epoch);
        for (int i = 0; i < 5; i++)
        {
            CompleteCurrentTask(engine, Epoch + i * Day);
            engine.Tick(Epoch + (i + 1) * Day);
        }
        CompleteCurrentTask(engine, Epoch + 5 * Day);
        path = FixturePath("offline-final");
        HarvestRewardStorage.Save(path, engine.State);
        engine = new HarvestRewardEngine(engine.Config, HarvestRewardStorage.Load(path), () => 0.0);
        engine.Tick(Epoch + 14 * Day);
        IsNull(engine.State.ActiveRequest, "final completed task plus eight offline days settles on return");
        Equal(0L, engine.State.FrozenCents, "offline settlement releases frozen balance");
        Equal(1, engine.State.Settlements.Count, "offline settlement creates exactly one record");
        engine.Tick(Epoch + 15 * Day);
        Equal(1, engine.State.Settlements.Count, "offline completion is idempotent");
    }

    private static void FrozenSingleApplication()
    {
        var engine = Funded(1200000);
        engine.TryApply(0, Epoch);
        string requestId = engine.State.ActiveRequest.RequestId;
        Equal(1100000L, engine.State.AvailableCents, "deduct available once");
        Equal(100000L, engine.State.FrozenCents, "freeze requested amount");
        engine.TryApply(1, Epoch + 1);
        engine.TryApply(0, Epoch + 1);
        Equal(requestId, engine.State.ActiveRequest.RequestId, "second application rejected");
        Equal(1100000L, engine.State.AvailableCents, "failed apply does not debit");
        Triples(engine, 1, Epoch + 1);
        Equal(1100100L, engine.State.AvailableCents, "earnings enter available balance");
        Equal(100000L, engine.State.FrozenCents, "new earnings do not alter frozen amount");
    }

    private static void InvalidApplications()
    {
        var engine = Funded(99999);
        engine.TryApply(-1, Epoch);
        engine.TryApply(engine.Config.ThresholdCents.Length, Epoch);
        engine.TryApply(0, Epoch);
        IsNull(engine.State.ActiveRequest, "invalid and insufficient applications rejected");
        Equal(99999L, engine.State.AvailableCents, "available balance unchanged");
        Equal(0L, engine.State.FrozenCents, "no frozen amount");
    }

    private static void DuplicateGameplayEvents()
    {
        var engine = Funded();
        string runId = NewRun(engine, Epoch);
        engine.RecordTriple(runId, 1, false, Epoch);
        long credited = engine.State.AvailableCents;
        engine.RecordTriple(runId, 1, false, Epoch + 1);
        engine.RecordTriple(runId, 0, false, Epoch + 1);
        engine.RecordTriple("another-run", 2, false, Epoch + 1);
        Equal(credited, engine.State.AvailableCents, "same or stale event cannot pay twice");
        engine.TryApply(0, Epoch + 1);
        engine.RecordWin(runId, false, Epoch + 1);
        Equal(1, engine.State.ActiveRequest.StageProgress, "win counts once");
        string rewardId = engine.State.PendingWinReward.RewardId;
        engine.RecordWin(runId, false, Epoch + 1);
        Equal(1, engine.State.ActiveRequest.StageProgress, "duplicate win ignored");
        Equal(rewardId, engine.State.PendingWinReward.RewardId, "duplicate win cannot replace offer");
    }

    private static void PermanentSlowdown()
    {
        var engine = Funded(99999);
        engine.State.EffectivePlaySeconds = 300;
        string runId = NewRun(engine, Epoch);
        engine.RecordTriple(runId, 1, false, Epoch);
        True(engine.State.ReachedFirstTarget, "first earned crossing marked permanently");
        engine.TryApply(0, Epoch);
        True(engine.State.AvailableCents < 100000, "freeze drops available below first threshold");
        engine = new HarvestRewardEngine(engine.Config, engine.State, () => 0.0);
        long before = engine.State.AvailableCents;
        engine.RecordTriple(runId, 2, false, Epoch + 1);
        Equal(100L, engine.State.AvailableCents - before, "post-target rate remains one simulated unit");
        int milestoneMask = engine.State.MilestoneClaimMask;
        before = engine.State.AvailableCents;
        for (int i = 0; i < 400; i++) engine.AdvanceEffectivePlay(5, Epoch + 2 + i * 5);
        Equal(before, engine.State.AvailableCents, "unclaimed acceleration milestones never restart");
        Equal(milestoneMask, engine.State.MilestoneClaimMask, "no new acceleration milestone claims");
    }

    private static void TutorialAndFirstWin()
    {
        var engine = Funded(0);
        string runId = NewRun(engine, Epoch);
        engine.RecordTriple(runId, 1, true, Epoch);
        Equal(0L, engine.State.AvailableCents, "tutorial triple has no reward");
        engine.RecordWin(runId, true, Epoch);
        Equal(2000L, engine.State.AvailableCents, "tutorial gift");
        True(engine.State.TutorialGiftClaimed, "gift marked");
        True(engine.State.PendingWinReward.IsFirstFree, "first win is free");
        Equal(10000L, engine.State.PendingWinReward.DisplayCents, "first win multiplier ten");
        True(engine.ClaimWinBase(runId, Epoch), "first reward has no three-second delay");
        Equal(12000L, engine.State.AvailableCents, "gift plus first win");
        True(!engine.ClaimWinBase(runId, Epoch), "first reward cannot be claimed again");
        engine.RecordWin(runId, true, Epoch);
        Equal(12000L, engine.State.AvailableCents, "duplicate tutorial win pays nothing");
        runId = NewRun(engine, Epoch + 1);
        engine.RecordWin(runId, true, Epoch + 1);
        Equal(12000L, engine.State.AvailableCents, "new tutorial run does not repeat lifetime gift");
        True(!engine.State.PendingWinReward.IsFirstFree, "first multiplier remains consumed");
        True(!engine.ClaimWinBase(runId, Epoch + 3), "ordinary reward locked until three seconds");
        True(engine.ClaimWinBase(runId, Epoch + 4), "ordinary reward available after three seconds");
        Equal(13000L, engine.State.AvailableCents, "ordinary reward ten simulated units");
        Equal(0, engine.State.WinAdSuccessCount, "free/base rewards are not ads");
    }

    private static void InGameBaseRewards()
    {
        var engine = Funded();
        string runId = NewRun(engine, Epoch);
        for (int i = 1; i <= 17; i++) engine.RecordTriple(runId, i, false, Epoch);
        engine.AdvanceEffectivePlay(1, Epoch + 1);
        NotNull(engine.State.PendingInGameReward, "offer after immediate group threshold");
        True(engine.State.PendingInGameReward.IsFirstFree, "first in-game multiplier is free");
        long before = engine.State.AvailableCents;
        True(engine.ClaimInGameBase(runId, Epoch + 1), "first in-game claim is immediate");
        Equal(1000L, engine.State.AvailableCents - before, "first in-game multiplier two");
        True(!engine.ClaimInGameBase(runId, Epoch + 1), "in-game claim idempotent");
        for (int i = 18; i <= 34; i++) engine.RecordTriple(runId, i, false, Epoch + 2);
        engine.AdvanceEffectivePlay(1, Epoch + 2);
        NotNull(engine.State.PendingInGameReward, "next offer created");
        True(!engine.State.PendingInGameReward.IsFirstFree, "first in-game claim consumed");
        before = engine.State.AvailableCents;
        True(!engine.ClaimInGameBase(runId, Epoch + 4), "normal base blocked at two seconds");
        True(engine.ClaimInGameBase(runId, Epoch + 5), "normal base claim at three seconds");
        Equal(500L, engine.State.AvailableCents - before, "normal base five simulated units");
        Equal(0, engine.State.InGameAdSuccessCount, "base reward cannot upgrade ad tier");
    }

    private static HarvestRewardEngine QuickOffers()
    {
        var config = new HarvestRewardSettings
        {
            InGameOfferTriples = 1,
            InGameOfferImmediateTriples = 1,
            InGameOfferEffectiveSeconds = 0
        };
        return new HarvestRewardEngine(config, new HarvestRewardState
        {
            AvailableCents = 100000,
            LifetimeEarnedCents = 100000,
            ReachedFirstTarget = true,
            FirstInGameClaimed = true,
            FirstWinClaimed = true
        }, () => 0.0);
    }

    private static void LastTripleWinPriority()
    {
        var engine = QuickOffers();
        string runId = NewRun(engine, Epoch);
        engine.RecordTriple(runId, 1, false, Epoch, true);
        long before = engine.State.AvailableCents;
        engine.AdvanceEffectivePlay(1, Epoch + 1);
        IsNull(engine.State.PendingInGameReward, "last triple must not produce in-game popup");
        Equal(before, engine.State.AvailableCents, "waiting for win cannot gain effective-play rewards");
        string path = FixturePath("last-triple");
        HarvestRewardStorage.Save(path, engine.State);
        engine = new HarvestRewardEngine(engine.Config, HarvestRewardStorage.Load(path), () => 0.0);
        engine.AdvanceEffectivePlay(1, Epoch + 2);
        IsNull(engine.State.PendingInGameReward, "last-triple win reservation survives restart");
        True(engine.RecordWin(runId, false, Epoch + 2), "win can complete after last-triple reservation");
        NotNull(engine.State.PendingWinReward, "win offer takes priority");
        IsNull(engine.State.PendingInGameReward, "no late in-game offer during win");
    }

    private static string OpenQuickOffer(HarvestRewardEngine engine, long now)
    {
        string runId = NewRun(engine, now);
        engine.RecordTriple(runId, 1, false, now);
        engine.AdvanceEffectivePlay(1, now);
        NotNull(engine.State.PendingInGameReward, "quick offer created");
        return runId;
    }

    private static void RewardedClaims()
    {
        var engine = QuickOffers();
        string runId = OpenQuickOffer(engine, Epoch);
        string requestId;
        long reward;
        True(engine.TryBeginRewardedClaim(HarvestRewardOffer.InGame, out requestId, out reward), "start reward request");
        Equal(1000L, reward, "first actual ad bracket is two");
        long before = engine.State.AvailableCents;
        True(!engine.CompleteRewardedClaim("wrong-id", true, Epoch + 1), "unrelated callback rejected");
        Equal(before, engine.State.AvailableCents, "no credit from unrelated callback");
        True(!engine.ClaimInGameBase(runId, Epoch + 3), "base cannot race an active request");
        True(!engine.CompleteRewardedClaim(requestId, false, Epoch + 3), "failed/closed ad pays no ad reward");
        Equal(0, engine.State.InGameAdSuccessCount, "failure cannot increment success count");
        True(engine.ClaimInGameBase(runId, Epoch + 3), "base remains accessible after failure");
        Equal(500L, engine.State.AvailableCents - before, "only base paid after failure");
        int[] multipliers = { 2, 4, 4, 6, 6, 6, 8 };
        for (int i = 0; i < multipliers.Length; i++)
        {
            long now = Epoch + 10 + i * 10;
            OpenQuickOffer(engine, now);
            True(engine.TryBeginRewardedClaim(HarvestRewardOffer.InGame, out requestId, out reward), "begin ad " + i);
            Equal(multipliers[i] * 500L, reward, "previous success count chooses current reward");
            before = engine.State.AvailableCents;
            True(engine.CompleteRewardedClaim(requestId, true, now + 1), "complete ad " + i);
            Equal(reward, engine.State.AvailableCents - before, "advertised reward paid");
            True(!engine.CompleteRewardedClaim(requestId, true, now + 1), "duplicate callback rejected");
            Equal(reward, engine.State.AvailableCents - before, "duplicate callback pays nothing");
            Equal(i + 1, engine.State.InGameAdSuccessCount, "only successful ads increment once");
        }
    }

    private static void WinProbabilities()
    {
        var engine = Funded(0);
        Close(.820, engine.GetWinHighMultiplierProbability(), "zero balance probability");
        engine.State.AvailableCents = 25000;
        Close((.820 + .350) / 2, engine.GetWinHighMultiplierProbability(), "linear interpolation");
        engine.State.AvailableCents = 95000;
        Close(.060, engine.GetWinHighMultiplierProbability(), "near threshold probability");
        engine.State.WinAdSuccessCount = 6;
        Close(.803, engine.GetWinHighMultiplierProbability(), "high ad bracket probability");
        engine.State.AvailableCents = 100000;
        engine.TryApply(0, Epoch);
        Close(.997, engine.GetWinHighMultiplierProbability(), "frozen funds excluded from ratio");
        engine.State.FirstWinClaimed = true;
        string runId = NewRun(engine, Epoch);
        engine.RecordWin(runId, false, Epoch);
        string requestId;
        long reward;
        True(engine.TryBeginRewardedClaim(HarvestRewardOffer.Win, out requestId, out reward), "begin win ad");
        Equal(6000L, reward, "high group sample chooses six; high probability is not ten-only probability");
        True(engine.CompleteRewardedClaim(requestId, true, Epoch + 1), "win ad completed");
        Equal(7, engine.State.WinAdSuccessCount, "win success count incremented");
        Equal(0, engine.State.InGameAdSuccessCount, "win ads do not increment in-game count");
    }

    private static string FixturePath(string name)
    {
        return Path.Combine(fixtureRoot, name + "-" + Guid.NewGuid().ToString("N") + ".bin");
    }

    private static void PersistedSnapshot()
    {
        var engine = Funded(500000);
        engine.TryApply(0, Epoch);
        Wins(engine, 10, Epoch);
        engine.Tick(Epoch + Day);
        string runId = NewRun(engine, Epoch + Day);
        engine.RecordTriple(runId, 1, false, Epoch + Day);
        string path = FixturePath("snapshot");
        HarvestRewardStorage.Save(path, engine.State);
        var loaded = HarvestRewardStorage.Load(path);
        var restarted = new HarvestRewardEngine(engine.Config, loaded, () => 0.0);
        Equal(engine.State.AvailableCents, loaded.AvailableCents, "available balance restored");
        Equal(engine.State.FrozenCents, loaded.FrozenCents, "frozen balance restored");
        Equal(engine.State.LifetimeEarnedCents, loaded.LifetimeEarnedCents, "lifetime amount restored");
        Equal(engine.State.ActiveRequest.RequestId, loaded.ActiveRequest.RequestId, "same request restored");
        Equal(1, loaded.ActiveRequest.StageIndex, "stage restored");
        Equal(1, loaded.ActiveRequest.StageProgress, "task progress restored");
        Equal(Epoch + Day, loaded.ActiveRequest.StageStartedUtcSeconds, "timer start restored");
        Equal(runId, loaded.ActiveRun.RunId, "run ID restored");
        long before = loaded.AvailableCents;
        restarted.RecordTriple(runId, 1, false, Epoch + Day + 1);
        Equal(before, loaded.AvailableCents, "replayed snapshot triple cannot pay again");
        Equal(1, loaded.ActiveRequest.StageProgress, "replayed snapshot triple cannot count again");
        restarted.RecordTriple(runId, 2, false, Epoch + Day + 1);
        Equal(before + 100, loaded.AvailableCents, "new triple after restart paid");
        Equal(2, loaded.ActiveRequest.StageProgress, "new triple after restart counted");
        restarted.RecordWin(runId, false, Epoch + Day + 1);
        string rewardId = loaded.PendingWinReward.RewardId;
        HarvestRewardStorage.Save(path, loaded);
        loaded = HarvestRewardStorage.Load(path);
        restarted = new HarvestRewardEngine(engine.Config, loaded, () => 0.0);
        Equal(rewardId, loaded.PendingWinReward.RewardId, "pending reward ID restored");
        before = loaded.AvailableCents;
        True(restarted.ClaimWinBase(runId, Epoch + Day + 4), "restored reward claim succeeds");
        Equal(before + 1000, loaded.AvailableCents, "restored base reward paid once");
        HarvestRewardStorage.Save(path, loaded);
        restarted = new HarvestRewardEngine(engine.Config, HarvestRewardStorage.Load(path), () => 0.0);
        True(!restarted.ClaimWinBase(runId, Epoch + Day + 4), "claimed reward remains consumed after restart");
    }

    private static void InterruptedAdRestart()
    {
        var engine = QuickOffers();
        string runId = OpenQuickOffer(engine, Epoch);
        string requestId;
        long reward;
        True(engine.TryBeginRewardedClaim(HarvestRewardOffer.InGame, out requestId, out reward), "start ad before interruption");
        string path = FixturePath("ad-restart");
        HarvestRewardStorage.Save(path, engine.State);
        var restarted = new HarvestRewardEngine(engine.Config, HarvestRewardStorage.Load(path), () => 0.0);
        long before = restarted.State.AvailableCents;
        True(!restarted.CompleteRewardedClaim(requestId, true, Epoch + 3), "old process callback cannot authorize payment");
        True(restarted.ClaimInGameBase(runId, Epoch + 3), "base remains available after interrupted ad");
        Equal(before + 500, restarted.State.AvailableCents, "only base credited");
        Equal(0, restarted.State.InGameAdSuccessCount, "interrupted ad does not increment success count");
    }

    private static void CorruptStorage()
    {
        string missing = FixturePath("missing");
        Equal(0L, HarvestRewardStorage.Load(missing).AvailableCents, "missing save creates empty initial state");
        File.WriteAllBytes(missing + ".tmp", new byte[] { 1, 2, 3 });
        Throws<InvalidDataException>(() => HarvestRewardStorage.Load(missing), "orphan temporary file is interrupted storage, not a new account");
        string path = FixturePath("corrupt");
        var engine = Funded(500000);
        engine.Tick(Epoch);
        HarvestRewardStorage.Save(path, engine.State);
        Triples(engine, 1, Epoch + 1);
        HarvestRewardStorage.Save(path, engine.State);
        True(File.Exists(path + ".bak"), "atomic replacement leaves diagnostic backup");
        byte[] healthy = File.ReadAllBytes(path);
        byte[] damaged = (byte[])healthy.Clone();
        damaged[damaged.Length - 1] ^= 0x5a;
        File.WriteAllBytes(path, damaged);
        Throws<InvalidDataException>(() => HarvestRewardStorage.Load(path), "checksum detects modified payload; no stale backup replay");
        Equal(damaged.Length, File.ReadAllBytes(path).Length, "failed load leaves evidence intact");
        File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
        Throws<InvalidDataException>(() => HarvestRewardStorage.Load(path), "truncated save");
        File.WriteAllBytes(path, healthy);
        Equal(engine.State.AvailableCents, HarvestRewardStorage.Load(path).AvailableCents, "original healthy payload remains readable");
    }

    private static void AtomicSaveFailure()
    {
        var engine = Funded(500000);
        engine.Tick(Epoch);
        string path = FixturePath("atomic-failure");
        HarvestRewardStorage.Save(path, engine.State);
        byte[] original = File.ReadAllBytes(path);
        long previousBalance = engine.State.AvailableCents;
        Triples(engine, 1, Epoch + 1);
        using (var lockedPrimary = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Throws<IOException>(() => HarvestRewardStorage.Save(path, engine.State), "locked primary rejects replacement");
        }
        byte[] afterFailure = File.ReadAllBytes(path);
        Equal(original.Length, afterFailure.Length, "failed save keeps full primary length");
        for (int i = 0; i < original.Length; i++) Equal(original[i], afterFailure[i], "failed save keeps primary byte " + i);
        Equal(previousBalance, HarvestRewardStorage.Load(path).AvailableCents, "previous primary remains loadable");
        HarvestRewardStorage.Save(path, engine.State);
        Equal(engine.State.AvailableCents, HarvestRewardStorage.Load(path).AvailableCents, "save can recover after lock release");
    }

    private static void ClockRollback()
    {
        var engine = Funded();
        engine.TryApply(0, Epoch);
        Wins(engine, 10, Epoch);
        engine.Tick(Epoch + Day / 2);
        engine.Tick(Epoch - Day);
        True(engine.State.ClockRollbackDetected, "rollback detected");
        Equal(Epoch + Day / 2, engine.State.LastUtcSeconds, "persist UTC high-water value");
        Equal(0, engine.State.ActiveRequest.StageIndex, "rollback never advances stage");
        engine = new HarvestRewardEngine(engine.Config, engine.State, () => 0.0);
        engine.Tick(Epoch - 2 * Day);
        Equal(Epoch + Day / 2, engine.State.LastUtcSeconds, "restart retains UTC high-water value");
        engine.Tick(Epoch + Day);
        Equal(1, engine.State.ActiveRequest.StageIndex, "natural forward time can resume");
    }

    private static void InvalidConfiguration()
    {
        var settings = new HarvestRewardSettings();
        settings.Stages[0].Target = 0;
        Throws<InvalidOperationException>(() => settings.Validate(), "zero target");
        settings = new HarvestRewardSettings();
        settings.ThresholdCents[1] = settings.ThresholdCents[0];
        Throws<InvalidOperationException>(() => settings.Validate(), "duplicate threshold");
        settings = new HarvestRewardSettings();
        settings.WinProbabilities[0].NoAds = float.NaN;
        Throws<InvalidOperationException>(() => settings.Validate(), "NaN probability");
    }

    private static void Equal<T>(T expected, T actual, string context)
    {
        if (!object.Equals(expected, actual))
            throw new InvalidOperationException(context + ": expected " + expected + ", actual " + actual);
    }

    private static void True(bool value, string context)
    {
        if (!value) throw new InvalidOperationException(context);
    }

    private static void Close(double expected, double actual, string context)
    {
        True(Math.Abs(expected - actual) < 0.000001, context + ": expected " + expected + ", actual " + actual);
    }

    private static void NotNull(object value, string context) { True(value != null, context); }
    private static void IsNull(object value, string context) { True(value == null, context); }

    private static void Throws<T>(Action action, string context) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException(context + ": expected exception was not thrown");
    }
}
