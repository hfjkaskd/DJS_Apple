using System;
using System.IO;
using System.Security.Cryptography;
using FruitApkEarning;

// Tests the saved transaction boundaries, including real v2 wire compatibility.
// Resource stubs come from ApkServiceTests; no production singleton is reset.
public static class ApkParityStateTests
{
    private static int checks;
    private static readonly ApkEarningRules Rules = ApkEarningRules.CreateDefault();
    private static readonly long Now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private static void Check(bool result, string message)
    { if (!result) throw new InvalidOperationException(message); checks++; Console.WriteLine("PASS " + message); }
    private static long Unix(int day, int hour, int minute = 0)
    { return new DateTimeOffset(2026, 10, day, hour, minute, 0, TimeSpan.Zero).ToUnixTimeSeconds(); }
    private static Func<long, long> Calendar(int hours)
    {
        var zone = TimeZoneInfo.CreateCustomTimeZone("Fixture" + hours, TimeSpan.FromHours(hours), "Fixture", "Fixture");
        return utc => TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(utc), zone).Date.Ticks / TimeSpan.TicksPerDay;
    }
    private static ApkEarningState ActivationState()
    {
        var state = new ApkEarningState { CurrentLevel = 3, HighestClearedLevel = 2, ActivationPending = true };
        state.Records.Add(new ApkRecord { Ordinal = 1, AmountCents = 40, Archived = true, CreatedUtc = Now });
        state.Records.Add(new ApkRecord { Ordinal = 2, AmountCents = 60, Archived = true, CreatedUtc = Now });
        return state;
    }
    public static void Main()
    {
        string folder = Path.Combine(Path.GetTempPath(), "Fruit-ParityState-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        CalendarCases();
        MigrationCases(folder);
        TestVideoCases();
        ServiceTransactions(folder);
        Console.WriteLine("APK_PARITY_STATE_PASS " + checks + " checks");
    }
    private static void CalendarCases()
    {
        var state = new ApkEarningState();
        var engine = new ApkEarningEngine(Rules, state, () => .5, Calendar(-4));
        engine.Tick(Unix(20, 3, 30)); state.TodayWins = 7;
        Check(state.LoginDays == 1, "first local login starts at one");
        engine.Tick(Unix(20, 4, 30));
        Check(state.LoginDays == 2 && state.TodayWins == 0, "local midnight rolls over without a UTC-date change");
        state.TodayWins = 8;
        engine.Tick(Unix(21, 0, 30));
        Check(state.LoginDays == 2 && state.TodayWins == 8, "UTC midnight does not reset a still-current local day");
        Check(!engine.NeedsTick(Unix(21, 0, 31)), "same-local-day polling avoids cloning history");
        engine.Tick(Unix(19, 12));
        Check(state.LoginDays == 2 && state.TodayWins == 8, "backward local date cannot grant login or reset wins");
        engine.Tick(Unix(30, 12));
        Check(state.LoginDays == 3 && state.TodayWins == 0, "absence adds one observed login, not skipped days");
        foreach (int offset in new[] { -12, -4, 8, 14 }) {
            state = new ApkEarningState { LastUtcDay = Unix(20, 0) / 86400, LoginDays = 9, TodayWins = 6 };
            engine = new ApkEarningEngine(Rules, state, () => .5, Calendar(offset));
            engine.Tick(Unix(20, 1));
            Check(state.LoginDays == 9 && state.TodayWins == 6 && state.LastLocalDay == Calendar(offset)(Unix(20, 1)),
                "legacy UTC migration establishes local baseline without another login at UTC offset " + offset);
        }
    }
    private static void MigrationCases(string folder)
    {
        string path = Path.Combine(folder, "v2.dat");
        WriteV2(path, false);
        bool recovered;
        var state = ApkEarningStorage.Load(path, Rules, out recovered);
        Check(state.Version == 3 && state.LastLocalDay == -1 && state.LoginDays == 4 && state.TodayWins == 8,
            "v2 read preserves its UTC counters until first local tick");
        var engine = new ApkEarningEngine(Rules, state, () => .5, Calendar(14));
        engine.Tick(Unix(20, 12));
        ApkEarningStorage.Save(path, state);
        state = ApkEarningStorage.Load(path, Rules, out recovered);
        Check(state.LastLocalDay == Calendar(14)(Unix(20, 12)) && state.LoginDays == 4 && state.TodayWins == 8,
            "migrated local calendar baseline survives save and reload");
        WriteV2(path, true);
        state = ApkEarningStorage.Load(path, Rules, out recovered);
        Check(state.Activated && state.ActivationStep == 6 && state.ActivationBalanceApplied && state.CashCents == 51000,
            "old completed activation cannot replay its absolute balance grant");
        engine = new ApkEarningEngine(Rules, state, () => .5);
        Check(!engine.ApplyActivationBalance(Now) && state.CashCents == 51000, "legacy activation is never paid twice");
    }
    private static void TestVideoCases()
    {
        var state = new ApkEarningState { CurrentLevel = 4 };
        var engine = new ApkEarningEngine(Rules, state, () => .5);
        engine.BeginRun("video", 0, 4, Now); engine.RecordWin("video", 4, Now);
        ApkReward reward = state.PendingRewards[0];
        long cash = reward.CashCents, diamonds = reward.Diamonds;
        Check(!engine.ClaimWithTestVideo(reward.Id, false, Now) && state.CashCents == 0 && state.RewardVideos == 0 && state.PendingRewards.Count == 1,
            "cancelled video changes neither earned reward nor balance");
        Check(engine.ClaimWithTestVideo(reward.Id, true, Now) && state.CashCents == cash * 10 && state.Diamonds == diamonds * 10 && state.RewardVideos == 1,
            "completed labelled video multiplies both settlement currencies once");
        Check(!engine.ClaimWithTestVideo(reward.Id, true, Now) && state.CashCents == cash * 10 && state.RewardVideos == 1,
            "duplicate video completion cannot repeat reward or video count");
        engine.BeginRun("early", 0, 1, Now); engine.RecordWin("early", 1, Now);
        Check(!engine.ClaimWithTestVideo("early:level", true, Now) && state.PendingRewards.Count == 1 && state.RewardVideos == 1,
            "early tutorial withdrawal cannot use the bonus-video route");
        engine.Claim("early:level", Now);
        engine.BeginRun("lucky-video", 0, 4, Now);
        for (int i = 1; i <= 4; i++) engine.RecordMatch("lucky-video", i, 4, Now);
        reward = state.PendingRewards[0]; cash = state.CashCents; long lucky = reward.CashCents;
        Check(engine.ClaimWithTestVideo(reward.Id, true, Now) && state.CashCents == cash + lucky * 10 && state.RewardVideos == 2,
            "lucky reward also uses its configured bonus and review counter");
        state = new ApkEarningState { CashCents = long.MaxValue - 1, CurrentLevel = 4 };
        state.PendingRewards.Add(new ApkReward { Id = "overflow", Kind = "level", Level = 4, CashCents = 10, CreatedUtc = Now });
        engine = new ApkEarningEngine(Rules, state);
        bool rejected = false;
        try { engine.ClaimWithTestVideo("overflow", true, Now); } catch (OverflowException) { rejected = true; }
        Check(rejected && state.PendingRewards[0].CashCents == 10 && state.CashCents == long.MaxValue - 1 && state.RewardVideos == 0,
            "overflow cannot consume or multiply a pending reward in place");
    }
    private static void ServiceTransactions(string folder)
    {
        string path = Path.Combine(folder, "fruit-apk-earning-v1.dat");
        var state = ActivationState();
        ApkEarningStorage.Save(path, state);
        UnityEngine.Application.persistentDataPath = folder;
        UnityEngine.Resources.Asset = new ApkEarningConfig { Rules = Rules };
        var service = ApkEarningService.Instance;
        Check(service.Init(3), "service loads activation checkpoint");
        Check(!service.CompleteActivation() && !service.SetActivationStep(2), "early completion and skipped steps are rejected");
        bool recovered;
        for (int step = 1; step <= 6; step++) {
            if (step == 4) {
                Check(!service.SetActivationStep(4), "receipt cannot advance before its collection commits");
                Check(service.ApplyActivationBalance() && service.State.CashCents == 50000 && !service.State.Activated,
                    "receipt collection grants absolute balance while activation remains incomplete");
                Check(service.ApplyActivationBalance() && service.State.CashCents == 50000, "receipt completion callback is idempotent");
            }
            if (step == 2) {
                Directory.CreateDirectory(path + ".tmp");
                Check(!service.SetActivationStep(step) && service.State.ActivationStep == 1, "failed checkpoint write leaves published step unchanged");
                Directory.Delete(path + ".tmp");
                Check(service.Init(3) && service.State.ActivationStep == 1, "checkpoint retry reloads last completed page");
            }
            Check(service.SetActivationStep(step), "activation page " + step + " persists");
            var saved = ApkEarningStorage.Load(path, Rules, out recovered);
            Check(saved.ActivationStep == step && !saved.Activated, "restart at page " + step + " retains incomplete activation");
            Check(service.SetActivationStep(step) && !service.SetActivationStep(step - 1), "page callback repeats safely but cannot go backwards");
        }
        Check(service.CompleteActivation() && service.State.Activated && !service.State.ActivationPending,
            "activation completes only after the final page");
        Check(service.CompleteActivation() && service.State.CashCents == 50000, "final completion can safely repeat");
        Check(service.BeginRun("saved-video", 0, 4) && service.RecordWin("saved-video", 4), "service prepares normal settlement for video");
        long before = service.State.CashCents;
        long grant = service.State.PendingRewards[0].CashCents * 10;
        byte[] previous = File.ReadAllBytes(path);
        Check(!service.ClaimWithTestVideo("saved-video:level", false) && SameBytes(previous, File.ReadAllBytes(path)),
            "cancellation performs no persistence write");
        Directory.CreateDirectory(path + ".tmp");
        Check(!service.ClaimWithTestVideo("saved-video:level", true) && service.State.CashCents == before &&
            service.State.PendingRewards.Count == 1 && service.State.RewardVideos == 0, "failed video grant preserves reward, balance and review counter");
        Directory.Delete(path + ".tmp");
        Check(service.Init(4) && service.State.PendingRewards.Count == 1, "same-session recovery keeps unclaimed video reward");
        Check(service.ClaimWithTestVideo("saved-video:level", true) && service.State.CashCents == before + grant && service.State.RewardVideos == 1,
            "successful video grant commits balance and counter atomically");
        var loaded = ApkEarningStorage.Load(path, Rules, out recovered);
        Check(loaded.RewardVideos == 1 && loaded.PendingRewards.Count == 0 && loaded.CashCents == before + grant,
            "reload cannot replay the committed video");
        Check(!service.ClaimWithTestVideo("saved-video:level", true) && service.State.RewardVideos == 1,
            "late duplicate video callback has no effect");
    }
    private static bool SameBytes(byte[] a, byte[] b)
    { if (a.Length != b.Length) return false; for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
    private static void WriteV2(string path, bool activated)
    {
        byte[] payload;
        using (var stream = new MemoryStream()) using (var writer = new BinaryWriter(stream)) {
            writer.Write(2); writer.Write(51000L); writer.Write(12345L);
            writer.Write(4); writer.Write(3); writer.Write(8); writer.Write(4); writer.Write(0);
            writer.Write(activated); writer.Write(false); writer.Write(true); writer.Write(true);
            writer.Write(activated ? 6 : 0); writer.Write(0); writer.Write(0L);
            writer.Write(Unix(20, 0) / 86400); writer.Write(Unix(20, 0)); writer.Write(0L);
            writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(true);
            writer.Flush(); payload = stream.ToArray();
        }
        using (var sha = SHA256.Create()) using (var file = File.Create(path)) using (var writer = new BinaryWriter(file)) {
            writer.Write(0x46414531u); writer.Write(2); writer.Write(payload.Length); writer.Write(sha.ComputeHash(payload)); writer.Write(payload);
        }
    }
}
