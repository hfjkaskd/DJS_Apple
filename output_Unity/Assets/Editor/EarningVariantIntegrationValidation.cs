using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using Apple.Framework2;
using CorePlay;
using FruitApkEarning;
using FruitEarningVariants;
using MahjongSlide.Withdrawal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Runs the actual Game scene and host buttons with isolated production-code storage.</summary>
[InitializeOnLoad]
public static class EarningVariantIntegrationValidation
{
    private const string ActiveKey = "Apple.EarningVariantIntegration.Active";
    private const string OutputKey = "Apple.EarningVariantIntegration.Output";
    private const string DeadlineKey = "Apple.EarningVariantIntegration.Deadline";
    private const string StartedKey = "Apple.EarningVariantIntegration.Started";
    private const string ProfileKey = "Apple.EarningVariantIntegration.Profile";
    private const string SettlementKey = "Apple.EarningVariantIntegration.Settlement";
    private const string GamePath = "Assets/Game.unity";
    private const double TimeoutSeconds = 180;
    private static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
    private static Report report;
    private static string output;
    private static bool started, finishing, installed, capturing;
    private static int lastFrame = -1;
    private static PlayerLoopSystem originalLoop;
    private static Dictionary<string, string> productionFiles;
    private static int productionSelection;

    [Serializable] private sealed class Snapshot
    {
        public string name, activeVariant;
        public int controllerInstanceId, gameplayLevel, runtimeLevel, stage;
        public long aCashCents, aDiamonds, bBalanceMinorUnits;
        public bool aHudActive, aSettingsActive, bRuntimeReady, testDisclosureVisible, bBackdropActive;
        public int controllerCount, bRuntimeCount;
    }

    [Serializable] private sealed class Report
    {
        public string generatedUtc, profile, saveDirectory;
        public string scope = "Actual Assets/Game.unity in Unity Play mode. A -> B -> A -> B via the saved host's standard Button.onClick handlers. " +
            "All game/A/B storage uses a dedicated QA profile. No fruit is clicked and no level is completed. " +
            "The B balance marker is explicit test setup using original GameManagerWZ.AddCoin(12.34, rewardLevel: 3), not earned gameplay. " +
            "Portrait screenshots render the actual live scene instances and gameplay camera into a temporary RenderTexture, restoring camera/canvas state afterwards. " +
            "They are no-notch diagnostic actual-instance renders with safe-area adapters temporarily paused, not phone captures, viewport/pixel parity proof, prefab fixtures or mockups. " +
            "The optional settlement scenario creates an explicitly labelled QA reward through the production service, without clearing the fruit board.";
        public bool passed, productionFilesUnchanged, productionSelectionUnchanged, includeSettlement, pendingSettlementRestoredOnce;
        public int assertions, playFrames, switchCount, screenWidth, screenHeight, captureWidth, captureHeight;
        public double elapsedSeconds;
        public List<Snapshot> states = new List<Snapshot>();
        public List<string> checks = new List<string>();
        public List<string> clicks = new List<string>();
        public List<string> screenshots = new List<string>();
        public List<CaptureDiagnostic> captureDiagnostics = new List<CaptureDiagnostic>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    [Serializable] private sealed class CanvasDiagnostic
    {
        public string name, camera;
        public int renderMode, sortingOrder;
        public float scaleFactor, planeDistance;
        public Vector2 rectSize;
        public Vector3 position, scale;
    }

    [Serializable] private sealed class CaptureDiagnostic
    {
        public string screenshot, phase, backgroundSprite, backgroundTexture;
        public bool backgroundReady, coreIsOpening, inputLocked;
        public int fruitChildCount, liveFruitCount;
        public double waitedSeconds;
        public Color backgroundColor;
        public Vector3 fieldPosition, fieldScale, trayPosition, trayScale;
        public List<CanvasDiagnostic> rootCanvases = new List<CanvasDiagnostic>();
    }

    static EarningVariantIntegrationValidation()
    {
        if (SessionState.GetBool(ActiveKey, false)) Attach();
    }

    public static void Run()
    {
        Start(false);
    }

    public static void RunWithSettlement()
    {
        Start(true);
    }

    private static void Start(bool includeSettlement)
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Run this smoke test in a separate batch Editor; it exits its Editor process.");
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Start host integration validation in Edit mode.");
        output = Environment.GetEnvironmentVariable("DJS_APPLE_VARIANT_VALIDATION_OUTPUT");
        if (string.IsNullOrWhiteSpace(output)) output = "E:/CodexCacheBackups/DJS_Apple-framework2-port-20261023/host-integration";
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(output);
        SessionState.SetString(OutputKey, output);
        SessionState.SetString(ProfileKey, "f2-host-check-" + Guid.NewGuid().ToString("N"));
        SessionState.SetBool(SettlementKey, includeSettlement);
        SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O"));
        SessionState.SetString(DeadlineKey, DateTime.UtcNow.AddSeconds(TimeoutSeconds).ToString("O"));
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
            Finish(new InvalidOperationException("Play mode exited before the host integration smoke test completed."));
    }

    private static void Watchdog()
    {
        if (finishing || !SessionState.GetBool(ActiveKey, false)) return;
        if (!DateTime.TryParse(SessionState.GetString(DeadlineKey, ""), null, DateTimeStyles.RoundtripKind, out DateTime deadline) || DateTime.UtcNow > deadline)
            Finish(new TimeoutException("Host integration validation exceeded its 180-second wall-clock limit."));
    }

    private static void InstallLoop()
    {
        if (installed) return;
        originalLoop = PlayerLoop.GetCurrentPlayerLoop();
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        for (int i = 0; i < loop.subSystemList.Length; i++)
        {
            if (loop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.Update)) continue;
            var update = loop.subSystemList[i];
            var systems = update.subSystemList;
            Array.Resize(ref systems, systems.Length + 1);
            systems[systems.Length - 1] = new PlayerLoopSystem { type = typeof(EarningVariantIntegrationValidation), updateDelegate = TickFrame };
            update.subSystemList = systems;
            loop.subSystemList[i] = update;
            PlayerLoop.SetPlayerLoop(loop);
            installed = true;
            return;
        }
        Finish(new InvalidOperationException("The player Update loop was not available."));
    }

    private static void TickFrame()
    {
        if (finishing || !EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
        try
        {
            lastFrame = Time.frameCount;
            if (!started)
            {
                started = true;
                EnsureReport();
                Require(string.IsNullOrEmpty(SceneManager.GetActiveScene().path) && EarningVariantController.Instance == null,
                    "The QA storage profile is selected in the empty scene before the gameplay host exists.");
                productionFiles = HashProductionFiles();
                productionSelection = PlayerPrefs.GetInt("Apple.EarningVariant.v1", -1);
                EarningVariantSelection.ConfigureStorageProfile(report.profile);
                EarningVariantSelection.SaveNextSelection(false);
                report.saveDirectory = EarningVariantSelection.SaveDirectory;
                Require(report.saveDirectory.IndexOf("TestProfiles", StringComparison.Ordinal) >= 0 &&
                    report.saveDirectory.EndsWith(report.profile, StringComparison.Ordinal), "Game/A storage uses the unique isolated QA directory.");
                Require(EarningVariantSelection.Framework2Namespace.Contains(report.profile), "B PlayerPrefs uses the same isolated QA namespace.");
                Screen.SetResolution(1080, 1920, FullScreenMode.Windowed);
                routines.Push(Cases());
            }
            report.playFrames++;
            while (routines.Count > 0)
            {
                var current = routines.Peek();
                if (!current.MoveNext())
                {
                    (current as IDisposable)?.Dispose();
                    routines.Pop();
                    continue;
                }
                if (current.Current is IEnumerator nested) { routines.Push(nested); continue; }
                if (current.Current != null) throw new InvalidOperationException("Unsupported test yield; use a frame or nested iterator.");
                return;
            }
            Finish(null);
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static IEnumerator Cases()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(GamePath, LoadSceneMode.Single);
        Require(load != null, "The original Game scene begins loading through SceneManager.");
        while (!load.isDone) yield return null;
        yield return WaitForHost(false, 0);
        yield return SettleFrames(6);
        var firstA = SnapshotState("A-initial", false);
        Require(ApkEarningService.Instance.Available, "A accounting initializes successfully inside the QA profile.");
        yield return Capture("01-A-initial.png");

        yield return Switch(true);
        var firstB = SnapshotState("B-initial", true);
        yield return Capture("02-B-initial.png");
        var manager = WZSDK.GameManagerWZ.instance;
        Require(manager != null, "B exposes the original source GameManager instance.");
        long beforeMarker = EarningVariantController.Instance.Runtime.BalanceMinorUnits;
        // Explicit fixture setup, not a level reward: original F2 forbids positive coin rewards for levels 1-2.
        manager.AddCoin(12.34f, 3);
        EarningVariantController.Instance.Runtime.Save();
        yield return SettleFrames(4);
        var markedB = SnapshotState("B-marker-fixture", true);
        Require(markedB.bBalanceMinorUnits == beforeMarker + 1234, "The explicit 12.34 test marker is saved by the original F2 money code.");
        Require(markedB.gameplayLevel == firstB.gameplayLevel && markedB.runtimeLevel == firstB.runtimeLevel && markedB.stage == firstB.stage,
            "The B fixture marker does not clear a game level or advance the task stage.");
        yield return Capture("03-B-marker-fixture.png");

        AppleF2Runtime disposedRuntime = EarningVariantController.Instance.Runtime;
        yield return Switch(false);
        Require(!disposedRuntime.Ready, "Switching away disposes the previous B runtime.");
        var restoredA = SnapshotState("A-restored", false);
        Require(restoredA.aCashCents == firstA.aCashCents && restoredA.aDiamonds == firstA.aDiamonds && restoredA.gameplayLevel == firstA.gameplayLevel,
            "A balances and level are unchanged after a B session.");
        yield return Capture("04-A-restored.png");

        yield return Switch(true);
        var restoredB = SnapshotState("B-restored", true);
        Require(restoredB.bBalanceMinorUnits == markedB.bBalanceMinorUnits && restoredB.gameplayLevel == markedB.gameplayLevel &&
            restoredB.runtimeLevel == markedB.runtimeLevel && restoredB.stage == markedB.stage,
            "Returning to B restores its independent balance marker, task stage and game level.");
        yield return Capture("05-B-restored.png");
        EarningVariantController.Instance.Runtime.Save();

        Require(report.switchCount == 3, "All three basic host-button reloads completed: A -> B -> A -> B.");
        if (report.includeSettlement) yield return PendingSettlementCases(markedB.bBalanceMinorUnits);

        report.productionFilesUnchanged = SameHashes(productionFiles, HashProductionFiles());
        report.productionSelectionUnchanged = PlayerPrefs.GetInt("Apple.EarningVariant.v1", -1) == productionSelection;
        Require(report.productionFilesUnchanged, "Production save files remain byte-identical outside the QA profile.");
        Require(report.productionSelectionUnchanged, "The production A/B selection preference remains unchanged.");
        Require(report.switchCount == (report.includeSettlement ? 8 : 3), "All requested real host-button scene reloads completed.");
    }

    private static IEnumerator PendingSettlementCases(long bMarker)
    {
        yield return Switch(false);
        yield return WaitUntil(() => !MCCIJBJGMCK.IsLock(), 15, "A UI remained locked before the settlement fixture.");
        var service = ApkEarningService.Instance;
        // Allow the real first-launch information page to finish via its own
        // authored button if the longer capture interval let it open naturally.
        if (ApkEarningHost.IsOpen)
        {
            var existingHost = Object.FindObjectOfType<ApkEarningHost>();
            var existingPage = existingHost == null ? null : existingHost.PageRoot.GetComponentInChildren<ApkEarningPageView>();
            if (existingPage != null && existingPage.name.IndexOf("TXRecordGuidePanel", StringComparison.Ordinal) >= 0)
            {
                Click(existingPage.GetButton("startButton"), "AcknowledgeExistingAWelcome");
                yield return WaitUntil(() => !ApkEarningHost.IsOpen, 10, "The real A welcome page did not close after its Continue button.");
            }
        }
        int levelBefore = JEFOMCDAPGK.Instance.CurMainLevelIndex;
        Require(service.Available && service.State.PendingRewards.Count == 0, "The isolated A session starts the settlement fixture with no pending rewards.");
        long cashBefore = service.State.CashCents, diamondsBefore = service.State.Diamonds;
        int recordsBefore = service.State.Records.Count, rewardVideosBefore = service.State.RewardVideos;
        string fixtureRun = "qa-pending-settlement-" + report.profile;
        Require(service.BeginRun(fixtureRun, 0, levelBefore), "Explicit QA setup creates an isolated A run checkpoint; the fruit board is not cleared.");
        Require(service.RecordWin(fixtureRun, levelBefore), "Explicit QA setup records one pending settlement using the production service.");
        Require(service.State.PendingRewards.Count == 1, "Exactly one QA settlement is pending before opening the real settlement page.");
        ApkReward pending = service.State.PendingRewards[0];
        long expectedCash = cashBefore + pending.CashCents, expectedDiamonds = diamondsBefore + pending.Diamonds;
        int expectedRecords = recordsBefore + (pending.EarlyOrdinal > 0 ? 1 : 0);
        int staleCompletionCalls = 0;
        ApkEarningHost.Open(false, true, () => staleCompletionCalls++);
        ApkEarningHost originalSettlementHost = null;
        yield return WaitUntil(() =>
        {
            if (!ApkEarningHost.IsOpen) return false;
            if (originalSettlementHost == null) originalSettlementHost = Object.FindObjectOfType<ApkEarningHost>();
            if (originalSettlementHost == null) return false;
            var page = originalSettlementHost.PageRoot.GetComponentInChildren<ApkEarningPageView>();
            return page != null && page.name.StartsWith("LocalSuccess", StringComparison.Ordinal);
        }, 15, "The real A settlement page did not open for the pending reward fixture.");
        yield return Capture("06-A-pending-settlement-fixture.png");
        yield return Switch(true);
        Require(originalSettlementHost == null, "The old real A settlement host is destroyed when the B scene takes over.");
        Require(EarningVariantController.Instance.Runtime.BalanceMinorUnits == bMarker, "B's independent balance survives the A pending-settlement switch.");
        yield return Switch(false);
        yield return SettleFrames(8);
        var restored = ApkEarningService.Instance;
        Require(restored.Available && restored.State.PendingRewards.Count == 0, "Returning to A resolves its already-recorded pending reward and leaves no pending queue.");
        Require(restored.State.CashCents == expectedCash && restored.State.Diamonds == expectedDiamonds && restored.State.Records.Count == expectedRecords,
            "A restored settlement is credited exactly once, with the expected single history record.");
        Require(restored.State.RewardVideos == rewardVideosBefore, "Recovering the recorded A settlement does not invent an ad completion.");
        Require(!ApkEarningHost.IsOpen && JEFOMCDAPGK.Instance.CurMainLevelIndex == levelBefore && staleCompletionCalls == 0,
            "The old settlement modal and its next-level callback do not resume after switching back to A.");
        yield return Capture("07-A-restored-settlement-fixture.png");
        // One further real reload proves the saved resolution cannot be credited a second time.
        yield return Switch(true);
        yield return Switch(false);
        restored = ApkEarningService.Instance;
        Require(restored.State.PendingRewards.Count == 0 && restored.State.CashCents == expectedCash && restored.State.Diamonds == expectedDiamonds &&
            restored.State.Records.Count == expectedRecords && restored.State.RewardVideos == rewardVideosBefore && staleCompletionCalls == 0, "A second reload preserves the resolved settlement without duplicate money, records, ads or stale callbacks.");
        SnapshotState("A-settlement-restored-twice", false);
        report.pendingSettlementRestoredOnce = true;
        yield return Capture("08-A-second-settlement-restore.png");
    }

    private static IEnumerator Switch(bool toB)
    {
        var previous = EarningVariantController.Instance;
        int previousId = previous.GetInstanceID();
        Click(previous.OpenSelectorButton, "OpenSelector");
        Require(previous.SelectorPanel.activeInHierarchy, "The saved selector panel opens through its standard Button handler.");
        yield return null;
        Click(toB ? previous.ChooseBButton : previous.ChooseAButton, toB ? "ChooseB" : "ChooseA");
        var selector=previous.GetComponent<Framework2AppleVariantSelector>();
        if(selector!=null)
        {
            Require(EarningVariantController.Instance==previous && EarningVariantSelection.IsB!=toB,
                "Choosing an Apple selector card keeps the current session until confirmation.");
            Click(selector.ConfirmButton,"Confirm selected variant");
        }
        yield return WaitForHost(toB, previousId);
        yield return SettleFrames(6);
        report.switchCount++;
    }

    private static IEnumerator WaitForHost(bool useB, int previousId)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 45;
        while (true)
        {
            var host = EarningVariantController.Instance;
            if (host != null && host.GetInstanceID() != previousId)
            {
                if (!string.IsNullOrEmpty(host.InitializationError)) throw new InvalidOperationException("Host initialization failed: " + host.InitializationError);
                if (host.Ready && EarningVariantSelection.IsB == useB && CorePlayUI.Instance != null && CorePlayUI.Instance.IsOpening)
                {
                    if (host.RulesPanel.activeSelf) Click(host.CloseRulesButton, "AcknowledgeSimulationRules");
                    Require(!host.RulesPanel.activeSelf, "The explicit simulation disclosure is acknowledged through its standard Button.");
                    break;
                }
            }
            if (Time.realtimeSinceStartupAsDouble > deadline)
                throw new TimeoutException("Waiting for " + (useB ? "B" : "A") + " gameplay host timed out after 45 seconds.");
            yield return null;
        }
    }

    private static Snapshot SnapshotState(string name, bool useB)
    {
        var host = EarningVariantController.Instance;
        var view = CorePlayUI.Instance.GetComponent<EarningVariantGameplayView>();
        Require(view != null && view.Owner == CorePlayUI.Instance, "The actual CorePlayUI uses its authored variant bindings.");
        var controllerInstances = Object.FindObjectsOfType<EarningVariantController>(true);
        var bInstances = Object.FindObjectsOfType<MahjongFramework2Runtime>(true);
        var a = ApkEarningService.Instance.State;
        var manager = WZSDK.GameManagerWZ.instance;
        var state = new Snapshot
        {
            name = name, activeVariant = useB ? "B" : "A", controllerInstanceId = host.GetInstanceID(),
            gameplayLevel = JEFOMCDAPGK.Instance.CurMainLevelIndex,
            runtimeLevel = host.Runtime == null ? 0 : host.Runtime.LinearLevelNumber,
            stage = useB && manager != null ? manager.currentStage : 0,
            aCashCents = a == null ? 0 : a.CashCents, aDiamonds = a == null ? 0 : a.Diamonds,
            bBalanceMinorUnits = host.Runtime == null ? 0 : host.Runtime.BalanceMinorUnits,
            aHudActive = view.ATopRoot.activeSelf, aSettingsActive = view.ALevelSettingsRow.activeSelf,
            bRuntimeReady = host.Runtime != null && host.Runtime.Ready,
            bBackdropActive = CorePlayUI.Instance.m_FieldBackdrop != null && CorePlayUI.Instance.m_FieldBackdrop.gameObject.activeSelf,
            controllerCount = controllerInstances.Length, bRuntimeCount = bInstances.Length,
            testDisclosureVisible = host.OpenSelectorButton.gameObject.activeInHierarchy && host.RulesButton.gameObject.activeInHierarchy
        };
        Require(state.controllerCount == 1, "Exactly one active scene controller exists in " + name + ".");
        Require(state.aHudActive == !useB && state.aSettingsActive == !useB, "Only A enables the original A HUD and level/settings row in " + name + ".");
        Require(state.bRuntimeReady == useB && state.bRuntimeCount == (useB ? 1 : 0), "Only B instantiates its original framework runtime in " + name + ".");
        Require(CorePlayUI.Instance.m_FieldBackdrop != null && state.bBackdropActive == useB && !CorePlayUI.Instance.m_FieldBackdrop.raycastTarget,
            "Only B enables the authored non-blocking full-screen map backdrop in " + name + ".");
        if (useB) Require(CorePlayUI.Instance.m_FieldBackdrop.sprite == CorePlayUI.Instance.m_BgImg.sprite,
            "B reuses the same loaded Sprite for its field and backdrop in " + name + ".");
        Require(state.testDisclosureVisible, "The global test disclosure and its buttons remain visible in " + name + ".");
        report.states.Add(state);
        return state;
    }

    private static void Click(Button button, string name)
    {
        Require(button != null && button.isActiveAndEnabled && button.interactable, "Standard Button is actionable: " + name);
        report.clicks.Add(name);
        button.onClick.Invoke();
    }

    private static IEnumerator SettleFrames(int count)
    {
        for (int i = 0; i < count; i++) yield return null;
    }

    private static IEnumerator WaitUntil(Func<bool> ready, double seconds, string error)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + seconds;
        while (!ready())
        {
            if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException(error);
            yield return null;
        }
    }

    private static IEnumerator Capture(string name)
    {
        // A batch Editor can run many frames before a real-time animation has
        // advanced. Wait for elapsed time and a loaded board, never hide effects.
        double startedWaiting = Time.realtimeSinceStartupAsDouble;
        while (Time.realtimeSinceStartupAsDouble - startedWaiting < 2.75) yield return null;
        double readyDeadline = Time.realtimeSinceStartupAsDouble + 10;
        while (!CaptureBoardReady() && Time.realtimeSinceStartupAsDouble < readyDeadline) yield return null;
        if (!CaptureBoardReady()) report.warnings.Add("Capture board readiness timed out for " + name + "; the diagnostic image still records the actual state.");
        double waited = Time.realtimeSinceStartupAsDouble - startedWaiting;
        report.captureDiagnostics.Add(ReadCaptureDiagnostic(name, "original-viewport", waited));
        report.screenWidth = Screen.width;
        report.screenHeight = Screen.height;
        report.captureWidth = 1080;
        report.captureHeight = 1920;
        capturing = true;
        try
        {
            Action fit = () =>
            {
                var ui = CorePlayUI.Instance;
                if (ui == null) return;
                var view = ui.GetComponent<EarningVariantGameplayView>();
                if (view != null) view.RefreshLayout();
                ui.FitGameField();
            };
            Action before = () => { fit(); report.captureDiagnostics.Add(ReadCaptureDiagnostic(name, "temporary-portrait-projection", waited)); };
            if (Framework2ActualCapture.TryCapturePortrait(Path.Combine(output, name), out string error, before, fit))
                report.screenshots.Add(name);
            else report.warnings.Add("Actual-instance portrait capture failed for " + name + ": " + error);
        }
        catch (Exception exception) { report.warnings.Add("Actual-instance portrait capture exception for " + name + ": " + exception); }
        finally { capturing = false; }
        yield return null;
    }

    private static bool CaptureBoardReady()
    {
        var ui = CorePlayUI.Instance;
        return ui != null && ui.IsOpening && ui.m_BgImg != null && ui.m_BgImg.sprite != null &&
            ui.m_BgImg.sprite.texture != null && ui.m_BgTrans != null &&
            EDLHEMMBABM.Instance.GetTotalItems().Count > 0 && !MCCIJBJGMCK.IsLock();
    }

    private static CaptureDiagnostic ReadCaptureDiagnostic(string screenshot, string phase, double waited)
    {
        var ui = CorePlayUI.Instance;
        var value = new CaptureDiagnostic
        {
            screenshot = screenshot, phase = phase, waitedSeconds = waited,
            coreIsOpening = ui != null && ui.IsOpening, inputLocked = MCCIJBJGMCK.IsLock(),
            liveFruitCount = EDLHEMMBABM.Instance.GetTotalItems().Count
        };
        if (ui != null)
        {
            if (ui.m_BgImg != null)
            {
                value.backgroundColor = ui.m_BgImg.color;
                var sprite = ui.m_BgImg.sprite;
                value.backgroundSprite = sprite == null ? "<null>" : sprite.name;
                value.backgroundTexture = sprite == null || sprite.texture == null ? "<null>" : sprite.texture.name;
                value.backgroundReady = sprite != null && sprite.texture != null;
            }
            if (ui.m_GameLayer != null) value.fruitChildCount = ui.m_GameLayer.childCount;
            if (ui.m_FieldPresentationRoot != null)
            {
                value.fieldPosition = ui.m_FieldPresentationRoot.localPosition;
                value.fieldScale = ui.m_FieldPresentationRoot.localScale;
            }
            if (ui.m_CollectArea != null)
            {
                value.trayPosition = ui.m_CollectArea.position;
                value.trayScale = ui.m_CollectArea.lossyScale;
            }
        }
        foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
        {
            if (!canvas.isActiveAndEnabled || !canvas.isRootCanvas) continue;
            var rect = canvas.transform as RectTransform;
            value.rootCanvases.Add(new CanvasDiagnostic
            {
                name = canvas.name, camera = canvas.worldCamera == null ? "<none>" : canvas.worldCamera.name,
                renderMode = (int)canvas.renderMode, sortingOrder = canvas.sortingOrder,
                scaleFactor = canvas.scaleFactor, planeDistance = canvas.planeDistance,
                rectSize = rect == null ? Vector2.zero : rect.rect.size,
                position = canvas.transform.position, scale = canvas.transform.lossyScale
            });
        }
        return value;
    }

    private static Dictionary<string, string> HashProductionFiles()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(Application.persistentDataPath)) return result;
        foreach (string path in Directory.GetFiles(Application.persistentDataPath, "*", SearchOption.TopDirectoryOnly))
        {
            string extension = Path.GetExtension(path);
            if (extension != ".json" && extension != ".dat" && extension != ".bak") continue;
            using (var hash = SHA256.Create())
            using (var stream = File.OpenRead(path))
                result.Add(Path.GetFileName(path), BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", ""));
        }
        return result;
    }

    private static bool SameHashes(Dictionary<string, string> before, Dictionary<string, string> after)
    {
        if (before.Count != after.Count) return false;
        foreach (var entry in before)
            if (!after.TryGetValue(entry.Key, out string value) || value != entry.Value) return false;
        return true;
    }

    private static void Require(bool condition, string message)
    {
        report.assertions++;
        if (!condition) throw new InvalidOperationException(message);
        report.checks.Add(message);
    }

    private static void EnsureReport()
    {
        if (report == null) report = new Report { generatedUtc = DateTime.UtcNow.ToString("O"), profile = SessionState.GetString(ProfileKey, ""), includeSettlement = SessionState.GetBool(SettlementKey, false) };
        if (string.IsNullOrEmpty(output)) output = SessionState.GetString(OutputKey, "");
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (!SessionState.GetBool(ActiveKey, false) || finishing) return;
        EnsureReport();
        if (capturing && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) report.warnings.Add("Portrait capture diagnostic: " + message + "\n" + stack);
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack);
        else if (type == LogType.Warning) report.warnings.Add(message);
    }

    private static void Finish(Exception failure)
    {
        if (finishing) return;
        finishing = true;
        EnsureReport();
        if (failure != null) report.errors.Add(failure.ToString());
        while (routines.Count > 0) (routines.Pop() as IDisposable)?.Dispose();
        if (DateTime.TryParse(SessionState.GetString(StartedKey, ""), null, DateTimeStyles.RoundtripKind, out DateTime began))
            report.elapsedSeconds = (DateTime.UtcNow - began).TotalSeconds;
        report.passed = report.errors.Count == 0 && report.switchCount == (report.includeSettlement ? 8 : 3) &&
            (!report.includeSettlement || report.pendingSettlementRestoredOnce) && report.productionFilesUnchanged && report.productionSelectionUnchanged;
        SessionState.SetBool(ActiveKey, false);
        if (installed) { PlayerLoop.SetPlayerLoop(originalLoop); installed = false; }
        EditorApplication.update -= Watchdog;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        Application.logMessageReceived -= OnLog;
        try
        {
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "host-integration.json"), JsonUtility.ToJson(report, true));
            Debug.Log("EARNING_VARIANT_INTEGRATION_" + (report.passed ? "PASS" : "FAIL") + " switches=" + report.switchCount + " assertions=" + report.assertions);
        }
        catch (Exception exception) { report.passed = false; Debug.LogError("Cannot save host integration report: " + exception); }
        EditorApplication.Exit(report.passed ? 0 : 1);
    }
}
