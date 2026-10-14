using System;
using FruitApkEarning;

static class ApkEarningTests
{
    const long Utc = 1700000000;
    static int count;
    static void Check(bool value, string message) { if (!value) throw new Exception(message); count++; }
    static ApkEarningEngine Engine(ApkEarningState state = null, double random = 0.5)
    { return new ApkEarningEngine(ApkEarningRules.CreateDefault(), state ?? new ApkEarningState(), () => random); }
    static ApkReward Win(ApkEarningEngine engine, string run, int level, long utc = Utc)
    { engine.BeginRun(run, 0, level, utc); engine.RecordWin(run, level, utc); return engine.State.PendingRewards[engine.State.PendingRewards.Count - 1]; }
    static void Main()
    {
        EarlyFlow(); MatchAndLucky(); SettlementBands(); CalendarAndConditions(); ResumeAndBounds();
        Console.WriteLine("APK_EARNING_PASS " + count + " checks");
    }
    static void EarlyFlow()
    {
        var engine = Engine();
        var first = Win(engine, "first", 1);
        Check(first.CashCents == 40 && first.Diamonds == 0 && first.EarlyOrdinal == 1, "US first reward is 0.40, with no diamond welcome grant");
        Check(engine.State.CashCents == 0, "Presentation alone does not grant reward");
        Check(engine.Claim(first.Id, Utc) && !engine.Claim(first.Id, Utc), "Claim is idempotent");
        Check(engine.State.CashCents == 40 && engine.State.Records.Count == 1, "Claim records exact first reward");
        Check(engine.ArchiveEarlyReward(1, Utc) && engine.State.CashCents == 0 && engine.State.Records[0].AmountCents == 40, "Archive transfers rather than destroys earned value");
        Check(!engine.ArchiveEarlyReward(1, Utc) && !engine.CompleteActivation(Utc), "One early reward cannot activate");
        Check(engine.SubmitEarlyRecord(1, Utc) && !engine.SubmitEarlyRecord(1, Utc) && engine.State.Records[0].Pending, "Early request remains local pending");
        var second = Win(engine, "second", 2);
        Check(second.CashCents == 60 && second.EarlyOrdinal == 2, "US second reward is 0.60");
        engine.Claim(second.Id, Utc); engine.ArchiveEarlyReward(2, Utc);
        Check(engine.State.ActivationPending && !engine.State.Activated, "Both archived records begin activation");
        Check(engine.CompleteActivation(Utc) && engine.State.CashCents == 50000 && engine.State.Activated && !engine.State.ActivationPending, "US activation sets absolute $500");
        Check(!engine.CompleteActivation(Utc) && engine.State.CashCents == 50000, "Activation does not repeat grant");
        var high = Engine(new ApkEarningState { CurrentLevel = 3, CashCents = 60000 });
        high.State.Records.Add(new ApkRecord { Ordinal = 1, AmountCents = 40, Archived = true });
        high.State.Records.Add(new ApkRecord { Ordinal = 2, AmountCents = 60, Archived = true });
        Check(high.CompleteActivation(Utc) && high.State.CashCents == 60000, "Tutorial balance never reduces existing cash");
        var late = Engine(new ApkEarningState { CurrentLevel = 4 });
        late.State.Records.Add(new ApkRecord { Ordinal = 1, AmountCents = 40, Archived = true });
        late.State.Records.Add(new ApkRecord { Ordinal = 2, AmountCents = 60, Archived = true });
        Check(late.CompleteActivation(Utc) && late.State.CashCents == 0, "No guessed historical tutorial balance");
    }
    static void MatchAndLucky()
    {
        var e = Engine(); e.BeginRun("l2", 0, 2, Utc); e.RecordMatch("l2", 1, 2, Utc);
        Check(e.State.CashCents == 0 && e.State.Diamonds == 0, "No rod rewards before level 3");
        e = Engine(); e.BeginRun("l3", 0, 3, Utc);
        Check(e.RecordMatch("l3", 1, 3, Utc) && e.State.CashCents == 10 && e.State.Diamonds == 0, "Level 3 match grants 0.10 only");
        Check(!e.RecordMatch("l3", 1, 3, Utc) && !e.RecordMatch("wrong", 2, 3, Utc) && e.State.CashCents == 10, "Duplicate and unregistered matches rejected");
        e.RecordMatch("l3", 2, 3, Utc); e.RecordMatch("l3", 3, 3, Utc); e.RecordMatch("l3", 4, 3, Utc);
        Check(e.State.PendingRewards.Count == 1 && e.State.PendingRewards[0].Kind == "lucky" && e.State.PendingRewards[0].CashCents == 150, "Fourth match triggers cached cash-band Lucky");
        e.RecordMatch("l3", 5, 3, Utc + 50);
        Check(e.State.PendingRewards.Count == 1, "Existing modal prevents another Lucky reward");
        e = Engine(); e.BeginRun("timer", 0, 3, Utc); e.RecordMatch("timer", 1, 3, Utc + 30);
        Check(e.State.PendingRewards.Count == 1, "30 seconds triggers Lucky on next match");
        e = Engine(); e.BeginRun("last", 0, 4, Utc); e.RecordMatch("last", 1, 4, Utc + 40, true);
        Check(e.State.CashCents == 10 && e.State.Diamonds == 100 && e.State.PendingRewards.Count == 0, "Last triple rewards both currencies but never interrupts victory with Lucky");
        var reward = Win(e, "level4", 4);
        Check(reward.Diamonds == 10000 && e.State.Diamonds == 100, "Level 4 pending award is 100 diamonds, not preview 30000");
        e.Claim(reward.Id, Utc + 40);
        Check(e.State.Diamonds == 10100 && e.State.AccountLevel == 0, "Claim adds correct units without inventing account rank");
    }
    static void SettlementBands()
    {
        var e = Engine(new ApkEarningState { CashCents = 5000 });
        Check(Win(e, "band", 3).CashCents == 120, "Cash lower-bound inclusive band");
        e = Engine(new ApkEarningState { CashCents = 29999 });
        Check(Win(e, "fallback", 3).CashCents == 10, "Cash band outside fallback matches bear_torque");
        e = Engine(new ApkEarningState { CashCents = 29000 }, 0);
        Check(Win(e, "round", 3).CashCents == 1, "Sub-cent result rounds away from zero");
        e = Engine(new ApkEarningState { Diamonds = 3200000 });
        Check(Win(e, "diamond-edge", 4).Diamonds == 8000, "Diamond interval threshold inclusive at 32000");
        e = Engine(new ApkEarningState { Diamonds = 4700000 }, 0);
        Check(Win(e, "decimal", 4).Diamonds == 120, "Fractional diamonds retained exactly");
        e = Engine(new ApkEarningState { Diamonds = 4800000 }, 1);
        Check(Win(e, "last-band", 4).Diamonds == 120, "Highest diamond interval continues above last threshold");
        var custom = ApkEarningRules.CreateDefault(); custom.MatchCashCents = 0;
        e = new ApkEarningEngine(custom, new ApkEarningState { CashCents = 50000 }, () => 0.5);
        var diamondOnly = Win(e, "diamond-only", 4);
        Check(diamondOnly.CashCents == 0 && diamondOnly.Diamonds > 0, "Diamond-only settlement is supported");
        e = new ApkEarningEngine(custom, e.State, () => 0.5);
        Check(e.Claim(diamondOnly.Id, Utc) && e.State.CashCents == 50000 && e.State.Diamonds == 10000, "Diamond-only pending survives restore and claim");
    }
    static void CalendarAndConditions()
    {
        var e = Engine(); Check(e.Tick(Utc) && e.State.LoginDays == 1, "First login gives one day");
        e.State.TodayWins = 7; Check(!e.Tick(Utc - 86400) && e.State.TodayWins == 7 && e.State.LoginDays == 1, "Backwards clock neither resets nor grants days");
        e.Tick(Utc + 86400 * 20); Check(e.State.LoginDays == 2 && e.State.TodayWins == 0, "A long absence counts one observed login, no skipped-day grants");
        var state = new ApkEarningState { CurrentLevel = 52, HighestClearedLevel = 51, CashCents = 60000, LoginDays = 5, AccountLevel = 31, CashGuidanceComplete = true };
        e = Engine(state); var needs = e.GetRequirements(false, 0);
        Check(e.GetCashStage() == 0 && needs.FirstIncomplete == "balance" && !needs.Items[2].Known, "MARK remains unknown rather than a made-up $500 target");
        state.CashTargetCents = 50000;
        Check(e.GetCashStage() == 7 && e.GetRequirements(false, 0).Eligible, "Completed login requirement waives daily gate");
        state.LoginDays = 4; state.TodayWins = 9;
        Check(e.GetCashStage() == 5 && e.GetRequirements(false, 0).FirstIncomplete == "daily", "Incomplete day requirements route daily first");
        state.TodayWins = 10; Check(e.GetCashStage() == 6 && e.GetRequirements(false, 0).FirstIncomplete == "login", "Daily completion routes login next");
        state.Diamonds = 5000000; state.LoginDays = 5;
        Check(e.GetRequirements(true, 0).Eligible && !e.GetRequirements(true, 1).Eligible, "Diamond tiers independently test balance/login/rank");
        Check(state.CashCents == 60000 && state.Diamonds == 5000000, "Eligibility never freezes/deducts balance");
    }
    static void ResumeAndBounds()
    {
        int samples = 0; var state = new ApkEarningState();
        var e = new ApkEarningEngine(ApkEarningRules.CreateDefault(), state, () => { samples++; return 0.5; });
        e.BeginRun("resume", 0, 4, Utc); e.RecordMatch("resume", 1, 4, Utc, true); e.RecordWin("resume", 4, Utc);
        var resumed = new ApkEarningEngine(ApkEarningRules.CreateDefault(), state, () => { throw new Exception("must not reroll"); });
        Check(!resumed.RecordWin("resume", 4, Utc) && !resumed.RecordMatch("resume", 2, 4, Utc), "Restored completed run rejects duplicate events");
        Check(resumed.Claim("resume:level", Utc) && samples == 2, "Persisted pending rewards never reroll on resume");
        e = Engine(new ApkEarningState { CashCents = long.MaxValue }); e.BeginRun("overflow", 0, 3, Utc);
        bool threw = false; try { e.RecordMatch("overflow", 1, 3, Utc); } catch (OverflowException) { threw = true; }
        Check(threw && e.State.CashCents == long.MaxValue && e.State.Runs[0].LastSequence == 0, "Overflow cannot mark reward as consumed");
        e = Engine(); e.State.Records.Add(new ApkRecord { Ordinal = 1, AmountCents = 40 });
        Check(!e.ArchiveEarlyReward(1, Utc) && e.State.CashCents == 0 && !e.State.Records[0].Archived, "Cannot archive money unavailable in balance");
        var corrupt = new ApkEarningState(); corrupt.PendingRewards.Add(new ApkReward { Id = "bad", Kind = "level", Level = 4, CashCents = -1, CreatedUtc = Utc });
        threw = false; try { Engine(corrupt); } catch (InvalidOperationException) { threw = true; }
        Check(threw, "Negative pending amount cannot subtract a claimed balance");
        corrupt = new ApkEarningState(); corrupt.Records.Add(new ApkRecord { Ordinal = 1, AmountCents = 40, Pending = true, SubmittedUtc = Utc });
        threw = false; try { Engine(corrupt); } catch (InvalidOperationException) { threw = true; }
        Check(threw, "Unarchived record cannot already be submitted");
    }
}
