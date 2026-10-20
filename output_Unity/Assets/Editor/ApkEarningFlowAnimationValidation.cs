using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Real Play-mode frame fixtures for the saved earning pages. This runs in an empty
/// scene without a gameplay host, account service, player storage, or payment code.
/// Invoke in a separate batch Editor without -quit; this runner exits on completion.
/// </summary>
[InitializeOnLoad]
public static class ApkEarningFlowAnimationValidation
{
    private const string ActiveKey = "Fruit.EarningAnimationValidation.Active";
    private const string OutputKey = "Fruit.EarningAnimationValidation.Output";
    private const string DeadlineKey = "Fruit.EarningAnimationValidation.Deadline";
    private const string StartedKey = "Fruit.EarningAnimationValidation.Started";
    private const string DefaultOutput = "C:/Projects/DJS_Apple/flow-parity-fix-20261020";
    private const string PagePath = "ApkEarningReference/Panels/";
    private const double TimeoutSeconds = 120;
    private static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
    private static Report report;
    private static Fixture fixture;
    private static bool started, finishing, playerLoopInstalled;
    private static int lastFrame = -1;
    private static string output;
    private static PlayerLoopSystem originalPlayerLoop;

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string scope = "Actual Unity Play-mode frames in an isolated empty scene. " +
            "Saved page prefabs, production ApkEarningPageView coroutines, real UGUI Buttons and pointer dispatch. " +
            "The HUD target and page text/status are explicit fixture data, not a player or device capture. " +
            "No gameplay host, earning service, player storage, payment operation or original scene is loaded.";
        public bool passed;
        public int assertions, playFrames;
        public int warmupFrames, verificationFrames;
        public int pointerHitChecks;
        public float largestPlayFrameDelta;
        public float conditionSevenRowsHeight, conditionFourRowsHeight, conditionRestoredHeight;
        public string isolatedScenePath;
        public int isolatedSceneRootCount;
        public double elapsedSeconds;
        public List<string> cases = new List<string>();
        public List<string> screenshots = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    static ApkEarningFlowAnimationValidation()
    {
        if (SessionState.GetBool(ActiveKey, false)) Attach();
    }

    public static void Run()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Use a separate batch Editor: this validation exits its Editor process.");
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Animation validation must start in Edit mode.");
        output = Environment.GetEnvironmentVariable("DJS_APPLE_FLOW_VALIDATION_OUTPUT");
        if (string.IsNullOrWhiteSpace(output)) output = DefaultOutput;
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(output);
        SessionState.SetString(OutputKey, output);
        SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O"));
        SessionState.SetString(DeadlineKey, DateTime.UtcNow.AddSeconds(TimeoutSeconds).ToString("O"));
        SessionState.SetBool(ActiveKey, true);
        Attach();
        try
        {
            // No user scene or serialized object is saved by this fixture.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static void Attach()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeState;
        EditorApplication.playModeStateChanged += OnPlayModeState;
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;
    }

    private static void OnPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(ActiveKey, false))
            InstallPlayerLoop();
        if (!finishing && started && state == PlayModeStateChange.ExitingPlayMode)
            Finish(new InvalidOperationException("Play mode stopped before animation validation completed."));
    }

    private static void Tick()
    {
        if (finishing || !SessionState.GetBool(ActiveKey, false)) return;
        DateTime deadline;
        if (!DateTime.TryParse(SessionState.GetString(DeadlineKey, ""), null,
                System.Globalization.DateTimeStyles.RoundtripKind, out deadline) || DateTime.UtcNow > deadline)
        {
            Finish(new TimeoutException("Animation validation exceeded the 120-second wall-clock limit."));
            return;
        }
        // Editor update is a wall-clock watchdog only. Its scheduling and delayCall
        // callbacks do not share the player coroutine update boundary.
    }

    private static void InstallPlayerLoop()
    {
        if (playerLoopInstalled) return;
        originalPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        for (int i = 0; i < loop.subSystemList.Length; i++)
        {
            if (loop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.Update)) continue;
            var update = loop.subSystemList[i];
            var systems = update.subSystemList;
            Array.Resize(ref systems, systems.Length + 1);
            systems[systems.Length - 1] = new PlayerLoopSystem
            {
                type = typeof(ApkEarningFlowAnimationValidation),
                updateDelegate = TickPlayFrame
            };
            update.subSystemList = systems;
            loop.subSystemList[i] = update;
            PlayerLoop.SetPlayerLoop(loop);
            playerLoopInstalled = true;
            return;
        }
        Finish(new InvalidOperationException("Cannot find the Unity player Update loop."));
    }

    private static void TickPlayFrame()
    {
        if (finishing || !EditorApplication.isPlaying || Time.frameCount == lastFrame) return;
        try
        {
            if (!started)
            {
                started = true;
                EnsureReport();
                var scene = SceneManager.GetActiveScene();
                report.isolatedScenePath = scene.path;
                report.isolatedSceneRootCount = scene.rootCount;
                Require(SceneManager.sceneCount == 1 && scene.rootCount == 0 && string.IsNullOrEmpty(scene.path),
                    "Play mode starts from the isolated empty unsaved scene, without gameplay roots.");
                fixture = new Fixture();
                routines.Push(Cases());
            }
            lastFrame = Time.frameCount;
            report.playFrames++;
            report.largestPlayFrameDelta = Mathf.Max(report.largestPlayFrameDelta, Time.unscaledDeltaTime);
            while (routines.Count != 0)
            {
                var current = routines.Peek();
                if (!current.MoveNext())
                {
                    (current as IDisposable)?.Dispose();
                    routines.Pop();
                    continue;
                }
                if (current.Current is IEnumerator nested) { routines.Push(nested); continue; }
                if (current.Current != null)
                    throw new InvalidOperationException("Unsupported fixture yield; use a frame or nested iterator.");
                return;
            }
            Finish(null);
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static IEnumerator Cases()
    {
        var page = fixture.Show("LocalWithdrawalVerification");
        yield return SettleFrames();
        Button confirm = page.GetButton("SureBtn");
        Require(confirm != null && confirm.interactable, "Saved verification confirm is available before presentation.");
        int clicks = 0;
        confirm.onClick.AddListener(() => clicks++);
        int verificationFirstFrame = Time.frameCount;
        var verification = page.PlayVerificationSteps();
        Require(verification.MoveNext(), "Verification starts across frames.");
        Require(page.IsPresentationPlaying && !page.VerificationReady && !confirm.interactable,
            "Verification disables confirmation while rows are opening.");
        Click(confirm);
        Require(clicks == 0, "Actual Button pointer dispatch cannot confirm during verification.");
        yield return Drain(verification, 10, () =>
        {
            Require(!confirm.interactable, "Confirmation remains disabled throughout verification.");
        });
        Require(page.VerificationReady && !page.IsPresentationPlaying && confirm.interactable,
            "Verification completion releases confirmation exactly after its animation.");
        report.verificationFrames = Time.frameCount - verificationFirstFrame;
        Require(report.verificationFrames >= 5, "Verification is sampled across multiple real player frames.");
        for (int i = 1; i <= 3; i++)
        {
            string key = "path:main/Step/" + i;
            Require(page.GetObject(key + "/Done").activeSelf && !page.GetObject(key + "/Loading").activeSelf,
                "Verification step " + i + " ends done without its loading marker.");
            Require(page.GetObject(key).transform.localScale.y > .99f &&
                    page.GetObject(key + "/Content").transform.localScale.y > .99f,
                "Verification step " + i + " and content finish expanded.");
        }
        Click(confirm);
        Require(clicks == 1, "Actual Button pointer dispatch works after verification.");
        fixture.Capture("animation-verification-complete.png");
        report.cases.Add("verification-progress-button-gate");

        yield return SettleFrames();
        verification = page.PlayVerificationSteps();
        Require(verification.MoveNext(), "Verification can be replayed.");
        yield return AdvanceFor(verification, .65f);
        page.CancelPresentation();
        Vector3 cancelledScale = confirm.transform.localScale;
        bool cancelledDone = page.GetObject("path:main/Step/1/Done").activeSelf;
        Require(!page.IsPresentationPlaying && !page.VerificationReady && confirm.interactable,
            "Explicit cancellation restores interaction without marking verification complete.");
        Require(!verification.MoveNext(), "Cancelled old verification iterator stops.");
        Require(Near(confirm.transform.localScale, cancelledScale) &&
                page.GetObject("path:main/Step/1/Done").activeSelf == cancelledDone,
            "Cancelled old iterator cannot mutate restored page state.");
        verification = page.PlayVerificationSteps();
        Require(verification.MoveNext(), "Verification starts after explicit cancellation.");
        page.gameObject.SetActive(false);
        Require(!page.IsPresentationPlaying && !verification.MoveNext(),
            "Disabling the page cancels its outstanding iterator.");
        page.gameObject.SetActive(true);
        var old = page.PlayVerificationSteps();
        Require(old.MoveNext(), "Verification starts after enabling.");
        var replacement = page.PlayVerificationSteps();
        Require(replacement.MoveNext(), "A new presentation can replace an active one.");
        Require(!old.MoveNext() && page.IsPresentationPlaying && !confirm.interactable,
            "The old iterator cannot cancel the new presentation.");
        page.CancelPresentation();
        Require(!replacement.MoveNext(), "Replacement iterator stops after cancellation.");
        report.cases.Add("verification-cancel-disable-reentry");

        page = fixture.Show("TXProgress0Panel");
        for (int i = 1; i <= 4; i++)
        {
            string key = "path:main/Step/" + i;
            if (i <= 2) page.SetVisible(key + "/Done", i == 1);
            page.SetVisible(key + "/Loading", i != 1);
        }
        yield return SettleFrames();
        confirm = page.GetButton("SureBtn");
        var firstStage = page.PlayStageSteps(false);
        Require(firstStage.MoveNext() && !confirm.interactable, "First stage starts with confirmation disabled.");
        yield return Drain(firstStage, 10);
        Require(page.GetObject("path:main/Step/1").transform.localScale.y > .99f &&
                page.GetObject("path:main/Step/2").transform.localScale.y > .99f &&
                Mathf.Abs(page.GetObject("path:main/Step/3").transform.localScale.y) < .001f &&
                Mathf.Abs(page.GetObject("path:main/Step/4").transform.localScale.y) < .001f,
            "Initial stage reveals only rows one and two before review.");
        Require(!confirm.interactable && confirm.transform.localScale.sqrMagnitude < .001f,
            "Initial stage keeps its confirmation hidden for the review route.");
        fixture.Capture("animation-stage-before-review.png");
        var resumedStage = page.PlayStageSteps(true);
        Require(resumedStage.MoveNext() && !confirm.interactable, "Stage resume remains gated while opening.");
        yield return Drain(resumedStage, 8);
        for (int i = 1; i <= 4; i++)
            Require(page.GetObject("path:main/Step/" + i).transform.localScale.y > .99f,
                "Resumed stage row " + i + " is fully shown.");
        Require(confirm.interactable && confirm.transform.localScale.y > .99f && !page.IsPresentationPlaying,
            "Resumed stage reveals and enables its confirmation.");
        Require(!page.GetObject("path:main/Step/2/Done").activeSelf &&
                page.GetObject("path:main/Step/2/Loading").activeSelf &&
                page.GetObject("path:main/Step/3/Loading").activeSelf,
            "Stage presentation does not fabricate completed eligibility.");
        fixture.Capture("animation-stage-after-review.png");
        report.cases.Add("stage-first-and-resume-with-pending-requirements");

        page = fixture.Show("LocalEarningSequence");
        ShowSequenceGroup(page, "bonusesGroup");
        var bonusScales = new Vector3[5];
        for (int i = 0; i < 5; i++)
        {
            var row = page.GetObject("bonusRows[" + i + "]");
            row.SetActive(true);
            var opacity = row.GetComponent<CanvasGroup>();
            if (opacity != null) opacity.alpha = 1;
            bonusScales[i] = row.transform.localScale;
        }
        yield return SettleFrames();
        bool sawPulse = false;
        var bonuses = page.PlayBonusSequence(0);
        yield return Drain(bonuses, 6, () =>
        {
            for (int i = 0; i < 5; i++)
                if (page.GetObject("bonusRows[" + i + "]").transform.localScale.y > bonusScales[i].y * 1.001f)
                    sawPulse = true;
        });
        Require(sawPulse && !page.IsPresentationPlaying, "Bonuses pulse on actual frames and then complete.");
        for (int i = 0; i < 5; i++)
            Require(Near(page.GetObject("bonusRows[" + i + "]").transform.localScale, bonusScales[i]),
                "Bonus row " + i + " restores its saved scale.");
        fixture.Capture("animation-bonuses-complete.png");
        report.cases.Add("bonus-sequential-pulse-and-restore");

        ShowSequenceGroup(page, "receiptGroup");
        var origins = new Vector3[5];
        var seen = new bool[5];
        var moved = new bool[5];
        for (int i = 0; i < 5; i++)
        {
            var icon = page.GetObject("flightIcons[" + i + "]");
            icon.SetActive(false);
            origins[i] = icon.transform.localPosition;
        }
        yield return SettleFrames();
        bool receiptCaptured = false;
        var receipt = page.PlayReceipt(fixture.ReceiptTarget, 0);
        yield return Drain(receipt, 6, () =>
        {
            for (int i = 0; i < 5; i++)
            {
                var icon = page.GetObject("flightIcons[" + i + "]");
                if (!icon.activeSelf) continue;
                seen[i] = true;
                moved[i] |= !Near(icon.transform.localPosition, origins[i]);
            }
            if (!receiptCaptured && seen[2])
            {
                fixture.Capture("animation-receipt-in-flight.png");
                receiptCaptured = true;
            }
        });
        Require(!page.IsPresentationPlaying, "Receipt presentation completes.");
        for (int i = 0; i < 5; i++)
        {
            var icon = page.GetObject("flightIcons[" + i + "]");
            Require(seen[i] && moved[i], "Receipt icon " + i + " becomes visible and moves.");
            Require(!icon.activeSelf && Near(icon.transform.localPosition, origins[i]),
                "Receipt icon " + i + " is hidden and reset after completion.");
        }
        receipt = page.PlayReceipt(fixture.ReceiptTarget, 0);
        yield return AdvanceFor(receipt, 1.1f);
        page.gameObject.SetActive(false);
        Require(!receipt.MoveNext() && !page.IsPresentationPlaying, "Receipt closes without a continuing old iterator.");
        for (int i = 0; i < 5; i++)
            Require(!page.GetObject("flightIcons[" + i + "]").activeSelf,
                "Cancelled receipt icon " + i + " remains hidden.");
        report.cases.Add("receipt-flight-completion-and-cancel");

        page = fixture.Show("LocalWithdrawalConditions");
        SetConditionRows(page, false);
        yield return SettleFrames();
        Canvas.ForceUpdateCanvases();
        Image card = page.GetImage("main/Image");
        Button action = page.GetButton("action");
        Require(card != null && action != null, "Conditions retain their existing card and action Button bindings.");
        Rect sevenRows = fixture.ScreenBounds(card.rectTransform);
        report.conditionSevenRowsHeight = sevenRows.height;
        Require(sevenRows.height > 0 && fixture.Contains((RectTransform)action.transform),
            "Seven-row cash conditions have a visible card and an action inside the viewport.");
        SetConditionRows(page, true);
        yield return SettleFrames();
        Canvas.ForceUpdateCanvases();
        Rect fourRows = fixture.ScreenBounds(card.rectTransform);
        report.conditionFourRowsHeight = fourRows.height;
        Require(fourRows.height > sevenRows.height * .5f && fourRows.height < sevenRows.height * .85f,
            "Real layout frames shrink the card meaningfully for four diamond rows without collapsing it.");
        Require(Mathf.Abs(fourRows.yMax - sevenRows.yMax) < 1,
            "Changing row count preserves the top-aligned card origin.");
        Require(fixture.Contains((RectTransform)action.transform),
            "The four-row diamond action stays inside the viewport after content shrink.");
        SetConditionRows(page, false);
        yield return SettleFrames();
        Canvas.ForceUpdateCanvases();
        Rect restored = fixture.ScreenBounds(card.rectTransform);
        report.conditionRestoredHeight = restored.height;
        Require(Mathf.Abs(restored.height - sevenRows.height) < 1 &&
                Mathf.Abs(restored.yMax - sevenRows.yMax) < 1,
            "Restoring all seven rows restores the card height and top position within one screen pixel.");
        Require(fixture.Contains((RectTransform)action.transform),
            "The restored seven-row action remains inside the viewport.");
        report.cases.Add("conditions-four-seven-row-layout-reflow");

        page = fixture.Show("TXPanel");
        int historyClicks = 0;
        page.Bind("HistoryButton", () => historyClicks++);
        yield return SettleFrames();
        ClickAtCenter(page.GetButton("HistoryButton"));
        yield return null;
        Require(historyClicks == 1, "The cash history Button receives exactly one center-pointer click.");
        page = fixture.Show("TXCoinPanel");
        var optionClicks = new int[4];
        for (int i = 0; i < optionClicks.Length; i++)
        {
            int option = i;
            page.Bind("options[" + i + "]", () => optionClicks[option]++);
        }
        yield return SettleFrames();
        for (int i = 0; i < optionClicks.Length; i++)
        {
            ClickAtCenter(page.GetButton("options[" + i + "]"));
            yield return null;
            for (int other = 0; other < optionClicks.Length; other++)
                Require(optionClicks[other] == (other <= i ? 1 : 0),
                    "Clicking diamond option " + i + " fires only its own callback, once; observed option " + other + ".");
        }
        report.cases.Add("wallet-history-and-diamond-options-pointer-hit");
    }

    private static void SetConditionRows(ApkEarningPageView page, bool diamond)
    {
        page.SetText("title", "Withdrawal requirements");
        page.SetText("subtitle", diamond ? "Diamond withdrawal" : "Cash withdrawal");
        page.SetText("notice", "Complete the requirements below to continue.");
        page.SetText("actionCaption", "Continue playing");
        string[] names = diamond
            ? new[] { "Required balance", "Levels completed today", "Login days", "Account level" }
            : new[] { "Complete level 22", "Complete level 51", "Required balance", "Complete cash guidance",
                "Levels completed today", "Login days", "Account level" };
        string[] values = diamond ? new[] { "355.11 / 50,000", "6 / 10", "2 / 5", "0 / 31" }
            : new[] { "6 / 22 boards", "6 / 51 boards", "Awaiting account update", "0 / 1", "6 / 10", "2 / 5", "0 / 31" };
        for (int i = 0; i < 7; i++)
        {
            string key = "rows[" + i + "]";
            GameObject row = page.GetObject(key + ".Root");
            Require(row != null, "Conditions retain row binding " + i + ".");
            page.SetVisible(key + ".Root", i < names.Length);
            if (i >= names.Length) continue;
            page.SetText(key + ".Name", names[i]);
            page.SetText(key + ".Value", values[i]);
            page.SetText(key + ".Status", "In progress");
        }
    }

    private static IEnumerator Drain(IEnumerator routine, float maximumSeconds, Action observe = null)
    {
        float began = Time.realtimeSinceStartup;
        try
        {
            // The caller may just have started this iterator. Never consume the same
            // player frame's delta a second time at a nested iterator boundary.
            yield return null;
            while (routine.MoveNext())
            {
                if (Time.realtimeSinceStartup - began > maximumSeconds)
                    throw new TimeoutException("A presentation exceeded its per-case timeout.");
                observe?.Invoke();
                yield return null;
            }
        }
        finally { (routine as IDisposable)?.Dispose(); }
    }

    private static IEnumerator AdvanceFor(IEnumerator routine, float seconds)
    {
        float began = Time.realtimeSinceStartup;
        yield return null;
        while (Time.realtimeSinceStartup - began < seconds)
        {
            Require(routine.MoveNext(), "Presentation remains live during its partial-frame sample.");
            yield return null;
        }
    }

    private static IEnumerator SettleFrames()
    {
        float began = Time.realtimeSinceStartup;
        int stable = 0;
        while (stable < 5)
        {
            yield return null;
            report.warmupFrames++;
            // Resource imports, Editor delayCall work and PNG encoding are fixture
            // setup, not animation samples. Wait for real frames instead of replacing
            // or clamping the delta consumed by production code.
            stable = Time.unscaledDeltaTime > 0 && Time.unscaledDeltaTime < .1f ? stable + 1 : 0;
            if (Time.realtimeSinceStartup - began > 20)
                throw new TimeoutException("Player frames did not stabilize after fixture setup.");
        }
    }

    private static void ShowSequenceGroup(ApkEarningPageView page, string visible)
    {
        page.SetVisible("messageGroup", visible == "messageGroup");
        page.SetVisible("bonusesGroup", visible == "bonusesGroup");
        page.SetVisible("receiptGroup", visible == "receiptGroup");
    }

    private static void Click(Button button)
    {
        var pointer = new PointerEventData(fixture.Events) { button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }
    private static void ClickAtCenter(Button button)
    {
        Require(button != null && button.isActiveAndEnabled && button.IsInteractable(),
            "The requested wallet control is an active interactable standard Button.");
        Canvas.ForceUpdateCanvases();
        Require(fixture.Contains((RectTransform)button.transform),
            "The requested wallet Button is wholly inside the fixture viewport: " + button.name);
        var pointer = new PointerEventData(fixture.Events) {
            pointerId = -1, button = PointerEventData.InputButton.Left,
            position = fixture.ScreenBounds((RectTransform)button.transform).center,
            clickCount = 1, eligibleForClick = true
        };
        var hits = new List<RaycastResult>();
        fixture.Events.RaycastAll(pointer, hits);
        Require(hits.Count > 0, "EventSystem raycast finds a Graphic at the Button center: " + button.name);
        GameObject hit = hits[0].gameObject;
        GameObject clicked = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit);
        Require(clicked == button.gameObject,
            "The frontmost Graphic routes its center click to the expected Button " + button.name +
            "; frontmost hit=" + hit.name + ", handler=" + (clicked == null ? "none" : clicked.name) + ".");
        report.pointerHitChecks++;
        pointer.pointerCurrentRaycast = pointer.pointerPressRaycast = hits[0];
        pointer.pressPosition = pointer.position;
        pointer.rawPointerPress = hit;
        // Resolve from the actual raycast winner rather than dispatching directly
        // to a requested Button; a newly blocking sibling must fail this test.
        pointer.pointerPress = ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerDownHandler);
        Require(pointer.pointerPress == clicked, "Pointer down resolves to the same Button as the center click.");
        ExecuteEvents.Execute(pointer.pointerPress, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(clicked, pointer, ExecuteEvents.pointerClickHandler);
        pointer.pointerPress = pointer.rawPointerPress = null;
        pointer.eligibleForClick = false;
    }
    private static bool Near(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < .00001f;
    private static void Require(bool value, string message)
    {
        report.assertions++;
        if (!value) throw new InvalidOperationException(message);
    }
    private static void EnsureReport()
    {
        if (report == null) report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
        if (string.IsNullOrEmpty(output)) output = SessionState.GetString(OutputKey, DefaultOutput);
    }
    private static void OnLog(string message, string stack, LogType type)
    {
        if (!SessionState.GetBool(ActiveKey, false) || finishing) return;
        EnsureReport();
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            report.errors.Add(message);
        else if (type == LogType.Warning) report.warnings.Add(message);
    }
    private static void Finish(Exception failure)
    {
        if (finishing) return;
        finishing = true;
        EnsureReport();
        if (failure != null) report.errors.Add(failure.ToString());
        try
        {
            while (routines.Count != 0) (routines.Pop() as IDisposable)?.Dispose();
            fixture?.Dispose();
        }
        catch (Exception cleanup) { report.errors.Add("Fixture cleanup: " + cleanup); }
        DateTime began;
        if (DateTime.TryParse(SessionState.GetString(StartedKey, ""), null,
                System.Globalization.DateTimeStyles.RoundtripKind, out began))
            report.elapsedSeconds = (DateTime.UtcNow - began).TotalSeconds;
        report.passed = report.errors.Count == 0 && report.cases.Count == 7;
        SessionState.SetBool(ActiveKey, false);
        if (playerLoopInstalled)
        {
            PlayerLoop.SetPlayerLoop(originalPlayerLoop);
            playerLoopInstalled = false;
        }
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeState;
        Application.logMessageReceived -= OnLog;
        try
        {
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "animation-regression.json"), JsonUtility.ToJson(report, true));
            Debug.Log("APK_EARNING_ANIMATION_" + (report.passed ? "PASS" : "FAIL") +
                " cases=" + report.cases.Count + " assertions=" + report.assertions);
        }
        catch (Exception writeError)
        {
            report.passed = false;
            Debug.LogError("Unable to write animation validation report: " + writeError);
        }
        EditorApplication.Exit(report.passed ? 0 : 1);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly GameObject root;
        private readonly Camera camera;
        private readonly Canvas canvas;
        private readonly RenderTexture texture;
        private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private ApkEarningPageView current;
        public EventSystem Events { get; }
        public Transform ReceiptTarget { get; }
        public Fixture()
        {
            foreach (string name in new[] { "LocalWithdrawalVerification", "TXProgress0Panel", "LocalEarningSequence",
                "LocalWithdrawalConditions", "TXPanel", "TXCoinPanel" })
            {
                var prefab = Resources.Load<GameObject>(PagePath + name);
                Require(prefab != null, "Saved fixture prefab exists: " + name);
                Require(prefab.GetComponentInChildren<ApkEarningHost>(true) == null,
                    "The isolated page prefab contains no earning Host: " + name);
                prefabs.Add(name, prefab);
            }
            root = new GameObject("Earning animation validation fixture");
            var cameraObject = new GameObject("Fixture camera");
            cameraObject.transform.SetParent(root.transform, false);
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 960;
            camera.transform.position = new Vector3(0, 0, -100);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 200;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(24, 38, 45, 255);
            texture = new RenderTexture(1080, 1920, 0);
            texture.Create();
            camera.targetTexture = texture;
            camera.aspect = 1080f / 1920;
            var canvasObject = new GameObject("Fixture canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 100;
            ((RectTransform)canvas.transform).sizeDelta = new Vector2(1080, 1920);
            Events = new GameObject("Fixture EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            Events.transform.SetParent(root.transform, false);
            var target = new GameObject("Fixture receipt HUD target", typeof(RectTransform));
            target.transform.SetParent(canvas.transform, false);
            var targetRect = (RectTransform)target.transform;
            targetRect.anchorMin = targetRect.anchorMax = new Vector2(.5f, 1);
            targetRect.anchoredPosition = new Vector2(-250, -60);
            targetRect.sizeDelta = new Vector2(80, 80);
            ReceiptTarget = targetRect;
        }
        public ApkEarningPageView Show(string name)
        {
            if (current != null) Object.DestroyImmediate(current.gameObject);
            GameObject prefab;
            prefabs.TryGetValue(name, out prefab);
            Require(prefab != null, "Saved prefab exists: " + name);
            current = Object.Instantiate(prefab, canvas.transform, false).GetComponent<ApkEarningPageView>();
            Require(current != null, "Saved page binding exists: " + name);
            var rect = (RectTransform)current.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            current.RefreshLayout();
            Canvas.ForceUpdateCanvases();
            return current;
        }
        public void Capture(string name)
        {
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var previous = RenderTexture.active;
            var image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(output, name), image.EncodeToPNG());
                report.screenshots.Add(name);
            }
            finally
            {
                RenderTexture.active = previous;
                Object.DestroyImmediate(image);
            }
        }
        public Rect ScreenBounds(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = camera.WorldToScreenPoint(corners[0]), max = min;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 point = camera.WorldToScreenPoint(corners[i]);
                min = Vector2.Min(min, point); max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        public bool Contains(RectTransform rect)
        {
            Rect bounds = ScreenBounds(rect);
            return bounds.width > 0 && bounds.height > 0 &&
                bounds.xMin >= -1 && bounds.yMin >= -1 &&
                bounds.xMax <= texture.width + 1 && bounds.yMax <= texture.height + 1;
        }
        public void Dispose()
        {
            if (camera != null) camera.targetTexture = null;
            if (root != null) Object.DestroyImmediate(root);
            if (texture != null) { texture.Release(); Object.DestroyImmediate(texture); }
        }
    }
}
