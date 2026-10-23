using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Apple.Framework2;
using MahjongSlide.Withdrawal;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WZSDK;
using Object = UnityEngine.Object;

/// <summary>
/// Isolated Play-mode smoke of the imported production runtime and saved prefabs.
/// Run in a separate batch Editor without -quit. Never selects an actual A/B profile.
/// </summary>
[InitializeOnLoad]
public static class Framework2ReplicaPlayValidation
{
    private const string ActiveKey = "Apple.F2.PlayValidation.Active";
    private const string OutputKey = "Apple.F2.PlayValidation.Output";
    private const string StartKey = "Apple.F2.PlayValidation.Start";
    private const string DeadlineKey = "Apple.F2.PlayValidation.Deadline";
    private const double MaximumSeconds = 120;
    private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
    private static readonly Queue<Action<bool>> AdResponses = new Queue<Action<bool>>();
    private static AppleF2Runtime runtime;
    private static GameObject fixtureRoot;
    private static EventSystem events;
    private static PlayerLoopSystem originalLoop;
    private static Report report;
    private static bool started, finishing, installed;
    private static int lastFrame = -1;
    private static string output;

    [Serializable]
    private sealed class Report
    {
        public bool passed;
        public string generatedUtc, storageNamespace, firstTwoLevelsStorageNamespace;
        public bool firstTwoLevelsCompletedThroughSourceButtons;
        public string scope = "Production B facade, source prefabs, real Play-mode frames and standard Button pointer events. " +
            "All data uses a new DJS.Apple.F2Validation GUID namespace. A and the formal B profile are never loaded. " +
            "Stage 3, stage 5 and level 4 in the main fixture are explicitly API-seeded; a second fresh namespace separately runs the first two source level-completion events and original UI Buttons without seeded progression/history. " +
            "Ad callbacks are simulated locally, never a live ad or payment service. Captures are 1080x1920 no-notch portrait diagnostics " +
            "of actual instances with safe-area adapters temporarily paused; they are not phone screenshots, physical viewport captures, or 1:1 pixel-parity evidence.";
        public string limitations = "Does not verify physical touch, the Apple game board, or actual host A/B switching. The fresh first-two-level case reports completed gameplay through the real facade OnWin event rather than solving a board. " +
            "Source HUD is rendered on an isolated camera background, without replacement UI or artwork.";
        public int assertions, playFrames, pointerClicks, screenWidth, screenHeight;
        public int captureWidth, captureHeight;
        public string syntheticHistoryJson, syntheticOrderJson, syntheticWalletJson;
        public double elapsedSeconds;
        public List<string> cases = new List<string>();
        public List<string> apiSetups = new List<string>();
        public List<string> screenshots = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    static Framework2ReplicaPlayValidation()
    {
        if (SessionState.GetBool(ActiveKey, false)) Attach();
    }

    public static void Run()
    {
        if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Run in a separate batch Editor starting in Edit mode; this runner exits that process.");
        output = Environment.GetEnvironmentVariable("DJS_APPLE_F2_PLAY_VALIDATION_OUTPUT");
        if (string.IsNullOrWhiteSpace(output))
            output = Path.Combine(Application.dataPath, "../Docs/CommercializationAB/Framework2Replica/PlayValidation");
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(output);
        SessionState.SetString(OutputKey, output);
        SessionState.SetString(StartKey, DateTime.UtcNow.ToString("O"));
        SessionState.SetString(DeadlineKey, DateTime.UtcNow.AddSeconds(MaximumSeconds).ToString("O"));
        SessionState.SetBool(ActiveKey, true);
        Attach();
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static void Attach()
    {
        EditorApplication.update -= Watchdog;
        EditorApplication.update += Watchdog;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        EditorApplication.playModeStateChanged += OnPlayMode;
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;
    }

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(ActiveKey, false)) InstallLoop();
        if (!finishing && started && state == PlayModeStateChange.ExitingPlayMode)
            Finish(new InvalidOperationException("Play mode stopped before the smoke validation completed."));
    }

    private static void Watchdog()
    {
        if (finishing || !SessionState.GetBool(ActiveKey, false)) return;
        if (!DateTime.TryParse(SessionState.GetString(DeadlineKey, ""), null,
            System.Globalization.DateTimeStyles.RoundtripKind, out DateTime deadline) || DateTime.UtcNow > deadline)
            Finish(new TimeoutException("Framework 2 Play validation exceeded 120 wall-clock seconds."));
    }

    private static void InstallLoop()
    {
        if (installed) return;
        originalLoop = PlayerLoop.GetCurrentPlayerLoop();
        var loop = originalLoop;
        for (int i = 0; i < loop.subSystemList.Length; i++)
        {
            if (loop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.Update)) continue;
            var update = loop.subSystemList[i];
            var systems = update.subSystemList;
            Array.Resize(ref systems, systems.Length + 1);
            systems[systems.Length - 1] = new PlayerLoopSystem { type = typeof(Framework2ReplicaPlayValidation), updateDelegate = TickFrame };
            update.subSystemList = systems;
            loop.subSystemList[i] = update;
            PlayerLoop.SetPlayerLoop(loop);
            installed = true;
            return;
        }
        Finish(new InvalidOperationException("Unity player Update loop is unavailable."));
    }

    private static void TickFrame()
    {
        if (finishing || !EditorApplication.isPlaying || Time.frameCount == lastFrame) return;
        try
        {
            if (!started)
            {
                started = true;
                EnsureReport();
                var scene = SceneManager.GetActiveScene();
                Require(SceneManager.sceneCount == 1 && scene.rootCount == 0 && string.IsNullOrEmpty(scene.path),
                    "Fixture starts in an empty, unsaved scene.");
                fixtureRoot = new GameObject("Framework2 isolated validation fixture");
                events = new GameObject("Validation EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
                events.transform.SetParent(fixtureRoot.transform, false);
                var camera = new GameObject("Validation Camera", typeof(Camera)).GetComponent<Camera>();
                camera.transform.SetParent(fixtureRoot.transform, false);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.14f, .17f, .2f);
                Routines.Push(Cases());
            }
            lastFrame = Time.frameCount;
            report.playFrames++;
            runtime?.Tick(Time.unscaledDeltaTime);
            while (Routines.Count > 0)
            {
                IEnumerator iterator = Routines.Peek();
                if (!iterator.MoveNext()) { (iterator as IDisposable)?.Dispose(); Routines.Pop(); continue; }
                if (iterator.Current is IEnumerator nested) { Routines.Push(nested); continue; }
                if (iterator.Current != null) throw new InvalidOperationException("Fixture supports only nested iterators and frame yields.");
                return;
            }
            Finish(null);
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static IEnumerator Cases()
    {
        CheckJsonLibraryRoundTrips();
        Task<Framework2ProductConfig> configTask = AppleF2Runtime.LoadProductConfigAsync();
        yield return Await(configTask, "Original configuration load");
        Framework2ProductConfig config = configTask.Result;
        Task<AppleF2Runtime> create = Create(config);
        yield return Await(create, "Original runtime and prefab initialization", 35);
        runtime = create.Result;
        Require(runtime.Ready, "Production facade initializes successfully.");
        var persistedFixture = new List<HistoryModel>
        {
            new HistoryModel { ID = "qa-local-only", DateTime = "10/23/2026", Stage = 2, Status = 2, Money = .1 }
        };
        const string qaRoundTripKey = "Validation.HistoryRoundTrip";
        try
        {
            SPlayerPrefs.SetListTem(qaRoundTripKey, persistedFixture);
            List<HistoryModel> restoredFixture = SPlayerPrefs.GetListTem<HistoryModel>(qaRoundTripKey);
            Require(restoredFixture != null && restoredFixture.Count == 1 && restoredFixture[0].ID == "qa-local-only" &&
                restoredFixture[0].Stage == 2 && Near(restoredFixture[0].Money, .1),
                "Production SPlayerPrefs JSON round-trip works inside the isolated validation namespace.");
        }
        finally { SPlayerPrefs.Delete(qaRoundTripKey); SPlayerPrefs.Save(); }
        yield return Frames(3);
        CheckSingleHud();
        var gm = GameManagerWZ.instance;
        Require(gm != null && gm.currentLv == 1 && gm.currentStage == 1, "Unique namespace starts at source level and stage 1.");
        int[] types = { 1, 1, 4, 2, 2 };
        int[] targets = { 1, 2, 3, 200, 3000 };
        for (int i = 0; i < targets.Length; i++)
        {
            Require(gm.TryGetStageRequirement(i + 1, out int type, out int target) && type == types[i] && target == targets[i],
                "Original task type and target preserved for stage " + (i + 1) + ".");
            Require(gm.ShouldAutoEnterNextStage(i + 1) == (i != 2), "Original automatic/manual progression preserved for stage " + (i + 1) + ".");
        }
        int oldExp = UserDataManager.Instance.GetExp();
        double initialCoin = gm.GetCoin();
        runtime.OnBegin(99);
        Require(gm.currentLv == 1, "OnBegin does not overwrite source progression with the Apple level argument.");
        runtime.OnTriple();
        yield return Frames(2);
        Require(UserDataManager.Instance.GetExp() == oldExp + 2, "A fruit triple reaches the source pair-reward experience hook.");
        Require(Near(gm.GetCoin(), initialCoin), "Ordinary coin reward stays disabled in the first two source levels.");
        yield return Capture("01-source-hud.png");
        report.cases.Add("production initialization, one HUD, five source task requirements, OnBegin and OnTriple");

        // Deliberately seed a task boundary; no assertion claims the skipped tutorials were played.
        gm.currentStage = 3;
        gm.maxUnlockedStage = 3;
        gm.SaveCurrentStage(); gm.SaveMaxUnlockedStage();
        gm.SetHostGameplayLevel(4);
        gm.currentCoin = .1f; gm.SaveCoinData();
        // HandleCompletedStage(2) normally performs both operations before stage 3.
        // Use those same source methods to create a complete boundary fixture;
        // an empty history is not a valid state reached by normal stage 2 play.
        Require(gm.GameScene != null, "Source GameScene history writer is initialized.");
        string stage2HistoryId = gm.GameScene.setHistory(gm.GetCoin(), 2);
        WithdrawApiService.EnsureQueuedFirstAdOrder(gm.GetCoin());
        List<HistoryModel> stage2History = SPlayerPrefs.GetListTem<HistoryModel>(FacadePlayerPrefExtend.History);
        Require(!string.IsNullOrEmpty(stage2HistoryId) &&
            stage2History != null && stage2History.Count == 1,
            "Stage 3 fixture contains the source stage 2 pending history and queued local order.");
        Require(gm.SetStageAdWatchProgressForTesting(0), "Source testing API can reset stage 3 ad progress.");
        gm.SavePlayerEmail("test@example.invalid");
        gm.CompleteHostStageTutorial();
        report.apiSetups.Add("Seed stage 3 / level 4 / balance 0.1 / zero ads / dummy email, with original GameScene.setHistory and EnsureQueuedFirstAdOrder side effects from completed stage 2; first two tutorials are not exercised.");
        int rewardedCalls = 0;
        AdsControl.Instance.ShowRewardedAd("validation-failure", success => rewardedCalls++, false);
        Action<bool> failed = TakeAd(); failed(false); failed(true);
        Require(gm.currentStageAdWatchCount == 0 && rewardedCalls == 1 && Near(gm.GetCoin(), .1),
            "Failed ad callback grants nothing; a duplicate callback is ignored.");
        for (int i = 1; i <= 3; i++)
        {
            AdsControl.Instance.ShowRewardedAd("validation-stage3", success => rewardedCalls++, false);
            Action<bool> success = TakeAd(); success(true); success(true);
            yield return Frames(1);
            Require(gm.currentStageAdWatchCount == i && gm.currentStage == 3,
                "Successful simulated ad " + i + " counts exactly once and does not auto-advance manual stage 3.");
        }
        Require(Near(gm.GetCoin(), .3), "Only the first two of the three stage ads add the original 0.1 coin reward.");
        Task<BaseView> missionTask = UIManager.instance.ShowViewAsync(EUIType.WithdrawMissionView);
        yield return Await(missionTask, "Source mission view");
        yield return Frames(5);
        yield return Capture("02-source-ad-mission.png");
        Click(FindButton(missionTask.Result.transform, "GenerateMoreBtn", "continuebtn", "continue"));
        yield return Until(() => UIManager.instance.HasActiveView(EUIType.WithdrawFeedbackView), "Local withdrawal feedback", 12);
        var order = WithdrawApiService.GetLatestOrderForTesting();
        Require(order != null && order.OrderStatus == 3 && string.IsNullOrEmpty(order.SuccessTime),
            "The submitted local order stays under review with no paid/success timestamp.");
        Require(gm.currentStage == 3, "Local submission alone does not skip the feedback-confirmation step.");
        yield return Frames(4);
        yield return Capture("03-source-local-review.png");
        WithdrawFeedbackView feedback = UIManager.instance.GetView<WithdrawFeedbackView>(EUIType.WithdrawFeedbackView);
        Click(FindButton(feedback.transform, "GenerateMoreBtn", "continuebtn", "close"));
        yield return Until(() => gm.currentStage == 4, "Source manual stage 3 confirmation");
        yield return Until(() => UIManager.instance.HasActiveView(EUIType.SlotView), "Original stage 4 slot presentation");
        var slot = UIManager.instance.GetView<SlotView>(EUIType.SlotView);
        yield return Until(() => slot.DrawButton != null && slot.DrawButton.isActiveAndEnabled && slot.DrawButton.IsInteractable(),
            "Original slot draw Button");
        yield return Capture("04-source-stage4-slot.png");
        double beforeSlot = gm.GetCoin();
        Click(slot.DrawButton);
        yield return Until(() => UIManager.instance.HasActiveView(EUIType.ProgressView), "Original slot reward and progress view", 20);
        Require(Near(gm.GetCoin(), beforeSlot + 50), "Source stage 4 slot grants exactly the configured 50 coins.");
        yield return Frames(4);
        yield return Capture("05-source-stage4-progress.png");
        var progress = UIManager.instance.GetView<ProgressView>(EUIType.ProgressView);
        Click(FindButton(progress.transform, "continuebtn", "close"));
        yield return Until(() => !gm.HasHostPendingFlow && !UIManager.instance.HasActiveView(EUIType.ProgressView),
            "Source slot flow closes completely");
        report.cases.Add("failed/duplicate ad callbacks, three successful ads, original mission Button, local review, feedback Button advances stage 4");
        report.cases.Add("original stage 4 slot draw Button, 50 reward, progress confirmation Button");

        gm.currentStage = 5; gm.maxUnlockedStage = 5;
        gm.SaveCurrentStage(); gm.SaveMaxUnlockedStage();
        gm.AddCoin(100000);
        Require(Near(gm.GetCoin(), 2999.5), "Ordinary rewards retain the exact original 2999.5 cap.");
        Require(!gm.IsStageCompleted(5), "The original 3000 final target is not reachable from the 2999.5 ordinary reward cap.");
        report.apiSetups.Add("Seed stage 5 to exercise ordinary reward cap, not a claimed natural route to the final stage.");
        report.cases.Add("ordinary reward cap 2999.5 remains below final target 3000");

        gm.currentCoin = 123.45f; gm.SaveCoinData();
        ChallangeSuccessView.ResetLuckySpinAdProgressForTesting();
        gm.CompleteHostStageTutorial();
        runtime.OnBegin(4);
        int nextLevelCalls = 0;
        runtime.OnWin(() => nextLevelCalls++);
        runtime.OnWin(() => nextLevelCalls++);
        yield return Until(() => UIManager.instance.HasActiveView(EUIType.ChallangeSuccessView), "Original settlement view", 15);
        var settlement = UIManager.instance.GetView<ChallangeSuccessView>(EUIType.ChallangeSuccessView);
        Button only = null;
        yield return Until(() => (only = FindButtonOrNull(settlement.transform, "OnlyBtn")) != null && only.isActiveAndEnabled && only.IsInteractable(),
            "Original delayed non-ad reward Button", 10);
        yield return Capture("06-source-settlement.png");
        Click(only);
        yield return Until(() => nextLevelCalls == 1, "Exactly one source next-level callback", 20);
        yield return Frames(8);
        Require(nextLevelCalls == 1 && gm.currentLv == 5 && !UIManager.instance.HasActiveView(EUIType.ChallangeSuccessView),
            "Duplicate OnWin calls produce one settlement, one next-level callback and one level increment.");
        report.apiSetups.Add("Reset source lucky-spin test counter and seed balance 123.45 before level 4 completion.");
        report.cases.Add("original settlement non-ad Button, duplicate OnWin guard, one next-level callback");

        int staleCompletion = 0;
        AdsControl.Instance.ShowRewardedAd("validation-stale", success => staleCompletion++, false);
        Action<bool> stale = TakeAd();
        runtime.Save();
        double savedCoin = gm.GetCoin();
        int savedStage = gm.currentStage, savedLevel = gm.currentLv;
        int savedOrders = WithdrawApiService.GetCachedOrders().Count;
        runtime.Dispose(); runtime = null;
        yield return Frames(3);
        create = Create(config);
        yield return Await(create, "Recreate same isolated namespace", 35);
        runtime = create.Result;
        yield return Frames(3);
        gm = GameManagerWZ.instance;
        CheckSingleHud();
        Require(gm.currentStage == savedStage && gm.currentLv == savedLevel && Near(gm.GetCoin(), savedCoin),
            "Recreating the facade restores stage, level and balance from the isolated production save path.");
        Require(WithdrawApiService.GetCachedOrders().Count == savedOrders, "Local order history survives runtime recreation.");
        int adsBeforeStale = gm.currentStageAdWatchCount;
        stale(true); stale(false);
        yield return Frames(2);
        Require(staleCompletion == 0 && gm.currentStageAdWatchCount == adsBeforeStale && Near(gm.GetCoin(), savedCoin),
            "A callback owned by the disposed runtime cannot mutate its replacement.");
        runtime.Dispose(); runtime = null;
        yield return Frames(3);
        Require(Object.FindObjectsOfType<GamePlayerView>().Length == 0, "Dispose removes the source HUD without a duplicate remaining.");
        report.cases.Add("dispose/recreate persistence, one HUD, local order restoration, stale callback rejected");
        yield return FirstTwoLevels(config);
    }

    private static Task<AppleF2Runtime> Create(Framework2ProductConfig config, string storageNamespace = null) => AppleF2Runtime.CreateAsync(
        fixtureRoot.transform, config, (_, callback) => AdResponses.Enqueue(callback), (_, callback) => AdResponses.Enqueue(callback),
        () => true, () => new DateTime(2026, 10, 23, 12, 0, 0), () => "US", storageNamespace ?? report.storageNamespace);

    private static IEnumerator FirstTwoLevels(Framework2ProductConfig config)
    {
        report.firstTwoLevelsStorageNamespace = "DJS.Apple.F2Validation." + Guid.NewGuid().ToString("N") + ".FirstTwo.";
        Require(AdResponses.Count == 0, "Earlier fixtures left no unanswered ad callback before the fresh source tutorial case.");
        Task<AppleF2Runtime> create = Create(config, report.firstTwoLevelsStorageNamespace);
        yield return Await(create, "Fresh source first-two-level initialization", 35);
        runtime = create.Result;
        yield return Frames(3);
        CheckSingleHud();
        GameManagerWZ gm = GameManagerWZ.instance;
        Require(gm.currentLv == 1 && gm.currentStage == 1 && Near(gm.GetCoin(), 0) &&
            WithdrawApiService.GetCachedOrders().Count == 0 && string.IsNullOrEmpty(gm.getPlayerEmail()),
            "Fresh tutorial profile has original level 1/stage 1/zero balance, no account and no order.");
        List<HistoryModel> before = SPlayerPrefs.GetListTem<HistoryModel>(FacadePlayerPrefExtend.History);
        Require(before == null || before.Count == 0, "Fresh tutorial profile has no seeded stage history.");

        int completedLevels = 0;
        runtime.OnBegin(1);
        runtime.OnWin(() => completedLevels++);
        yield return WaitForTutorialStep(0);
        Require(gm.currentLv == 2 && gm.currentStage == 2 && completedLevels == 0 && gm.HasPendingHostStageTutorial,
            "First actual OnWin enters source stage 2 and retains its checkpoint while the source withdrawal guide is open.");
        Require(GameApp.viewManager.IsOpen((int)ViewType.WithDraw), "First guide targets the actual source WithDraw view.");
        yield return Capture("07-first-level-source-guide.png");
        ClickTutorialButton();
        yield return Until(() => GameApp.viewManager.IsOpen((int)ViewType.WithDrawMethod), "Source receiver form after first guide", 15);
        WithDrawMethod method = GameApp.viewManager.GetView<WithDrawMethod>((int)ViewType.WithDrawMethod);
        InputField receiver = method.Find<InputField>("PanelA/PanelC/Email");
        Require(receiver != null && receiver.isActiveAndEnabled && receiver.interactable,
            "Original source account form has an active standard InputField.");
        receiver.text = "test@example.invalid";
        yield return Frames(2);
        Click(FindButton(method.transform, "Confirm"));
        yield return Until(() => GameApp.viewManager.IsOpen((int)ViewType.WithDrawConfirm), "Original dummy-account confirmation", 15);
        WithDrawConfirm confirm = GameApp.viewManager.GetView<WithDrawConfirm>((int)ViewType.WithDrawConfirm);
        Require(gm.getPlayerEmail() == "test@example.invalid", "Source form validation and Confirm persist the fictitious account.");
        Click(FindButton(confirm.transform, "Confirm"));
        yield return Until(() => completedLevels == 1 && !runtime.HasPendingGameplayPresentation, "First source tutorial completion callback", 15);
        Require(gm.currentLv == 2 && gm.currentStage == 2 && !gm.HasPendingHostStageTutorial && Near(gm.GetCoin(), 0),
            "First guide completes once, clears its checkpoint and grants no first-level cash.");
        Require(WithdrawApiService.GetCachedOrders().Count == 0 && !UIManager.instance.HasActiveView(EUIType.ChallangeSuccessView),
            "Original CloseInt path does not add an ordinary settlement or a fabricated first-level order.");

        runtime.OnBegin(2);
        runtime.OnWin(() => completedLevels++);
        yield return WaitForTutorialStep(3);
        Require(gm.currentLv == 3 && gm.currentStage == 3 && completedLevels == 1 && Near(gm.GetCoin(), .1),
            "Second actual OnWin naturally enters stage 3 and grants the original 0.1 amount.");
        List<HistoryModel> naturalHistory = SPlayerPrefs.GetListTem<HistoryModel>(FacadePlayerPrefExtend.History);
        Require(naturalHistory != null && naturalHistory.Count == 1 && naturalHistory[0].Stage == 2 &&
            naturalHistory[0].Status == 2 && Near(naturalHistory[0].Money, .1),
            "Actual source stage 2 completion creates exactly one pending 0.1 history entry without fixture setup.");
        List<WithdrawOrderData> naturalOrders = WithdrawApiService.GetCachedOrders();
        Require(naturalOrders.Count == 1 && naturalOrders[0].ClientQueuedPlaceholder && naturalOrders[0].OrderStatus == 0 &&
            Near(naturalOrders[0].GetAmountValue(), .1) && string.IsNullOrEmpty(naturalOrders[0].SuccessTime),
            "Actual source stage 2 completion creates one local queued placeholder, never a paid result.");
        string naturalOrderId = naturalOrders[0].OrderNo;
        ClickTutorialButton();
        yield return WaitForTutorialStep(0);
        Require(GameApp.viewManager.IsOpen((int)ViewType.WithDraw), "The second CoinBoard guide opens the original withdrawal view and next guide step.");
        ClickTutorialButton();
        yield return Until(() => GameApp.viewManager.IsOpen((int)ViewType.WithDrawProcess1), "Source verification flow after second guide", 15);
        WithDrawProcess1 process = GameApp.viewManager.GetView<WithDrawProcess1>((int)ViewType.WithDrawProcess1);
        Button continueButton = null;
        yield return Until(() => (continueButton = FindButtonOrNull(process.transform, "Continue")) != null,
            "Original timed source verification Continue", 15);
        yield return Capture("08-second-level-source-verification.png");
        Click(continueButton);
        yield return Until(() => completedLevels == 2 && !runtime.HasPendingGameplayPresentation, "Second source tutorial completion callback", 15);
        yield return Frames(4);
        Require(gm.currentLv == 3 && gm.currentStage == 3 && gm.currentStageAdWatchCount == 0 &&
            !gm.HasPendingHostStageTutorial && completedLevels == 2 && Near(gm.GetCoin(), .1),
            "Both natural source guides finish once at level 3/stage 3, with no seeded ads or duplicate reward.");
        naturalOrders = WithdrawApiService.GetCachedOrders();
        Require(naturalOrders.Count == 1 && naturalOrders[0].OrderNo == naturalOrderId && naturalOrders[0].ClientQueuedPlaceholder &&
            naturalOrders[0].OrderStatus == 0 && string.IsNullOrEmpty(naturalOrders[0].SuccessTime),
            "Original verification retains the same unpaid local queue record; no actual transfer is claimed.");
        Require(AdResponses.Count == 0 && !UIManager.instance.HasActiveView(EUIType.ChallangeSuccessView),
            "No advertisement or additional ordinary settlement is invented for the first two source levels.");
        report.firstTwoLevelsCompletedThroughSourceButtons = true;
        report.cases.Add("fresh namespace: two real OnWin events, original guide Buttons, dummy-account InputField/Confirm, original verification timer/Continue, naturally created 0.1 pending history and local queue");
        runtime.Dispose(); runtime = null;
        yield return Frames(3);
        Require(Object.FindObjectsOfType<GamePlayerView>().Length == 0, "Fresh tutorial runtime also releases its source HUD.");
    }

    private static IEnumerator WaitForTutorialStep(int step)
    {
        yield return Until(() => Tutorial.instance != null && UIManager.instance.HasActiveView(EUIType.Tutorial) &&
            Tutorial.instance.currentType == Tutorial.TYPE.TYPE3 && Tutorial.instance.step == step &&
            Tutorial.instance.ClickPos != null && Tutorial.instance.ClickPos.activeInHierarchy &&
            Tutorial.instance.ClickPos.GetComponent<Button>() != null && Tutorial.instance.ClickPos.GetComponent<Button>().IsInteractable(),
            "Original TYPE3 guide step " + step, 15);
        yield return Frames(2);
    }

    private static void ClickTutorialButton()
    {
        Require(Tutorial.instance != null && Tutorial.instance.ClickPos != null, "Source guide has its configured click Button.");
        Click(Tutorial.instance.ClickPos.GetComponent<Button>());
    }

    private static void CheckJsonLibraryRoundTrips()
    {
        // These strings contain fixed QA-only values, never the actual player's data.
        var history = new List<HistoryModel>
        {
            new HistoryModel { ID = "qa-local-only", DateTime = "10/23/2026", Stage = 2, Status = 2, Money = .1 }
        };
        report.syntheticHistoryJson = JsonConvert.SerializeObject(history);
        Require(!string.IsNullOrEmpty(report.syntheticHistoryJson) && report.syntheticHistoryJson.Contains("\"Money\"") &&
            report.syntheticHistoryJson.Contains("qa-local-only"),
            "JSON library serializes the source HistoryModel list to nonempty JSON with stable source property names.");
        List<HistoryModel> decodedHistory = JsonConvert.DeserializeObject<List<HistoryModel>>(report.syntheticHistoryJson);
        Require(decodedHistory != null && decodedHistory.Count == 1 && decodedHistory[0].ID == "qa-local-only" &&
            decodedHistory[0].DateTime == "10/23/2026" && decodedHistory[0].Stage == 2 && decodedHistory[0].Status == 2 &&
            Near(decodedHistory[0].Money, .1), "JSON library restores every source HistoryModel property.");

        var orders = new List<WithdrawOrderData>
        {
            new WithdrawOrderData { OrderNo = "qa-order-only", WithdrawType = "qa-simulation", OrderStatus = 3,
                Receiver = "test@example.invalid", Amount = "0.30", Currency = "USD", ClientQueuedPlaceholder = true,
                LocalStatusText = "Local test review", ClientSortAnchorTime = "2026-10-23T12:00:00Z" }
        };
        report.syntheticOrderJson = JsonConvert.SerializeObject(orders);
        Require(!string.IsNullOrEmpty(report.syntheticOrderJson) && report.syntheticOrderJson.Contains("\"orderNo\"") &&
            report.syntheticOrderJson.Contains("\"clientQueuedPlaceholder\""), "JSON library honors source order JsonProperty wire names.");
        List<WithdrawOrderData> decodedOrders = JsonConvert.DeserializeObject<List<WithdrawOrderData>>(report.syntheticOrderJson);
        Require(decodedOrders != null && decodedOrders.Count == 1 && decodedOrders[0].OrderNo == "qa-order-only" &&
            decodedOrders[0].OrderStatus == 3 && decodedOrders[0].ClientQueuedPlaceholder && decodedOrders[0].Amount == "0.30" &&
            decodedOrders[0].Receiver == "test@example.invalid" && decodedOrders[0].LocalStatusText == "Local test review" &&
            string.IsNullOrEmpty(decodedOrders[0].SuccessTime), "JSON order round-trip preserves local review without a payment timestamp.");

        var wallet = new UserWalletData { Id = 7, Balance = 12.34, FrozenBalance = .3, Currency = "USD", Status = 1,
            PayPalAccountExists = 0, LuckySuccessRewardAmount = 20.5, LuckySpinRewardAmount = null };
        report.syntheticWalletJson = JsonConvert.SerializeObject(wallet);
        Require(!string.IsNullOrEmpty(report.syntheticWalletJson) && report.syntheticWalletJson.Contains("\"balance\""),
            "JSON wallet serializer returns actual JSON under source wire names.");
        UserWalletData decodedWallet = JsonConvert.DeserializeObject<UserWalletData>(report.syntheticWalletJson);
        Require(decodedWallet != null && decodedWallet.Id == 7 && Near(decodedWallet.Balance, 12.34) &&
            Near(decodedWallet.FrozenBalance, .3) && decodedWallet.Currency == "USD" && decodedWallet.PayPalAccountExists == 0 &&
            decodedWallet.LuckySuccessRewardAmount == 20.5 && !decodedWallet.LuckySpinRewardAmount.HasValue,
            "JSON wallet round-trip preserves amounts and nullable DTO fields.");
        report.cases.Add("real JSON serializer: synthetic source history, order and wallet DTO round-trips");
    }

    private static Action<bool> TakeAd()
    {
        Require(AdResponses.Count == 1, "The production adapter dispatched exactly one local ad request.");
        return AdResponses.Dequeue();
    }
    private static void CheckSingleHud()
    {
        GamePlayerView[] hud = Object.FindObjectsOfType<GamePlayerView>();
        Require(hud.Length == 1 && UIManager.instance.HasActiveView(EUIType.GamePlayerView), "Exactly one source gameplay HUD is active.");
        Require(hud[0].GetComponentsInChildren<Button>(true).Length > 0, "Source HUD retains standard Unity Buttons.");
    }
    private static Button FindButtonOrNull(Transform root, params string[] names)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (string name in names)
            foreach (Button button in buttons)
                if (button.name == name && button.isActiveAndEnabled && button.IsInteractable()) return button;
        return null;
    }
    private static Button FindButton(Transform root, params string[] names)
    {
        Button result = FindButtonOrNull(root, names);
        Require(result != null, "The source view exposes an active standard Button: " + string.Join(" / ", names));
        return result;
    }
    private static void Click(Button button)
    {
        Require(button != null && button.isActiveAndEnabled && button.IsInteractable(), "Clicked control is an active standard Unity Button.");
        var pointer = new PointerEventData(events) { button = PointerEventData.InputButton.Left, clickCount = 1 };
        // This is Button event dispatch, explicitly not an assertion about hardware input or raycast visibility.
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        report.pointerClicks++;
    }
    private static IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }
    private static IEnumerator Until(Func<bool> condition, string operation, double seconds = 10)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(seconds);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException(operation + " timed out.");
            yield return null;
        }
    }
    private static IEnumerator Await(Task task, string operation, double seconds = 20)
    {
        yield return Until(() => task.IsCompleted, operation, seconds);
        if (task.IsFaulted) throw task.Exception ?? new Exception(operation + " failed.");
        if (task.IsCanceled) throw new OperationCanceledException(operation);
    }
    private static IEnumerator Capture(string name)
    {
        Canvas.ForceUpdateCanvases();
        yield return Frames(2);
        string path = Path.Combine(output, name);
        // Batch Editor can run player frames without a presented GameView frame,
        // so ScreenCapture's asynchronous file may never arrive. Capture existing
        // instances directly and never let missing image output skip logic checks.
        report.captureWidth = 1080;
        report.captureHeight = 1920;
        if (Framework2ActualCapture.TryCapturePortrait(path, out string error))
            report.screenshots.Add(path);
        else report.warnings.Add("Actual viewport capture unavailable; logic validation continued: " + name + " / " + error);
        yield return Frames(2);
    }
    private static bool Near(double a, double b) => Math.Abs(a - b) < .005;
    private static void Require(bool passed, string message)
    {
        EnsureReport(); report.assertions++;
        if (!passed) throw new InvalidOperationException(message);
    }
    private static void EnsureReport()
    {
        if (report != null) return;
        output = SessionState.GetString(OutputKey, Path.Combine(Application.dataPath, "../F2PlayValidation"));
        report = new Report { generatedUtc = DateTime.UtcNow.ToString("O"), storageNamespace = "DJS.Apple.F2Validation." + Guid.NewGuid().ToString("N") + ".",
            screenWidth = Screen.width, screenHeight = Screen.height };
    }
    private static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (finishing) return;
        EnsureReport();
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            report.errors.Add(condition + "\n" + stackTrace);
        else if (type == LogType.Warning && report.warnings.Count < 100) report.warnings.Add(condition);
    }
    private static void Finish(Exception exception)
    {
        if (finishing) return;
        finishing = true;
        EnsureReport();
        if (exception != null) report.errors.Add(exception.ToString());
        try { runtime?.Dispose(); runtime = null; }
        catch (Exception cleanup) { report.errors.Add("Dispose: " + cleanup); }
        if (installed) PlayerLoop.SetPlayerLoop(originalLoop);
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Watchdog;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        Application.logMessageReceived -= OnLog;
        if (DateTime.TryParse(SessionState.GetString(StartKey, ""), null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime start))
            report.elapsedSeconds = (DateTime.UtcNow - start).TotalSeconds;
        report.passed = exception == null && report.errors.Count == 0;
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "framework2-play-validation.json"), JsonUtility.ToJson(report, true));
        Debug.Log("Framework2 Play validation " + (report.passed ? "PASS" : "FAIL") + ": " + output);
        EditorApplication.Exit(report.passed ? 0 : 1);
    }
}
