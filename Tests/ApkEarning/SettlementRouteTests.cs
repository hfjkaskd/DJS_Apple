using System;
using CorePlay;
using Framework.Base.UtilModule;
using FruitApkEarning;

// Compiles and executes the repository's real LDNJKLHLHAH.cs.
// Host/scene behavior is stubbed: this is routing coverage, not a visual or device test.
public static class SettlementRouteTests
{
    private static int passed;

    public static int Main()
    {
        Run(RewardThenOneNextLevel, "one reward screen and one continuation after duplicate callbacks");
        Run(WaitForInputLock, "waits for the closing-animation input lock");
        Run(WaitForExistingReward, "existing reward flow finishes before continuation");
        Run(CancelWhenGameplayCloses, "closed gameplay cancels a queued settlement");
        Run(ContinueAlreadyClaimedReward, "already-claimed rewards need no empty settlement page");
        Run(CancelStaleGeneration, "a stale settlement cannot interrupt a newer level");
        Run(KeepRecoveryFlow, "save failure keeps the existing recovery UI");
        Run(FormerRatingLevelsGoDirectToReward, "levels 6/16/36/66/106 open only one reward and continue once");
        Run(LevelSixAlreadyClaimedReward, "level 6 already-claimed reward continues once without a rating page");
        Run(LevelSixWaitsOnlyForInputLock, "level 6 waits for the real input lock and needs no rating callback");
        Console.WriteLine("SETTLEMENT_ROUTE_PASS cases=" + passed + "/10; isolated routing tests, not a Unity runtime capture.");
        return 0;
    }

    private static void Run(Action test, string description)
    {
        Reset();
        test();
        Check(MgrUI.Instance.LegacyWinOpens == 0, "The original WinUI must never open.");
        Check(MgrUI.Instance.RatingOpens == 0, "Settlement must not open the original rating UI.");
        Check(Module.Setting.UIRate.onCloseAction == null, "Settlement must not register a rating continuation.");
        passed++;
        Console.WriteLine("PASS " + description);
    }

    private static void Reset()
    {
        EDLHEMMBABM.Instance = new EDLHEMMBABM();
        JEFOMCDAPGK.Instance = new JEFOMCDAPGK();
        CorePlayUI.Instance = new CorePlayUI();
        BMNFNJFCPHG.Instance = new BMNFNJFCPHG();
        MgrUI.Instance = new MgrUI();
        CoroutineManager.Instance = new CoroutineManager();
        ApkEarningService.Instance = new ApkEarningService();
        ApkEarningService.Instance.State.PendingRewards.Add(new object());
        ApkEarningHost.IsOpen = false;
        ApkEarningHost.Opens = 0;
        ApkEarningHost.Completion = null;
        Module.Setting.UIRate.onCloseAction = null;
        MCCIJBJGMCK.Locked = false;
        WinUI.wonLevel = 3;
    }

    private static void Start() { new LDNJKLHLHAH().StartQueue(false, OGPEIPAJIDB.MainLevelWin); }
    private static void Tick() { CoroutineManager.Instance.Tick(); }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void RewardThenOneNextLevel()
    {
        Start();
        Check(ApkEarningHost.Opens == 1 && EDLHEMMBABM.Instance.StartedLevels == 0,
            "The new reward page must open before advancing.");
        Action finished = ApkEarningHost.Completion;
        ApkEarningHost.IsOpen = false;
        finished();
        finished();
        Check(EDLHEMMBABM.Instance.StartedLevels == 1, "Duplicate completion advanced more than once.");
    }

    private static void WaitForInputLock()
    {
        MCCIJBJGMCK.Locked = true;
        Start();
        Check(ApkEarningHost.Opens == 0, "The input lock was ignored.");
        MCCIJBJGMCK.Locked = false;
        Tick();
        Check(ApkEarningHost.Opens == 1, "Settlement did not resume after unlocking.");
    }

    private static void WaitForExistingReward()
    {
        ApkEarningHost.IsOpen = true;
        Start();
        Check(ApkEarningHost.Opens == 0, "An existing reward flow was opened twice.");
        ApkEarningService.Instance.State.PendingRewards.Clear();
        ApkEarningHost.IsOpen = false;
        Tick();
        Check(EDLHEMMBABM.Instance.StartedLevels == 1, "Settlement did not advance after the existing reward was claimed.");
    }

    private static void CancelWhenGameplayCloses()
    {
        MCCIJBJGMCK.Locked = true;
        Start();
        CorePlayUI.Instance.IsOpening = false;
        MCCIJBJGMCK.Locked = false;
        Tick();
        Check(ApkEarningHost.Opens == 0 && EDLHEMMBABM.Instance.StartedLevels == 0,
            "Settlement resumed after gameplay closed.");
    }

    private static void ContinueAlreadyClaimedReward()
    {
        ApkEarningService.Instance.State.PendingRewards.Clear();
        Start();
        Check(EDLHEMMBABM.Instance.StartedLevels == 1 && ApkEarningHost.Opens == 0,
            "An already-claimed reward showed another page or did not continue.");
    }

    private static void CancelStaleGeneration()
    {
        MCCIJBJGMCK.Locked = true;
        Start();
        EDLHEMMBABM.Instance.Generation++;
        MCCIJBJGMCK.Locked = false;
        Tick();
        Check(ApkEarningHost.Opens == 0 && EDLHEMMBABM.Instance.StartedLevels == 0,
            "An old level's settlement resumed after the generation changed.");
    }

    private static void KeepRecoveryFlow()
    {
        ApkEarningService.Instance.Available = false;
        ApkEarningService.Instance.Error = "save unavailable";
        Start();
        Check(ApkEarningHost.Opens == 1 && EDLHEMMBABM.Instance.StartedLevels == 0,
            "A save failure bypassed the existing recovery flow.");
    }

    private static void FormerRatingLevelsGoDirectToReward()
    {
        foreach (int level in new[] { 6, 16, 36, 66, 106 })
        {
            Reset();
            BMNFNJFCPHG.Instance.Rated = false;
            WinUI.wonLevel = level;
            Start();
            Tick();
            Check(MgrUI.Instance.RatingOpens == 0 && Module.Setting.UIRate.onCloseAction == null,
                "Former rating checkpoint opened a rating page or installed its callback: " + level);
            Check(ApkEarningHost.Opens == 1 && EDLHEMMBABM.Instance.StartedLevels == 0,
                "Former rating checkpoint did not immediately open exactly one reward: " + level);
            Action finished = ApkEarningHost.Completion;
            ApkEarningHost.IsOpen = false;
            finished();
            finished();
            Tick();
            Check(ApkEarningHost.Opens == 1 && EDLHEMMBABM.Instance.StartedLevels == 1,
                "Reward completion did not advance exactly once at level " + level);
        }
    }

    private static void LevelSixAlreadyClaimedReward()
    {
        BMNFNJFCPHG.Instance.Rated = false;
        WinUI.wonLevel = 6;
        ApkEarningService.Instance.State.PendingRewards.Clear();
        Start();
        Tick();
        Check(ApkEarningHost.Opens == 0 && EDLHEMMBABM.Instance.StartedLevels == 1,
            "Already-claimed level 6 must continue once without an empty reward or rating page.");
    }

    private static void LevelSixWaitsOnlyForInputLock()
    {
        BMNFNJFCPHG.Instance.Rated = false;
        WinUI.wonLevel = 6;
        MCCIJBJGMCK.Locked = true;
        Start();
        Tick();
        Check(MgrUI.Instance.RatingOpens == 0 && ApkEarningHost.Opens == 0,
            "Level 6 opened a page before its real input lock ended.");
        MCCIJBJGMCK.Locked = false;
        Tick();
        Tick();
        Check(ApkEarningHost.Opens == 1 && EDLHEMMBABM.Instance.StartedLevels == 0,
            "Level 6 did not open exactly one reward immediately after unlocking.");
        ApkEarningHost.IsOpen = false;
        ApkEarningHost.Completion();
        Tick();
        Check(EDLHEMMBABM.Instance.StartedLevels == 1,
            "Level 6 needed a rating callback to continue after its reward.");
    }
}
