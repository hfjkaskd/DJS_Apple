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
        Run(RatingThenReward, "rating popup hands off directly to the new reward settlement");
        Console.WriteLine("SETTLEMENT_ROUTE_PASS cases=" + passed + "/8; isolated routing tests, not a Unity runtime capture.");
        return 0;
    }

    private static void Run(Action test, string description)
    {
        Reset();
        test();
        Check(MgrUI.Instance.LegacyWinOpens == 0, "The original WinUI must never open.");
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

    private static void RatingThenReward()
    {
        BMNFNJFCPHG.Instance.Rated = false;
        WinUI.wonLevel = 6;
        Start();
        Check(MgrUI.Instance.RatingOpens == 1 && ApkEarningHost.Opens == 0,
            "Rating and reward pages were opened together.");
        Module.Setting.UIRate.onCloseAction();
        Check(ApkEarningHost.Opens == 1, "Rating did not continue to the new reward page.");
    }
}
