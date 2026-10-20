using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using FruitApkEarning;

// Compiled with the existing service-test Unity resource/path stubs. Each mode
// runs in a fresh Mono process and uses only a newly allocated temporary folder.
public static class ApkStartupRecoveryTests
{
    private static int checks;
    private static string root;
    private static readonly ApkEarningRules Rules = ApkEarningRules.CreateDefault();
    private static long Now { get { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); } }
    private static string SavePath { get { return Path.Combine(root, "fruit-apk-earning-v1.dat"); } }
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
        Console.WriteLine("PASS " + message);
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2 || args.Length > 3) throw new ArgumentException("Pass a mode and isolated temporary directory.");
        root = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(root);
        UnityEngine.Application.persistentDataPath = root;
        UnityEngine.Resources.Asset = new ApkEarningConfig { Rules = Rules };
        Check(!ApkEarningService.Instance.Available && ApkEarningService.Instance.State == null,
            "fresh process starts with no published reward state");
        switch (args[0])
        {
            case "seed-unclaimed": SeedUnclaimed(); break;
            case "seed-claimed": SeedClaimed(); break;
            case "seed-unfinished": SeedUnfinished(); break;
            case "seed-copy": SeedCopy(args[2]); break;
            case "resume": Resume(false); break;
            case "resume-failed-save": Resume(true); break;
            case "runtime-claim-retry": RuntimeClaimRetry(); break;
            default: throw new ArgumentException("Unknown mode " + args[0]);
        }
        Console.WriteLine("APK_STARTUP_RECOVERY_PASS mode=" + args[0] + " checks=" + checks);
    }

    private static ApkEarningEngine Engine()
    {
        return new ApkEarningEngine(Rules,
            new ApkEarningState { CashCents = 315, Diamonds = 10200, CurrentLevel = 5, HighestClearedLevel = 4 },
            () => .5);
    }

    private static void SeedUnclaimed()
    {
        var engine = Engine();
        long now = Now;
        engine.BeginRun("level-five", 0, 5, now);
        for (int sequence = 1; sequence <= Rules.LuckyMatchCount; sequence++)
            engine.RecordMatch("level-five", sequence, 5, now);
        engine.RecordWin("level-five", 5, now);
        engine.BeginRun("early-one", 0, 1, now); engine.RecordWin("early-one", 1, now);
        engine.BeginRun("early-two", 0, 2, now); engine.RecordWin("early-two", 2, now);
        Check(engine.State.PendingRewards.Count == 4 &&
            engine.State.PendingRewards[0].Kind == "lucky" && engine.State.PendingRewards[1].Kind == "level",
            "save contains earned lucky, level and both early rewards before any Claim");
        SaveSeed(engine.State);
    }

    private static void SeedClaimed()
    {
        var engine = Engine();
        long now = Now;
        engine.BeginRun("claimed-before-receipt-ended", 0, 5, now);
        engine.RecordWin("claimed-before-receipt-ended", 5, now);
        Check(engine.Claim("claimed-before-receipt-ended:level", now),
            "receipt-interruption fixture commits Claim before application shutdown");
        Check(engine.State.PendingRewards.Count == 0, "claimed receipt has no pending reward to replay");
        SaveSeed(engine.State);
    }

    private static void SeedUnfinished()
    {
        var engine = Engine();
        long now = Now;
        engine.BeginRun("unfinished-level", 0, 5, now);
        engine.RecordMatch("unfinished-level", 1, 5, now, true);
        Check(engine.State.PendingRewards.Count == 0 && !engine.State.Runs[0].Won,
            "unfinished level has only committed match earnings");
        SaveSeed(engine.State);
    }

    private static void SaveSeed(ApkEarningState state)
    {
        long cash = state.CashCents, diamonds = state.Diamonds, won = 0, earlyCount = state.Records.Count;
        var expectedRecords = new List<string>();
        foreach (ApkRecord record in state.Records) expectedRecords.Add(RecordExpectation(record));
        foreach (ApkReward reward in state.PendingRewards)
        {
            cash = checked(cash + reward.CashCents);
            diamonds = checked(diamonds + reward.Diamonds);
            if (reward.EarlyOrdinal > 0)
            {
                earlyCount++;
                expectedRecords.Add(RecordExpectation(new ApkRecord {
                    Ordinal = reward.EarlyOrdinal, AmountCents = reward.CashCents }));
            }
        }
        foreach (ApkRunCheckpoint run in state.Runs) if (run.Won) won++;
        // Service.Init already performs the ordinary UTC-day rollover. A device
        // backup can be from an earlier day; recovery must not add a win, while
        // the existing day rollover is still allowed to reset today's count.
        long todayWins = state.LastUtcDay >= 0 && Now / 86400 > state.LastUtcDay ? 0 : state.TodayWins;
        long[] expected = { cash, diamonds, state.CurrentLevel, state.HighestClearedLevel,
            todayWins, state.Runs.Count, won, earlyCount };
        var lines = new string[expected.Length];
        for (int i = 0; i < expected.Length; i++) lines[i] = expected[i].ToString(CultureInfo.InvariantCulture);
        File.WriteAllLines(Path.Combine(root, "expected.txt"), lines);
        File.WriteAllLines(Path.Combine(root, "expected-records.txt"), expectedRecords.ToArray());
        ApkEarningStorage.Save(SavePath, state);
        bool recovered;
        ApkEarningState roundTrip = ApkEarningStorage.Load(SavePath, Rules, out recovered);
        Check(!recovered && roundTrip.PendingRewards.Count == state.PendingRewards.Count &&
            roundTrip.CashCents == state.CashCents && roundTrip.Diamonds == state.Diamonds,
            "actual checksummed binary storage round trip preserves the interrupted state");
    }

    private static string RecordExpectation(ApkRecord record)
    {
        return record.Ordinal.ToString(CultureInfo.InvariantCulture) + "|" +
            record.AmountCents.ToString(CultureInfo.InvariantCulture) + "|" + record.Archived + "|" + record.Pending;
    }

    private static void SeedCopy(string originalPath)
    {
        // Read the supplied backup; never point Service or Storage.Save at it.
        byte[] original = File.ReadAllBytes(originalPath);
        File.WriteAllBytes(SavePath, original);
        bool recovered;
        ApkEarningState state = ApkEarningStorage.Load(SavePath, Rules, out recovered);
        long cashDelta = 0, diamondDelta = 0;
        int level = 0, lucky = 0;
        foreach (ApkReward reward in state.PendingRewards)
        {
            cashDelta = checked(cashDelta + reward.CashCents);
            diamondDelta = checked(diamondDelta + reward.Diamonds);
            if (reward.Kind == "level") level++; else lucky++;
        }
        Console.WriteLine("BACKUP_REWARD_TOTALS pending=" + state.PendingRewards.Count + " level=" + level +
            " lucky=" + lucky + " cashCents=" + state.CashCents + " diamondsHundredths=" + state.Diamonds +
            " cashDeltaCents=" + cashDelta + " diamondDeltaHundredths=" + diamondDelta);
        SaveSeed(state);
        Check(Convert.ToBase64String(original) == Convert.ToBase64String(File.ReadAllBytes(originalPath)),
            "original supplied backup remains byte-identical; only its isolated copy was used");
    }

    private static void RuntimeClaimRetry()
    {
        var service = ApkEarningService.Instance;
        Check(service.Init(5) && service.BeginRun("live-claim-retry", 0, 5) &&
            service.RecordWin("live-claim-retry", 5), "live session creates its normal claimable reward");
        ApkReward reward = service.State.PendingRewards[0];
        long expectedCash = service.State.CashCents + reward.CashCents;
        long expectedDiamonds = service.State.Diamonds + reward.Diamonds;
        string obstacle = SavePath + ".tmp";
        Directory.CreateDirectory(obstacle);
        try
        {
            Check(!service.Claim(reward.Id) && !service.Available && service.State.PendingRewards.Count == 1,
                "failed live Claim retains the committed pending reward");
        }
        finally { Directory.Delete(obstacle); }
        Check(service.Init(6) && service.State.PendingRewards.Count == 1,
            "same-session Init retry does not consume the modal's pending Claim");
        Check(service.Claim(reward.Id) && !service.Claim(reward.Id) && service.State.PendingRewards.Count == 0 &&
            service.State.CashCents == expectedCash && service.State.Diamonds == expectedDiamonds,
            "original modal Claim can retry once without a recovery loop or duplicate balance");
    }

    private static void Resume(bool blockSave)
    {
        var lines = File.ReadAllLines(Path.Combine(root, "expected.txt"));
        var expected = new long[lines.Length];
        for (int i = 0; i < lines.Length; i++) expected[i] = long.Parse(lines[i], CultureInfo.InvariantCulture);
        var service = ApkEarningService.Instance;
        int successfulPublications = 0;
        service.Changed += () =>
        {
            if (!service.Available) return;
            bool recovered;
            ApkEarningState disk = ApkEarningStorage.Load(SavePath, Rules, out recovered);
            Check(disk.CashCents == service.State.CashCents && disk.Diamonds == service.State.Diamonds &&
                disk.PendingRewards.Count == 0, "successful Changed observes already-committed recovered earnings");
            successfulPublications++;
        };
        if (blockSave)
        {
            byte[] original = File.ReadAllBytes(SavePath);
            string obstacle = SavePath + ".tmp";
            Directory.CreateDirectory(obstacle);
            try
            {
                Check(!service.Init((int)expected[2]) && !service.Available && service.State == null,
                    "failed startup save never publishes recovered balances");
                Check(Convert.ToBase64String(original) == Convert.ToBase64String(File.ReadAllBytes(SavePath)),
                    "failed startup save leaves the committed pending rewards byte-identical");
                Check(successfulPublications == 0, "failed startup recovery emits no successful publication");
            }
            finally { Directory.Delete(obstacle); } // exact empty directory created by this fixture
        }
        Check(service.Init((int)expected[2]) && service.Available,
            "actual Service.Init recovers interrupted rewards before UI startup");
        ApkEarningState state = service.State;
        Check(state.PendingRewards.Count == 0 && state.CashCents == expected[0] && state.Diamonds == expected[1],
            "all saved rewards credited exactly once with no stale presentation queue");
        long won = 0;
        foreach (ApkRunCheckpoint run in state.Runs) if (run.Won) won++;
        Check(state.CurrentLevel == expected[2] && state.HighestClearedLevel == expected[3] &&
            state.TodayWins == expected[4] && state.Runs.Count == expected[5] && won == expected[6],
            "startup recovery does not create a victory, checkpoint or level progression");
        Check(state.Records.Count == expected[7], "early reward record count is preserved without duplicates");
        var expectedRecords = new HashSet<string>(File.ReadAllLines(Path.Combine(root, "expected-records.txt")));
        foreach (ApkRecord record in state.Records)
            Check(expectedRecords.Remove(RecordExpectation(record)),
                "early reward record retains its earned amount and original archive/submission state");
        Check(expectedRecords.Count == 0, "all expected early records remain present exactly once");
        Check(successfulPublications == 1 && service.Init((int)expected[2]) && successfulPublications == 1,
            "repeated Init in the same process neither republishes nor credits again");
        foreach (ApkRunCheckpoint run in state.Runs)
            if (run.Won)
                Check(!service.RecordWin(run.Id, run.Level) && !service.Claim(run.Id + ":level"),
                    "recovered winning checkpoint rejects replayed Win and Claim");
        Check(service.State.CashCents == expected[0] && service.State.Diamonds == expected[1],
            "repeated initialization and duplicate callbacks preserve balances");
    }
}
